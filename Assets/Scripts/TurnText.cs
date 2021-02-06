using UnityEngine;
using TMPro;

public class TurnText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    BoardManager boardManager;
    private void Awake() {
        boardManager = GameObject.FindObjectOfType<BoardManager>();
        boardManager.newTurn += NewTurn;
    }

    private void NewTurn(Team team)
    {
        turnText.text = team == Team.White ? "White's Turn" : "Black's Turn";
        turnText.color = team == Team.White ? Color.white : Color.black;
    }
}