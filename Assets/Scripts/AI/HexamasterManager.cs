using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexamasterManager : MonoBehaviour
{
    [SerializeField] private Board board;
    public Hexamaster whiteAgent;
    public Hexamaster blackAgent;

    private bool gameOver = false;

    private void Start() {
        board.newTurn += NewTurn;
        board.gameOver += GameOver;

        NewTurn(board.GetCurrentBoardState());
    }

    private void GameOver(Game game)
    {
        switch(game.winner)
        {
            case Winner.White:
                whiteAgent.AddReward(1);
                blackAgent.AddReward(-1);
                break;
            case Winner.Black:
                whiteAgent.AddReward(-1);
                blackAgent.AddReward(1);
                break;
            case Winner.Draw:
                whiteAgent.AddReward(0.5f);
                blackAgent.AddReward(0.5f);
                break;
            case Winner.None:
                whiteAgent.AddReward(-0.5f);
                blackAgent.AddReward(-0.5f);
                break;
        }
        gameOver = true;
        whiteAgent.EndEpisode();
        blackAgent.EndEpisode();
    }

    private void NewTurn(BoardState newState)
    {
        if(gameOver)
            return;
        Hexamaster agent = newState.currentMove switch {
            Team.White => whiteAgent,
            Team.Black => blackAgent,
            _ => null
        };
    
        if(agent != null)
            agent.RequestDecision();
    }
}