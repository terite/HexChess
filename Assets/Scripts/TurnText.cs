using UnityEngine;
using TMPro;

public class TurnText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    Board board;
    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        board.newTurn += NewTurn;
    }

    private void NewTurn(BoardState newState)
    {
        turnText.text = newState.currentMove == Team.White ? "White's Turn" : "Black's Turn";
        turnText.color = newState.currentMove == Team.White ? Color.white : Color.black;
    }
}