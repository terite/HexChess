using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using Extensions;

public class Lobby : MonoBehaviour
{
    Networker networker;
    [SerializeField] private GroupFader fader;
    [SerializeField] private GroupFader connectionChoiceFader;
    [SerializeField] private PlayerLobby playerLobbyPrefab;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private GroupFader opponentSearchingPanel;
    [SerializeField] public IPPanel ipPanel;
    [SerializeField] public ReadyButton readyToggle;

    PlayerLobby host;
    PlayerLobby client;

    public Toggle clockToggle;
    [SerializeField] private GameObject clockObj;
    [SerializeField] private TextMeshProUGUI clockText;

    public Toggle timerToggle;
    [SerializeField] private GameObject timerObj;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TMP_InputField timerInputField;

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
            timerObj.SetActive(false);
            clockObj.SetActive(false);
        }
        else
        {   
            // host
            opponentSearchingPanel.FadeIn();
            readyToggle.gameObject.SetActive(false);
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
    
    public void SpawnPlayer(Player player)
    {
        if(player.isHost)
        {
            host = Instantiate(playerLobbyPrefab, playerContainer);
            host.SetPlayer(player);
        }
        else
        {
            opponentSearchingPanel.FadeOut();
            client = Instantiate(playerLobbyPrefab, playerContainer);
            client.SetPlayer(player);
        }
    }

    public void RemovePlayer(Player player)
    {
        if(!player.isHost)
            opponentSearchingPanel.FadeIn();
        PlayerLobby lobbyToDestroy = player.isHost ? host : client;
        Destroy(lobbyToDestroy.gameObject);
    }

    public void UpdateName(Player player)
    {
        PlayerLobby toChange = player.isHost ? host : client;
        toChange.SetPlayer(player);
    }

    public void SwapTeams(Player hostPlayer, Player clientPlayer)
    {
        host.SetPlayer(hostPlayer);
        client.SetPlayer(clientPlayer);
    }

    public void SetIP(string ip) => ipPanel.SetIP(ip);

    public void OpponentSearching()
    {
        readyToggle.Hide();
        opponentSearchingPanel.FadeIn();
        Debug.Log("Opponent Searching...");
    }
    public void OpponentFound()
    {
        readyToggle.Show();
        opponentSearchingPanel.FadeOut();
        Debug.Log("Opponent Found!");
    }
}