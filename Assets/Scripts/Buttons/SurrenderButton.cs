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
            if(networker != null)
            {
                networker.SendMessage(new Message(MessageType.Surrender));
                board.Surrender(networker.isHost ? networker.host.team : networker.player.Value.team);
            }
            else
                board.Surrender(board.GetCurrentTurn());
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