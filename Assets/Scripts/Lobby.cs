using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class Lobby : MonoBehaviour
{
    [SerializeField] private PlayerLobby playerLobbyPrefab;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private TextMeshProUGUI ipText;
    [ReadOnly, ShowInInspector] Dictionary<Player, PlayerLobby> lobbyDict = new Dictionary<Player, PlayerLobby>();
    
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