using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Networker : MonoBehaviour
{
    [ReadOnly, ShowInInspector] public bool isHost {get; private set;} = false;
    [ReadOnly, ShowInInspector] public string ip;
    [ShowInInspector] public int port;

    Lobby lobby;
    Multiplayer multiplayer;
    Latency latency;
    QueryTeamChangePanel teamChangePanel;
    [ReadOnly, ShowInInspector] public Player host;
    [ReadOnly, ShowInInspector] public Player? player;

    TcpListener server;
    TcpClient client;
    NetworkStream stream;

    public int messageMaxSize = 1024;
    public float pingDelay = 2f;
    float pingAtTime;
    float pingedAtTime;
    bool pongReceived = true;

    byte[] readBuffer;
    ConcurrentQueue<byte[]> readQueue = new ConcurrentQueue<byte[]>();
    List<byte> readMessageFragment = new List<byte>();
    
    ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    public bool clientIsReady {get; private set;}
    GameParams gameParams;
    private void Awake()
    {
        List<Networker> networkers = GameObject.FindObjectsOfType<Networker>().ToList();
        networkers = networkers.Where(networker => networker != this).ToList();
        for(int i = networkers.Count() - 1; i >= 0; i--)
            Destroy(networkers[i].gameObject);
            
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        while(mainThreadActions.TryDequeue(out Action a))
            a.Invoke();
        
        // To track latency, every pingDelay seconds we ping the socket, we expect back a pong in a timely fashion
        if(Time.timeSinceLevelLoad >= pingAtTime && stream != null && pongReceived)
        {
            byte[] ping = new Message(MessageType.Ping).Serialize();
            try {
                stream.Write(ping, 0, ping.Length);
                pingedAtTime = Time.timeSinceLevelLoad;
                pingAtTime = Time.timeSinceLevelLoad + pingDelay;
                pongReceived = false;
            } catch (Exception e) {
                Debug.LogWarning($"Failed to write ping to socket with error:\n{e}");
            }
        }
    }

    public void Shutdown()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        Destroy(gameObject);
    }

    private void OnDestroy() => Disconnect();

    private void Disconnect()
    {
        if(isHost)
            server?.Stop();
        else if(client != null && client.Connected)
        {
            // Write disconnect to socket
            byte[] disconnectMessage = new Message(MessageType.Disconnect).Serialize();
            try {
                stream.Write(disconnectMessage, 0, disconnectMessage.Length);
            } catch (Exception e) {
                Debug.LogWarning($"Failed to write to socket with error:\n{e}");
            }
        }

        stream?.Close();
        client?.Close();
        Debug.Log($"Disconnected.");
    }

    private void LoadLobby(Scene scene, LoadSceneMode loadMode)
    {
        if(scene.name != "Lobby")
            return;

        if(lobby == null)
            lobby = GameObject.FindObjectOfType<Lobby>();
        
        lobby?.SpawnPlayer(host);
        lobby?.SetIP(isHost ? GetPublicIPAddress() : $"{ip}", port);

        if(!isHost)
            lobby?.SpawnPlayer(player.Value);

        SceneManager.sceneLoaded -= LoadLobby;
    }

    static string GetPublicIPAddress()
    {
        String address = "";
        WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
        using (WebResponse response = request.GetResponse())
        using (StreamReader stream = new StreamReader(response.GetResponseStream()))
        {
            address = stream.ReadToEnd();
        }

        //Search for the ip in the html
        int first = address.IndexOf("Address: ") + 9;
        int last = address.LastIndexOf("</body></html>");
        address = address.Substring(first, last - first);

        return address;
    }

    // Server
    public void Host()
    {
        isHost = true;

        host = new Player("Host", Team.White, true);
        server = new TcpListener(IPAddress.Any, port);

        try{
            server.Start();
            server.BeginAcceptTcpClient(new AsyncCallback(AcceptClientCallback), server);
        }
        catch (Exception e) {
            Debug.LogWarning($"Failed to host on {ip}:{port} with error:\n{e}");
        }

        // Load to lobby
        SceneManager.sceneLoaded += LoadLobby;
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    private void AcceptClientCallback(IAsyncResult ar)
    {
        TcpListener server = (TcpListener)ar.AsyncState;
        if(server == null)
            return;
        
        try{
            TcpClient incommingClient = server.EndAcceptTcpClient(ar);
            if(client == null)
            {
                client = incommingClient;
                stream = client.GetStream();
            }
            // In chess, there is only ever 2 players, (1 host, 1 player), so reject any connection trying to come in if the player slot is already full
            else
                incommingClient.Close();
        } catch (Exception e) {
            Debug.LogWarning($"Failed to connect to incoming client:\n{e}");
            return;
        }

        Debug.Log($"Connected to incoming client: {client.IP()}.");
        
        mainThreadActions.Enqueue(() => {
            if(lobby == null)
                lobby = GameObject.FindObjectOfType<Lobby>();

            player = new Player($"{client.IP()}", Team.Black, false);
            lobby?.SpawnPlayer(player.Value);
        });
        
        readBuffer = new byte[messageMaxSize];

        try {
            stream.BeginRead(readBuffer, 0, messageMaxSize, new AsyncCallback(ReceiveMessage), this);
        } catch (Exception e) {
            Debug.LogWarning($"Failed to read from socket:\n{e}");
        }
   
        try {
            server.BeginAcceptTcpClient(new AsyncCallback(AcceptClientCallback), server);
        } catch (Exception e) {
            Debug.LogWarning($"Failed to connect to incoming client:\n{e}");
        }
    }

    // Client
    public void TryConnectClient(string ip, int port)
    {
        this.ip = ip;
        this.port = port;

        Debug.Log($"Attempting to connect to {ip}:{port}.");
        client = new TcpClient();
        try{
            client.BeginConnect(IPAddress.Parse(ip), port, new AsyncCallback(ClientConnectCallback), this);
        } catch (Exception e) {
            Debug.LogWarning($"Failed to connect to {ip}:{port} with error:\n{e}");
        }
    }

    private void ClientConnectCallback(IAsyncResult ar)
    {
        Networker networker = (Networker)ar.AsyncState;
        if(networker == null)
            return;
        
        try{
            networker.client.EndConnect(ar);
            networker.stream = networker.client.GetStream();
        } catch (Exception e) {
            Debug.LogWarning($"Failed to connect with error:\n{e}");
            return;
        }
        Debug.Log("Sucessfully connected.");
        
        host = new Player("Host", Team.White, true);
        player = new Player($"{client.IP()}", Team.Black, false);

        networker.mainThreadActions.Enqueue(() => {
            SceneManager.sceneLoaded += LoadLobby;
            SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        });

        networker.readBuffer = new byte[messageMaxSize];
        try {
            networker.stream.BeginRead(networker.readBuffer, 0, messageMaxSize, new AsyncCallback(ReceiveMessage), networker);
        } catch (Exception e) {
            Debug.LogWarning($"Failed to read from socket:\n{e}");
        }
    }

    // Both client + server
    private void ReceiveMessage(IAsyncResult ar)
    {
        Networker networker = (Networker)ar.AsyncState;
        if(networker == null)
            return;

        try {
            int amountOfBytesRead = networker.stream.EndRead(ar);

            if(amountOfBytesRead == 0)
            {
                if(!isHost)
                {
                    Debug.Log("The host closed the socket.");
                    if(lobby != null)
                        networker.mainThreadActions.Enqueue(Shutdown);
                    else if(multiplayer != null)
                    {
                        networker.mainThreadActions.Enqueue(() => multiplayer.Surrender(host.team)); 
                        networker.Disconnect();
                    }
                }
                else
                {
                    Debug.Log("The player disconnected.");
                    networker.mainThreadActions.Enqueue(PlayerDisconnected);
                    return;
                }
            }
            else
            {
                // process incoming message
                byte[] readBytes = new byte[amountOfBytesRead];
                Buffer.BlockCopy(networker.readBuffer, 0, readBytes, 0, amountOfBytesRead);
                networker.readQueue.Enqueue(readBytes);
                networker.mainThreadActions.Enqueue(networker.CheckCompleteMessage);                
            }
            
            // Wait for next message
            networker.stream.BeginRead(networker.readBuffer, 0, messageMaxSize, new AsyncCallback(ReceiveMessage), networker);
            
        } catch (IOException e) {
            Debug.Log($"The socket was closed.\n{e}");
            networker.mainThreadActions.Enqueue(Shutdown);
        }
        catch (Exception e) {
            Debug.LogWarning($"Failed to read from socket:\n{e}");
        }
    }

    private void CheckCompleteMessage()
    {
        if(readQueue.TryDequeue(out byte[] result))
        {
            readMessageFragment.AddRange(result);
            Span<byte> mySignature = Message.GetSignature();

            if(readMessageFragment.Count >= mySignature.Length)
            {
                byte[] messageSig = readMessageFragment.Take(mySignature.Length).ToArray();
                // If the message came from another copy of this game and not some rando in the internet cesspool, it should have a matching signature
                // This doesn't prevent targeted attacks, but it's a good way to eliminate a lot of garbage
                if(mySignature.SequenceEqual(new ReadOnlySpan<byte>(messageSig)))
                {
                    // The first 2 bytes should be the message length
                    if(readMessageFragment.Count >= mySignature.Length + 2)
                    {
                        byte[] messageLength = readMessageFragment.Skip(mySignature.Length).Take(2).ToArray();
                        ushort dataLength = BitConverter.ToUInt16(messageLength, 0);
                        // The 3rd byte should be the type
                        int copleteMessageLength = mySignature.Length + 3 + dataLength;
                        if(readMessageFragment.Count >= copleteMessageLength)
                        {
                            // The incomming message matches our format
                            Message completeMessage = new Message(
                                signature: messageSig,
                                type: (MessageType)readMessageFragment.Skip(mySignature.Length + 2).First(),
                                data: readMessageFragment.Skip(mySignature.Length + 3).Take(dataLength).ToArray()
                            );

                            readMessageFragment.RemoveRange(0, copleteMessageLength);
                            Dispatch(completeMessage);
                        }
                    }
                }
                // Maybe only remove mySignature.Length and check the next set of bytes?
                // This could end up looking through a lot of trash though
                else
                    readMessageFragment.Clear();
            }
        }
    }

    private void Dispatch(Message completeMessage)
    {
        Action action = completeMessage.type switch {
            MessageType.Disconnect when isHost => PlayerDisconnected,
            MessageType.Disconnect when !isHost => Shutdown,
            MessageType.Ping => () => {
                // All pings should get a pong in response
                byte[] pong = new Message(MessageType.Pong).Serialize();
                try {
                    stream.Write(pong, 0, pong.Length);
                } catch (Exception e) {
                    Debug.LogWarning($"Failed to write to socket with error:\n{e}");
                }
            },
            MessageType.Pong => () => {
                // Measure and update latency when pong received
                pongReceived = true;
                mainThreadActions.Enqueue(() => {
                    if(latency == null)
                        latency = GameObject.FindObjectOfType<Latency>();
                    latency?.UpdateLatency(Time.timeSinceLevelLoad - pingedAtTime);
                });
            },
            MessageType.ProposeTeamChange => ReceiveTeamChangeProposal,
            MessageType.ApproveTeamChange => () => mainThreadActions.Enqueue(SwapTeams),
            MessageType.Ready when isHost => Ready,
            MessageType.Unready when isHost => Unready,
            MessageType.PreviewMovesOn when lobby => PreviewOn,
            MessageType.PreviewMovesOff when lobby => PreviewOff,
            MessageType.StartMatch when !isHost => StartMatch,
            MessageType.Surrender => () => multiplayer?.Surrender(isHost ? player.Value.team : host.team),
            MessageType.BoardState => () => multiplayer?.UpdateBoard(BoardState.Deserialize(completeMessage.data)),
            MessageType.Promotion => () => multiplayer?.ReceivePromotion(Promotion.Deserialize(completeMessage.data)),
            _ => null
        };

        action?.Invoke();
    }

    private void PreviewOn()
    {
        PreviewMovesToggle previewToggle = GameObject.FindObjectOfType<PreviewMovesToggle>();
        if (previewToggle != null)
            previewToggle.toggle.isOn = true;
    }
    private void PreviewOff()
    {
        PreviewMovesToggle previewToggle = GameObject.FindObjectOfType<PreviewMovesToggle>();
        if (previewToggle != null)
            previewToggle.toggle.isOn = false;
    }

    public void SendMessage(Message message)
    {
        byte[] messageData = message.Serialize();
        stream?.Write(messageData, 0, messageData.Length);
    }

    private void PlayerDisconnected()
    {
        if(lobby == null)
            lobby = GameObject.FindObjectOfType<Lobby>();

        if(lobby != null && player.HasValue)
        {
            lobby.RemovePlayer(player.Value);
            player = null;
            client = null;

            if(host.team == Team.Black)
            {
                lobby.RemovePlayer(host);
                host.team = Team.White;
                lobby.SpawnPlayer(host);
            }

            if(clientIsReady)
                Unready();
        }

        if(teamChangePanel != null && teamChangePanel.isOpen)
            teamChangePanel.Close();
        
        // For now, assume a loss when the player disconnects, later we should wait for a potential reconnect
        multiplayer?.Surrender(player.Value.team);
    }

    public void ProposeTeamChange()
    {
        if(client == null)
            return;

        byte[] ProposeTeamChangeMessage = new Message(MessageType.ProposeTeamChange).Serialize();
        try {
            stream.Write(ProposeTeamChangeMessage, 0, ProposeTeamChangeMessage.Length);
        } catch (Exception e) {
            Debug.LogWarning($"Failed to write to socket with error:\n{e}");
        }
    }

    private void ReceiveTeamChangeProposal()
    {
        mainThreadActions.Enqueue(() =>
        {
            if(teamChangePanel == null)
                teamChangePanel = GameObject.FindObjectOfType<QueryTeamChangePanel>();
            teamChangePanel?.Query();
        });

        if(!isHost)
        {
            ReadyButton ready = GameObject.FindObjectOfType<ReadyButton>();
            if(ready != null && ready.toggle.isOn)
                ready.toggle.isOn = false;
        }
    }

    public void RespondToTeamChange(MessageType answer)
    {
        lobby = GameObject.FindObjectOfType<Lobby>();
        if(lobby == null)
            return;

        byte[] response = new Message(answer).Serialize();
        try {
            stream.Write(response, 0, response.Length);
        } catch (Exception e) {
            Debug.LogWarning($"Failed to write to socket with error:\n{e}");
        }
        
        if(answer == MessageType.ApproveTeamChange)
            SwapTeams();
    }

    public void SwapTeams()
    {
        if(lobby == null)
            lobby = GameObject.FindObjectOfType<Lobby>();

        if(!player.HasValue && lobby == null)
            return;

        lobby?.RemovePlayer(host);
        lobby?.RemovePlayer(player.Value);

        Team hostTeam = host.team;
        Player playerModified = player.Value;
        host.team = playerModified.team;
        playerModified.team = hostTeam;
        player = playerModified;

        lobby?.SpawnPlayer(host);
        lobby?.SpawnPlayer(player.Value);
    }

    private void Ready()
    {
        clientIsReady = true;

        StartMatchButton startButton = GameObject.FindObjectOfType<StartMatchButton>();
        startButton?.ShowButton();
    }

    private void Unready()
    {
        clientIsReady = false;

        StartMatchButton startButton = GameObject.FindObjectOfType<StartMatchButton>();
        startButton?.HideButton();
    }

    public void StartMatch()
    {
        if(isHost)
            SendMessage(new Message(MessageType.StartMatch));

        PreviewMovesToggle previewToggle = GameObject.FindObjectOfType<PreviewMovesToggle>();
        gameParams = new GameParams(isHost ? host.team : player.Value.team, previewToggle.toggle.isOn);
        
        // Load scene
        SceneManager.LoadScene("VersusMode");
        SceneManager.activeSceneChanged += SendParams;
    }

    private void SendParams(Scene arg0, Scene arg1)
    {
        multiplayer = GameObject.FindObjectOfType<Multiplayer>();
        lobby = null;
        multiplayer?.SetupGame(gameParams);
        SceneManager.activeSceneChanged -= SendParams;
    }
}