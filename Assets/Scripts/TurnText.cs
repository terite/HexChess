using UnityEngine;
using TMPro;
using System;

public class TurnText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    Board board;
    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        board.newTurn += NewTurn;
        board.gameOver += GameOver;
    }

    private void GameOver(Game game)
    {
        turnText.text = $"Game over! In {game.turns} turns {game.winner} is victorius!";
        // turnText.color = newState.currentMove == Team.White ? Color.white : Color.black;

        turnText.color = game.winner switch {
            Winner.White => Color.white,
            Winner.Black => Color.black,
            _ => Color.gray
        };
        turnText.text = game.winner switch {
            Winner.Pending => "",
            Winner.Draw => $"After {game.turns} turns, Draw.",
            _ => $"Game over! In {game.turns} turns {game.winner} is victorius!"
        };
    }

    private void NewTurn(BoardState newState)
    {
        string text = newState.currentMove == Team.White ? "White's Turn" : "Black's Turn";
        turnText.text = $"{Mathf.FloorToInt((float)board.turnHistory.Count / 2f) + 1}:{text}";
        turnText.color = newState.currentMove == Team.White ? Color.white : Color.black;
    }
}