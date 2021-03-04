using System;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;

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
        // Prevents it from opening too many save file browsers
        EventSystem.current.SetSelectedGameObject(null);

        string path = Application.persistentDataPath + $"/saves";
        Directory.CreateDirectory(path);

        string file = StandaloneFileBrowser.SaveFilePanel(
            title: "Save File", 
            directory: path, 
            defaultName: $"/{DateTime.Now.ToString().Replace("/", "-").Replace(":", "-")}.json", 
            extensions: new []{
                new ExtensionFilter("Json Files", "json"),
                new ExtensionFilter("All FIles", "*")
            }
        );

        if(string.IsNullOrEmpty(file))
        {
            Debug.Log("Failed to save to file. Path empty.");
            return;
        }
        
        File.WriteAllText(
            file, 
            Game.Serialize(board.turnHistory, board.promotions, board.game.winner)
        );

        Debug.Log($"Saved to file: {file}");
    }
}