using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexamasterManager : MonoBehaviour
{
    [SerializeField] private Hexamaster agentPrefab;
    [SerializeField] private Board board;
    public bool whiteAI = false;
    public bool blackAI = true;
    public Hexamaster whiteAgent;
    public Hexamaster blackAgent;

    private bool gameOver = false;

    // public float decisionTime = 1f;
    // private float decideAtTime;
    // private Hexamaster nextDecision;

    private void Start() {
        board.newTurn += NewTurn;
        board.gameOver += GameOver;

        NewTurn(board.GetCurrentBoardState());
    }

    // private void Update() {
    //     if(Time.timeSinceLevelLoad >= decideAtTime && nextDecision != null)
    //     {
    //         nextDecision.RequestDecision();
    //     }
    // }

    private void GameOver(Game game)
    {
        switch(game.winner)
        {
            case Winner.White:
                if(whiteAI)
                    whiteAgent?.AddReward(10);
                if(blackAI)
                    blackAgent?.AddReward(-10);
                break;
            case Winner.Black:
                if(whiteAI)
                    whiteAgent?.AddReward(-10);
                if(blackAI)
                    blackAgent?.AddReward(10);
                break;
            case Winner.Draw:
                if(whiteAI)
                    whiteAgent?.AddReward(0.5f);
                if(blackAI)
                    blackAgent?.AddReward(0.5f);
                break;
            case Winner.None:
                if(whiteAI)
                    whiteAgent?.AddReward(-0.5f);
                if(blackAI)
                    blackAgent?.AddReward(-0.5f);
                break;
        }
        gameOver = true;
        if(whiteAI)
            whiteAgent?.EndEpisode();
        if(blackAI)
            blackAgent?.EndEpisode();
    }

    private void NewTurn(BoardState newState)
    {
        if(gameOver)
            return;
        
        // Reward the team that checked or mated, but punish the team that was checked or mated
        if(newState.checkmate != Team.None)
        {
            if(newState.checkmate == Team.White)
            {
                if(whiteAI)
                    whiteAgent?.AddReward(-10f);
                if(blackAI)
                    blackAgent?.AddReward(100f);
            }
            else
            {
                if(whiteAI)
                    whiteAgent?.AddReward(100f);
                if(blackAI)
                    blackAgent?.AddReward(-10f);
            }
        }
        else if(newState.check != Team.None)
        {
            if(newState.check == Team.White)
            {
                if(whiteAI)
                    whiteAgent?.AddReward(-0.2f);
                if(blackAI)
                    blackAgent?.AddReward(0.2f);
            }
            else
            {
                if(whiteAI)
                    whiteAgent?.AddReward(0.2f);
                if(blackAI)
                    blackAgent?.AddReward(-0.2f);
            }
        }

        Move m = BoardState.GetLastMove(board.turnHistory);
        if(m.capturedPiece.HasValue)
        {
            // Reward the team that captured a piece, while punishing the team who's piece was captured
            if(m.lastTeam == Team.White)
            {
                if(whiteAI)
                    whiteAgent?.AddReward(0.1f);
                if(blackAI)
                    blackAgent?.AddReward(-0.1f);
            }
            else if(m.lastTeam == Team.Black)
            {
                if(whiteAI)
                    whiteAgent?.AddReward(-0.1f);
                if(blackAI)
                    blackAgent?.AddReward(0.1f);
            }
        }
    
        Hexamaster agent = newState.currentMove switch {
            Team.White when whiteAI => whiteAgent,
            Team.Black when blackAI => blackAgent,
            _ => null
        };

        agent?.RequestDecision();
        // if(agent != null)
        // {
        //     nextDecision = agent;
        //     decideAtTime = Time.timeSinceLevelLoad + decisionTime;
        // }
    }
}