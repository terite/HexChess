using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Linq;

public class Lobby : MonoBehaviour
{
    Networker networker;
    [SerializeField] private PlayerLobby playerLobbyPrefab;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private TextMeshProUGUI ipText;
    // [ReadOnly, ShowInInspector] Dictionary<Player, PlayerLobby> lobbyDict = new Dictionary<Player, PlayerLobby>();
    PlayerLobby host;
    PlayerLobby client;

    [SerializeField] private GameObject timerPanel;
    public Toggle noneToggle;
    [SerializeField] private Image noneImage;
    public Toggle clockToggle;
    [SerializeField] private Image clockImage;
    public Toggle timerToggle;
    [SerializeField] private Image timerImage;
    [SerializeField] private TMP_InputField timerInputField;
    [SerializeField] private GameObject minutesPanel;

    public Color toggleOnColor;
    public Color toggleOffColor;


    private void Awake() {
        networker = GameObject.FindObjectOfType<Networker>();
        if(networker == null || !networker.isHost)
            timerPanel.SetActive(false);
        else
            timerPanel.SetActive(true);
        
        noneToggle.onValueChanged.AddListener((isOn) => {
            noneImage.color = GetToggleColor(isOn);
            if(isOn)
            {
                clockToggle.isOn = false;
                timerToggle.isOn = false;
            }
        });
        clockToggle.onValueChanged.AddListener((isOn) => {
            clockImage.color = GetToggleColor(isOn);
            if(isOn)
            {
                noneToggle.isOn = false;
                timerToggle.isOn = false;
            }

        });
        timerToggle.onValueChanged.AddListener((isOn) => {
            timerImage.color = GetToggleColor(isOn);
            if(isOn)
            {
                noneToggle.isOn = false;
                clockToggle.isOn = false;
                minutesPanel.SetActive(true);
                timerInputField.text = "20";
            }
            else
                minutesPanel.SetActive(false);
        });
    }
    private void Start() {
        noneToggle.isOn = true;
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
            client = Instantiate(playerLobbyPrefab, playerContainer);
            client.SetPlayer(player);
        }
    }

    public void RemovePlayer(Player player)
    {
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

    public void SetIP(string ip, int port) => ipText.text = $"{ip}:{port}";
}