using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Extensions;
using System;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    public enum Type {None = 0, Host = 1, Client = 2, AI = 3}
    [SerializeField] private GroupFader fader;
    [SerializeField] private GroupFader connectionChoiceFader;
    [SerializeField] private GroupFader whiteLocalIconFader;
    [SerializeField] private GroupFader blackLocalIconFader;
    [SerializeField] private GroupFader opponentSearchingPanel;
    [SerializeField] private TextMeshProUGUI opponentSearchingText;
    [SerializeField] public IPPanel ipPanel;
    [SerializeField] public ReadyButton readyToggle;
    [SerializeField] public StartMatchButton startButton;
    [SerializeField] private TextMeshProUGUI readyButtonContextText;
    [SerializeField] private TextMeshProUGUI proposeTeamChangeText;
    [SerializeField] private TextMeshProUGUI youPlayTeamText;

    [SerializeField] private GroupFader opponentLoadingFader;
    [SerializeField] private GroupFader opponentNameFader;
    [SerializeField] private GroupFader blackOpponentIconFader;
    [SerializeField] private GroupFader whiteOpponentIconFader;
    [SerializeField] private GroupFader readyFader;

    [SerializeField] private TextMeshProUGUI opponentName;

    public Toggle clockToggle;
    [SerializeField] private GameObject clockObj;
    [SerializeField] private TextMeshProUGUI clockText;

    public Toggle timerToggle;
    [SerializeField] private GameObject timerObj;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TMP_InputField timerInputField;

    [SerializeField] private QueryTeamChangePanel teamChangePanel;
    [SerializeField] private HandicapOverlayToggle handicapOverlayToggle;

    public Color toggleOnColor;
    public Color toggleOffColor;

    private bool opponentSearching = true;

    public Type lobbyType {get; private set;} = Type.None;

    private Team AITeam = Team.None;

    private void Awake()
    {
        timerInputField.gameObject.SetActive(false);

        ListenForClock();
        ListenForTimer();
    }

    private void ListenForClock()
    {
        clockToggle.onValueChanged.AddListener((isOn) =>
        {
            SetToggleColor(clockToggle, isOn);

            if(isOn)
            {
                timerToggle.isOn = false;
                clockText.text = "Toggle Clock (On)";
            }
            else
                clockText.text = "Toggle Clock (Off)";

            EventSystem.current.Deselect();
        });
    }

    private void ListenForTimer()
    {
        timerToggle.onValueChanged.AddListener((isOn) =>
        {
            SetToggleColor(timerToggle, isOn);

            if(isOn)
            {
                clockToggle.isOn = false;
                timerText.rectTransform.sizeDelta = new Vector2(150, timerText.rectTransform.sizeDelta.y);
                timerText.text = "Timer (mins)";
                timerInputField.gameObject.SetActive(true);
                timerInputField.text = "20";
            }
            else
            {
                timerText.rectTransform.sizeDelta = new Vector2(223, timerText.rectTransform.sizeDelta.y);
                timerText.text = "Toggle Timer (off)";
                timerInputField.gameObject.SetActive(false);
            }

            EventSystem.current.Deselect();
        });
    }

    private void SetToggleColor(Toggle toggle, bool isOn)
    {
        ColorBlock block = toggle.colors;
        block.normalColor = GetToggleColor(isOn);
        block.selectedColor = block.normalColor;
        block.disabledColor = block.normalColor;
        toggle.colors = block;
    }

    public void Show(Type lobbyType)
    {
        this.lobbyType = lobbyType;

        Action lobbyAction = lobbyType switch {
            Type.Host => () => {
                OpponentSearching();
                timerObj.SetActive(true);
                clockObj.SetActive(true);
            },
            Type.Client => () => {
                opponentSearching = true;
                SetSearchingText();
                timerObj.SetActive(false);
                clockObj.SetActive(false);
            },
            Type.AI => () => {
                SetAIPanel();
                startButton.ShowEnabledButton();
                timerObj.SetActive(true);
                clockObj.SetActive(true);
            },
            _ => null
        };
        lobbyAction?.Invoke();

        connectionChoiceFader?.FadeOut();
        fader.FadeIn();
    }

    public void Hide()
    {
        lobbyType = Type.None;
        connectionChoiceFader?.FadeIn();
        fader.FadeOut();
    } 

    public Color GetToggleColor(bool isOn) => isOn ? toggleOnColor : toggleOffColor;
    public float GetTimeInSeconds() => int.Parse(timerInputField.text) * 60;

    public void RemovePlayer(Player player)
    {
        if(!player.isHost)
            opponentSearchingPanel.FadeIn();

        opponentSearching = true;

        SetSearchingText();
        opponentLoadingFader.FadeIn();

        opponentNameFader.FadeOut();
        blackOpponentIconFader.FadeOut();

        opponentName.text = "";
    }

    public void DisconnectRecieved()
    {
        if(teamChangePanel != null && teamChangePanel.isOpen)
            teamChangePanel.Close();

        opponentSearching = true;

        readyButtonContextText.text = "";
        ResetToSearchingPanel();

        if(lobbyType == Type.Host)
            startButton.HideButton();
        else if(lobbyType == Type.Client)
        {
            readyToggle?.Hide();
            readyToggle?.gameObject.SetActive(false);
        }
    }

    public void LoadAIGame()
    {
        SceneManager.activeSceneChanged += StartAIGame;
        SceneTransition transition = GameObject.FindObjectOfType<SceneTransition>();
        if(transition != null)
            transition.Transition("SandboxMode");
        else
            SceneManager.LoadScene("SandboxMode");
    }

    public void StartAIGame(Scene arg0, Scene arg1)
    {
        AIBattleController aiController = GameObject.FindObjectOfType<AIBattleController>();
        aiController.SetAI(AITeam, 2);
        aiController.SetAI(AITeam.Enemy(), 6);
        aiController.StartGame();

        SceneManager.activeSceneChanged -= StartAIGame;
    }


    public void UpdatePlayerName(Player player)
    {
        opponentName.text = player.name;
    } 

    public void UpdateTeam(Player player)
    {
        // local
        if((player.isHost && lobbyType == Type.Host) || (!player.isHost && lobbyType == Type.Client))
            SetLocalTeam(player.team == Team.White);
        // opponent
        else
            SetLocalTeam(player.team == Team.Black);
    }

    public void SetLocalTeam(bool isWhite)
    {
        string team = isWhite ? "White" : "black";
        youPlayTeamText.text = $"Team: {team}";
        
        if(isWhite)
        {
            // if(!whiteLocalIconFader.visible)
            whiteLocalIconFader.FadeIn();
            // if(blackLocalIconFader.visible)
            blackLocalIconFader.FadeOut();

            // if(whiteOpponentIconFader.visible)
            whiteOpponentIconFader.FadeOut();
            // if(!blackOpponentIconFader.visible)
            blackOpponentIconFader.FadeIn();
        }
        else
        {
            // if(whiteLocalIconFader.visible)
            whiteLocalIconFader.FadeOut();
            // if(!blackLocalIconFader.visible)
            blackLocalIconFader.FadeIn();

            // if(!whiteOpponentIconFader.visible)
            whiteOpponentIconFader.FadeIn();
            // if(blackOpponentIconFader.visible)
            blackOpponentIconFader.FadeOut();
        }
    }

    public void SetIP(string ip) => ipPanel.SetIP(ip);

    public void OpponentSearching()
    {
        opponentSearching = true;
        readyToggle.Hide();
        readyToggle.gameObject.SetActive(false);
        readyButtonContextText.text = "";

        if(readyFader.visible)
            readyFader.FadeOut();
        
        ResetToSearchingPanel();

        Debug.Log("Opponent Searching...");
    }
    public void OpponentFound(Player host)
    {
        opponentSearching = false;
        if(lobbyType == Type.Client)
        {
            if(!readyToggle.gameObject.activeSelf)
                readyToggle.gameObject.SetActive(true);
            readyToggle.Show();
        }
        else
            startButton.ShowDisabledButton();

        readyButtonContextText.text = lobbyType == Type.Host ? "Waiting for opponent to ready up" : "";

        if(!opponentSearchingPanel.visible)
            opponentSearchingPanel.FadeIn();
        
        if(readyFader.visible)
            readyFader.FadeOut();

        opponentSearchingText.text = "Opponent Found!";
        opponentLoadingFader.FadeOut();

        opponentNameFader.FadeIn();
        blackOpponentIconFader.FadeIn();

        UpdateTeam(host);

        Debug.Log("Opponent Found!");
    }

    public void ReadyRecieved()
    {
        Debug.Log("Ready Recieved");
        if(lobbyType == Type.Host)
            startButton.ShowEnabledButton();
        readyButtonContextText.text = "";
        if(!readyFader.visible)
            readyFader.FadeIn();
    }
    public void UnreadyRecieved()
    {
        Debug.Log("Unready Recieved");
        if(lobbyType == Type.Host)
            startButton.ShowDisabledButton();
        readyButtonContextText.text = opponentSearching ? "" : "Waiting for opponent to ready up";
        if(readyFader.visible)
            readyFader.FadeOut();
    }

    private void ResetToSearchingPanel()
    {
        // This is going to be called when the networker recieves a disconnect or is destroyed.
        // We destroy the networker when loading an AI lobby.
        // AI is never searching for an opponent, just leave.
        if(lobbyType == Type.AI)
            return;

        if(!opponentSearchingPanel.visible)
            opponentSearchingPanel.FadeIn();
            
        SetSearchingText();

        if(!opponentLoadingFader.visible)
            opponentLoadingFader.FadeIn();

        if(opponentNameFader.visible)
            opponentNameFader.FadeOut();
        if(blackOpponentIconFader.visible)
            blackOpponentIconFader.FadeOut();
        if(whiteOpponentIconFader.visible)
            whiteOpponentIconFader.FadeOut();

        if(readyFader.visible)
            readyFader.FadeOut();
    }

    private void SetAIPanel()
    {
        if(!opponentSearchingPanel.visible)
            opponentSearchingPanel.FadeIn();

        SetSearchingText();

        opponentLoadingFader.Disable();

        if(!blackOpponentIconFader.visible)
            blackOpponentIconFader.FadeIn();
        AITeam = Team.Black;
        
        ipPanel?.FadeOut();

        readyButtonContextText.text = "";

        proposeTeamChangeText.text = "SWAP TEAMS";
    }

    private void SetSearchingText() => opponentSearchingText.text = lobbyType switch
    {
        Type.Host => "Waiting for opponent...",
        Type.Client => "Finding opponent...",
        Type.AI => "AI Settings",
        _ => ""
    };

    public void QueryTeamChange()
    {
        if(lobbyType == Type.Client)
        {
            if(readyToggle.toggle.isOn)
                readyToggle.toggle.isOn = false;
        }
        teamChangePanel?.Query();
    }

    public void SwapAITeam()
    {
        AITeam = AITeam.Enemy();
        SetLocalTeam(AITeam == Team.Black);
    }

    public void ToggleHandicapOverlay(bool isOn) => handicapOverlayToggle.toggle.isOn = isOn;
}