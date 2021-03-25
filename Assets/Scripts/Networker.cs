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
    [ReadOnly, ShowInInspector] public int port;

    Lobby lobby;
    Latency latency;
    Player host;
    Player? player;

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
    
    ConcurrentQueue<Action> mainTreadActions = new ConcurrentQueue<Action>();

    private void Awake() => DontDestroyOnLoad(gameObject);

    private void Update() {
        while(mainTreadActions.TryDequeue(out Action a))
            a.Invoke();
        
        // To track latency, every pingDelay seconds we ping the socket, we expect back a pong in a timely fashion
        if(Time.timeSinceLevelLoad >= pingAtTime && stream != null && pongReceived)
        {
            byte[] ping = new Message(MessageType.Ping).Serialize();
            stream.Write(ping, 0, ping.Length);
            pingedAtTime = Time.timeSinceLevelLoad;
            pingAtTime = Time.timeSinceLevelLoad + pingDelay;
            pongReceived = false;
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
        else if(client.Connected)
        {
            // Write disconnect to socket
            byte[] disconnectMessage = new Message(MessageType.Disconnect).Serialize();
            stream.Write(disconnectMessage, 0, disconnectMessage.Length);
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
        lobby?.SetIP(ip, port);

        if(!isHost)
            lobby?.SpawnPlayer(player.Value);

        SceneManager.sceneLoaded -= LoadLobby;
    }

    // Server
    public void Host()
    {
        isHost = true;
        
        // Eventually need to fetch the host's IP here
        ip = "127.0.0.1";
        port = 8080;

        host = new Player("Host", Team.White, true);
        
        server = new TcpListener(IPAddress.Parse(ip), port);

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
        
        mainTreadActions.Enqueue(() => {
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

        networker.mainTreadActions.Enqueue(() => {
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
                    networker.mainTreadActions.Enqueue(() => Shutdown());
                }
                else
                {
                    Debug.Log("The player disconnected.");
                    networker.mainTreadActions.Enqueue(() => PlayerDisconnected());
                    return;
                }
            }
            else
            {
                // process incoming message
                byte[] readBytes = new byte[amountOfBytesRead];
                Buffer.BlockCopy(networker.readBuffer, 0, readBytes, 0, amountOfBytesRead);
                networker.readQueue.Enqueue(readBytes);
                networker.mainTreadActions.Enqueue(() => networker.CheckCompleteMessage());                
            }
            
            // Wait for next message
            networker.stream.BeginRead(networker.readBuffer, 0, messageMaxSize, new AsyncCallback(ReceiveMessage), networker);
            
        } catch (IOException e) {
            Debug.Log($"The socket was closed.\n{e}");
            networker.mainTreadActions.Enqueue(() => Shutdown());
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
                            Dispatch(ref completeMessage);
                        }
                    }
                }
                // Maybe only remove mySignature.Length and check the next set of bytes?
                // This chould endup looking through a lot of trash though
                else
                    readMessageFragment.Clear();
            }
        }
    }

    private void Dispatch(ref Message completeMessage)
    {
        switch(completeMessage.type)
        {
            case MessageType.Disconnect:
                if(!isHost)
                    Shutdown();
                else
                    PlayerDisconnected();
                break;
            case MessageType.Ping:
                byte[] pong = new Message(MessageType.Pong).Serialize();
                stream.Write(pong, 0, pong.Length);
                break;
            case MessageType.Pong:
                pongReceived = true;
                mainTreadActions.Enqueue(() => {
                    if(latency == null)
                        latency = GameObject.FindObjectOfType<Latency>();
                    latency?.UpdateLatency(Time.timeSinceLevelLoad - pingedAtTime);
                });
                break;
            default:
                break;
        }
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
            // server.BeginAcceptTcpClient(new AsyncCallback(AcceptClientCallback), server);
        }
    }
}