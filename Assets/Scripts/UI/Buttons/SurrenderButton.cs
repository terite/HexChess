using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurrenderButton : MonoBehaviour
{
    Board board;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button resetButtonPrefab;
    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        button.onClick.AddListener(() => {
            Networker networker = GameObject.FindObjectOfType<Networker>();
            float timestamp = Time.timeSinceLevelLoad + board.timeOffset;
            if(networker != null)
            {
                networker.SendMessage(new Message(
                    type: MessageType.Surrender, 
                    data: Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(timestamp))
                ));
                Team surrenderingTeam = networker.isHost ? networker.host.team : networker.player.Value.team;
                board.EndGame(timestamp, GameEndType.Surrender, surrenderingTeam == Team.White ? Winner.Black : Winner.White);
            }
            else
                board.EndGame(timestamp, GameEndType.Surrender, board.GetCurrentTurn() == Team.White ? Winner.Black : Winner.White);
        });
        board.gameOver += gameOver;
    }

    private void gameOver(Game game)
    {
        board.gameOver -= gameOver;
        button.onClick.RemoveAllListeners();

        if(GameObject.FindObjectOfType<Multiplayer>() == null)
        {
            Button resetButton = Instantiate(resetButtonPrefab, transform.parent);
            resetButton.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }

        Destroy(gameObject);
    }
}