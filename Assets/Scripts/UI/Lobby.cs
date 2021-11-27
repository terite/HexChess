using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Extensions;

public class Lobby : MonoBehaviour
{
    Networker networker;
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

    private void Awake() {
        networker = GameObject.FindObjectOfType<Networker>();
        timerInputField.gameObject.SetActive(false);

        clockToggle.onValueChanged.AddListener((isOn) => {
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

    public void Show()
    {
        opponentSearching = true;

        connectionChoiceFader?.FadeOut();
        if(networker == null || !networker.isHost)
        {
            // client
            opponentSearchingText.text = "Finding opponent...";
            timerObj.SetActive(false);
            clockObj.SetActive(false);
        }
        else
        {   
            // host
            OpponentSearching();
            timerObj.SetActive(true);
            clockObj.SetActive(true);
        }
        fader.FadeIn();
    }

    public void Hide()
    {
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

        opponentSearchingText.text = networker.isHost ? "Waiting for opponent..." : "Finding opponent...";
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

        if(networker.isHost)
            startButton.HideButton();
        else
        {
            readyToggle.Hide();
            readyToggle.gameObject.SetActive(!networker.isHost);
        }
    }

    public void UpdatePlayerName(Player player) => opponentName.text = player.name;

    public void UpdateTeam(Player player)
    {
        // local
        if((player.isHost && networker.isHost) || (!player.isHost && !networker.isHost))
            SetLocalTeam(player.team == Team.White);
        // opponent
        else
            SetLocalTeam(player.team == Team.Black);
    }

    public void SetLocalTeam(bool isWhite)
    {
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
        opponentSearchingText.text = networker.isHost ? "Waiting for opponent..." : "Finding opponent...";
        readyToggle.Hide();
        readyToggle.gameObject.SetActive(false);
        readyButtonContextText.text = "";

        if(readyFader.visible)
            readyFader.FadeOut();
        
        ResetToSearchingPanel();

        Debug.Log("Opponent Searching...");
    }
    public void OpponentFound()
    {
        opponentSearching = false;
        if(!networker.isHost)
        {
            if(!readyToggle.gameObject.activeSelf)
                readyToggle.gameObject.SetActive(true);
            readyToggle.Show();
        }
        else
            startButton.ShowDisabledButton();

        readyButtonContextText.text = networker.isHost ? "Waiting for opponent to ready up" : "";

        if(!opponentSearchingPanel.visible)
            opponentSearchingPanel.FadeIn();
        
        if(readyFader.visible)
            readyFader.FadeOut();

        opponentSearchingText.text = "Opponent Found!";
        opponentLoadingFader.FadeOut();

        opponentNameFader.FadeIn();
        blackOpponentIconFader.FadeIn();

        UpdateTeam(networker.host);

        Debug.Log("Opponent Found!");
    }

    public void ReadyRecieved()
    {
        Debug.Log("Ready Recieved");
        if(networker.isHost)
            startButton.ShowEnabledButton();
        readyButtonContextText.text = "";
        if(!readyFader.visible)
            readyFader.FadeIn();
    }
    public void UnreadyRecieved()
    {
        Debug.Log("Unready Recieved");
        if(networker.isHost)
            startButton.ShowDisabledButton();
        readyButtonContextText.text = opponentSearching ? "" : "Waiting for opponent to ready up";
        if(readyFader.visible)
            readyFader.FadeOut();
    }

    private void ResetToSearchingPanel()
    {
        if(!opponentSearchingPanel.visible)
            opponentSearchingPanel.FadeIn();

        opponentSearchingText.text = networker.isHost ? "Waiting for opponent..." : "Finding opponent...";

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

    public void QueryTeamChange()
    {
        if(!networker.isHost)
        {
            if(readyToggle.toggle.isOn)
                readyToggle.toggle.isOn = false;
        }
        teamChangePanel?.Query();
    }

    public void ToggleHandicapOverlay(bool isOn) => handicapOverlayToggle.toggle.isOn = isOn;
}