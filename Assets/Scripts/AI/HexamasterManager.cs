using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Extensions;

public class HexamasterManager : MonoBehaviour
{
    [SerializeField] private Hexamaster agentPrefab;
    [SerializeField] private Board board;
    public bool whiteAI = false;
    public bool blackAI = true;
    public Hexamaster whiteAgent;
    public Hexamaster blackAgent;

    private bool gameOver = false;

    public float decisionTime = 1f;
    private float decideAtTime;
    private Hexamaster nextDecision;

    private void Start() {
        board.newTurn += NewTurn;
        board.gameOver += GameOver;

        SpawnAI();

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
                    whiteAgent?.AddReward(1);
                if(blackAI)
                    blackAgent?.AddReward(-1);
                break;
            case Winner.Black:
                if(whiteAI)
                    whiteAgent?.AddReward(-1);
                if(blackAI)
                    blackAgent?.AddReward(1);
                break;
            case Winner.Draw:
                if(whiteAI)
                    whiteAgent?.AddReward(0);
                if(blackAI)
                    blackAgent?.AddReward(0);
                break;
            case Winner.None:
                if(whiteAI)
                    whiteAgent?.AddReward(0);
                if(blackAI)
                    blackAgent?.AddReward(0);
                break;
        }

        if(whiteAI && whiteAgent != null)
        {
            whiteAgent.EndEpisode();
            Destroy(whiteAgent.gameObject);
        }
        if(blackAI && blackAgent != null)
        {
            blackAgent.EndEpisode();
            Destroy(blackAgent.gameObject);
        }

        gameOver = true;
        Reset();
    }

    private void SpawnAI()
    {
        if(whiteAI)
        {
            if(whiteAgent != null)
                Destroy(whiteAgent.gameObject);
            whiteAgent = Instantiate(agentPrefab);
            whiteAgent.Init(board, 0);
        }
        if(blackAI)
        {
            if(blackAgent != null)
                Destroy(blackAgent.gameObject);
            blackAgent = Instantiate(agentPrefab);
            blackAgent.Init(board, 1);
        }
    }

    private void Reset()
    {
        board.LoadDefaultBoard();
        SpawnAI();

        if(whiteAI)
            whiteAgent?.CollectObservations(new VectorSensor(310));
        if(blackAI)
            blackAgent?.CollectObservations(new VectorSensor(310));
        
        gameOver = false;
        
        NewTurn(board.GetCurrentBoardState());
    }

    private void NewTurn(BoardState newState)
    {
        if(gameOver)
            return;

        Move m = BoardState.GetLastMove(board.turnHistory);
        if(m.capturedPiece.HasValue)
        {
            // Reward the team that captured a piece, while punishing the team who's piece was captured
            float val = 0.1f * (float)m.capturedPiece.Value.GetMaterialValue();

            Hexamaster positiveAgent = m.lastTeam == Team.White ? whiteAI ? whiteAgent : null : m.lastTeam == Team.Black ? blackAI ? blackAgent : null : null;
            Hexamaster negativeAgent = m.lastTeam == Team.White ? blackAI ? blackAgent : null : m.lastTeam == Team.Black ? whiteAI ? whiteAgent : null : null;

            positiveAgent?.AddReward(val);
            negativeAgent?.AddReward(-val);
        }
        
        // Reward the team that checked or mated, but punish the team that was checked or mated
        if(newState.checkmate != Team.None){ }
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
    
        Hexamaster agent = newState.currentMove switch {
            Team.White when whiteAI => whiteAgent,
            Team.Black when blackAI => blackAgent,
            _ => null
        };

        agent?.CollectObservations(new VectorSensor(310));
        agent?.RequestDecision();
        // if(agent != null)
        // {
        //     agent.CollectObservations(new VectorSensor(310));
        //     nextDecision = agent;
        //     decideAtTime = Time.timeSinceLevelLoad + decisionTime;
        // }
    }
}