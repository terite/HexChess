using TMPro;
using UnityEngine;

public class MovePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnNumberText;
    [SerializeField] private TextMeshProUGUI whiteText;
    [SerializeField] private TextMeshProUGUI blackText;
    Board board;
    private void Awake() => board = GameObject.FindObjectOfType<Board>();

    public void SetMove(Move move)
    {
        TextMeshProUGUI toChange = move.lastTeam == Team.White ? whiteText : blackText;
        BoardState boardState = board.GetCurrentBoardState();
        string shortForm = move.GetNotation(board.promotions, boardState, NotationType.LongForm);
        
        toChange.text = shortForm;

        if(move.lastTeam == Team.White)
            blackText.text = "";
    }

    public void SetTurnNumber(int val) => turnNumberText.text = $"{val}";
}