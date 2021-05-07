using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobby : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image teamColor;
    public Color whiteColor;
    public Color blackColor;
    [ReadOnly, ShowInInspector] public Player player {get; private set;}
    Networker networker;

    public void SetPlayer(Player player)
    {
        this.player = player;
        networker = GameObject.FindObjectOfType<Networker>();
        UpdateText(player.name);
        teamColor.color = player.team == Team.White ? whiteColor : blackColor;
    }

    public void UpdateText(string newName)
    {
        string mod = networker?.isHost == player.isHost ? "you" : "opponent";
        playerNameText.text = $"{newName} ({mod})";

    }
}
