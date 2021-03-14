using UnityEngine;
using TMPro;
using System;

public class CheckText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    Board board;

    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        board.newTurn += NewTurn;
        board.gameOver += GameOver;
        gameObject.SetActive(false);
    }

    private void GameOver(Game game)
    {
        BoardState finalState = game.turnHistory[game.turnHistory.Count - 1];

        if(finalState.checkmate == Team.None && game.winner != Winner.Draw)
        {
            gameObject.SetActive(true);
            text.color = Color.red;
            Team loser = game.winner == Winner.White ? Team.Black : Team.White;
            text.text = $"{loser} surrendered.";
        }
    }

    private void NewTurn(BoardState newState)
    {
        if(newState.check == Team.None && newState.checkmate == Team.None)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        if(newState.check > 0)
        {
            text.color = Color.yellow;
            text.text = "Check";
        }
        else if(newState.checkmate > 0)
        {
            text.color = Color.red;
            text.text = "Checkmate";
        }
        else
            text.text = "Something went wrong";
    }


}
