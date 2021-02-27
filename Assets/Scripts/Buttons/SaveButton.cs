using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SaveButton : MonoBehaviour
{
    [SerializeField] private Board board;

    private void Awake() 
    {
        if(board == null)
            board = GameObject.FindObjectOfType<Board>();
    }

    public void Save()
    {
        List<(Team, List<TeamPieceLoc>, Team, Team)> ml = new List<(Team, List<TeamPieceLoc>, Team, Team)>();
        foreach(BoardState bs in board.turnHistory)
        {
            List<TeamPieceLoc> serializeableList = bs.GetSerializeable();
            ml.Add((bs.currentMove, serializeableList, bs.check, bs.checkmate));
        }
        string json = JsonConvert.SerializeObject(ml);
        string path = Application.persistentDataPath + $"/saves";
        Directory.CreateDirectory(Application.persistentDataPath + $"/saves");
        File.WriteAllText(path + $"/{DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "")}.json", json);
        Debug.Log($"Saved to file: {path}");
    }
}