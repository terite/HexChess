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
        string path = Application.persistentDataPath + $"/saves";
        Directory.CreateDirectory(path);

        File.WriteAllText(
            path + $"/{DateTime.Now.ToString().Replace("/", "-").Replace(":", "-")}.json", 
            Game.Serialize(board.turnHistory)
        );

        Debug.Log($"Saved to file: {path}");
    }
}