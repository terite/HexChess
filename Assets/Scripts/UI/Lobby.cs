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

    [SerializeField] private GroupFader opponentTitleFader;
    [SerializeField] private GroupFader opponentLoadingFader;
    [SerializeField] private GroupFader opponentNameFader;
    [SerializeField] private GroupFader blackOpponentIconFader;
    [SerializeField] private GroupFader whiteOpponentIconFader;

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

        opponentTitleFader.FadeIn();
        opponentLoadingFader.FadeIn();

        opponentNameFader.FadeOut();
        blackOpponentIconFader.FadeOut();

        opponentName.text = "";
    }

    public void DisconnectRecieved()
    {
        if(teamChangePanel != null && teamChangePanel.isOpen)
            teamChangePanel.Close();

        Debug.Log("Opponent Disconnected.");
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
        Debug.Log("here");
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
        opponentSearchingText.text = networker.isHost ? "Waiting for opponent..." : "Finding opponent...";
        readyToggle.Hide();
        readyToggle.gameObject.SetActive(false);
        readyButtonContextText.text = "";
        
        ResetToSearchingPanel();

        Debug.Log("Opponent Searching...");
    }
    public void OpponentFound()
    {
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

        opponentTitleFader.FadeOut();
        opponentLoadingFader.FadeOut();

        opponentNameFader.FadeIn();
        blackOpponentIconFader.FadeIn();

        UpdateTeam(networker.host);

        Debug.Log("Opponent Found!");
    }

    public void ReadyRecieved()
    {
        startButton.ShowEnabledButton();
        readyButtonContextText.text = "";
    }
    public void UnreadyRecieved()
    {
        startButton.ShowDisabledButton();
        readyButtonContextText.text = "Waiting for opponent to ready up";
    }

    private void ResetToSearchingPanel()
    {
        if(!opponentSearchingPanel.visible)
            opponentSearchingPanel.FadeIn();

        if(!opponentTitleFader.visible)
            opponentTitleFader.FadeIn();
        if(!opponentLoadingFader.visible)
            opponentLoadingFader.FadeIn();

        if(opponentNameFader.visible)
            opponentNameFader.FadeOut();
        if(blackOpponentIconFader.visible)
            blackOpponentIconFader.FadeOut();
        if(whiteOpponentIconFader.visible)
            whiteOpponentIconFader.FadeOut();
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