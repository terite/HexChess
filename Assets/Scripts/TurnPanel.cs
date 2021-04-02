using UnityEngine;
using TMPro;

public class TurnPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private SurrenderButton surrenderButton;
    GameObject mainMenuButton;
    [SerializeField] private GameObject mainMenuButtonPrefab;

    Board board;
    Multiplayer multiplayer;
    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        board.newTurn += NewTurn;
        board.gameOver += GameOver;
        multiplayer = GameObject.FindObjectOfType<Multiplayer>();
    }

    private void GameOver(Game game)
    {
        turnText.text = $"Game over! In {game.GetTurnCount()} turns {game.winner} is victorius!";

        turnText.color = game.winner switch {
            Winner.White => Color.white,
            Winner.Black => Color.black,
            _ => Color.gray
        };
        turnText.text = game.winner switch {
            Winner.Pending => "",
            Winner.Draw => $"After {game.GetTurnCount()} turns, Draw.",
            _ => $"Game over! In {game.GetTurnCount()} turns {game.winner} is victorius!"
        };
        
        if(mainMenuButton == null)
            mainMenuButton = Instantiate(mainMenuButtonPrefab, buttonContainer);
    }

    private void NewTurn(BoardState newState)
    {
        string text = multiplayer == null 
            ? newState.currentMove == Team.White ? "White's Turn" : "Black's Turn"
            : newState.currentMove == multiplayer.localTeam ? "Your Turn" : "Opponent's Turn";

        int turnCount = board.turnHistory.Count % 2 == 0 
            ? Mathf.FloorToInt((float)board.turnHistory.Count / 2)
            : Mathf.FloorToInt((float)board.turnHistory.Count / 2f) + 1;
        turnText.text = $"{turnCount}:{text}";
        turnText.color = newState.currentMove == Team.White ? Color.white : Color.black;
    }

    public void Reset()
    {
        if(mainMenuButton != null)
            Destroy(mainMenuButton);
        // surrenderButton.Reset();
    }
}