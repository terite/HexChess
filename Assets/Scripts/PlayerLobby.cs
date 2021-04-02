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
    [ReadOnly, ShowInInspector] private Player player;

    public void SetPlayer(Player player)
    {
        this.player = player;
        Networker networker = GameObject.FindObjectOfType<Networker>();
        string mod = networker?.isHost == player.isHost ? "you" : "opponent";
        playerNameText.text = $"{player.name} ({mod})";
        teamColor.color = player.team == Team.White ? whiteColor : blackColor;
    }
}
