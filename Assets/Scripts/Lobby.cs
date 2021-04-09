using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    Networker networker;
    [SerializeField] private PlayerLobby playerLobbyPrefab;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private TextMeshProUGUI ipText;
    [ReadOnly, ShowInInspector] Dictionary<Player, PlayerLobby> lobbyDict = new Dictionary<Player, PlayerLobby>();

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
        if(lobbyDict.ContainsKey(player))
            return;

        PlayerLobby pl = Instantiate(playerLobbyPrefab, playerContainer);
        pl.SetPlayer(player);
        lobbyDict.Add(player, pl);
    }

    public void RemovePlayer(Player player)
    {
        if(!lobbyDict.ContainsKey(player))
            return;
        
        PlayerLobby pl = lobbyDict[player];
        lobbyDict.Remove(player);
        Destroy(pl.gameObject);
    }

    public void SetIP(string ip, int port) => ipText.text = $"{ip}:{port}";
}