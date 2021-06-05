using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Hexamaster : Agent
{

    [SerializeField] private Board board;
    List<(IPiece, Index)> cachedMoves = new List<(IPiece, Index)>();

    public override void OnEpisodeBegin()
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        BoardState state = board.GetCurrentBoardState();
        List<Promotion> promotions = board.promotions;
        List<float> availableMoves = new List<float>();

        foreach(KeyValuePair<(Team team, Piece piece), Index> kvp in state.allPiecePositions)
        {
            sensor.AddObservation(new Vector3(
                (float)kvp.Key.team, 
                (float)kvp.Key.piece, 
                kvp.Value.GetSingleVal()
            ));

            if(kvp.Key.team != state.currentMove)
                continue;
            
            IPiece piece = board.activePieces[kvp.Key];
            IEnumerable<(Index target, MoveType moveType)> moves = board.GetAllValidMovesForPiece(piece, state);
            cachedMoves.AddRange(moves.Select(move => (piece, move.target)));
            availableMoves.AddRange(moves.Select(move => (float)move.target.GetSingleVal()));
        }

        sensor.AddObservation((float)state.currentMove);
        sensor.AddObservation(availableMoves);
        sensor.AddObservation(availableMoves.Count - 1);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Debug.Log(vectorAction[0]);
        MakeDecision((int)vectorAction[0]);
    }
    public override void Heuristic(float[] actionsOut)
    {
        MakeDecision((int)actionsOut[0]);
    }

    private void MakeDecision(int index)
    {
        if(index >= 0 && index < cachedMoves.Count)
        {
            float reward = 0;
            (IPiece piece, Index target) move = cachedMoves[index];
            BoardState newState = board.MovePiece(move.piece, move.target, board.GetCurrentBoardState());
            board.AdvanceTurn(newState);

            reward += .1f;

            Move agentMove = BoardState.GetLastMove(board.turnHistory);
            if(agentMove.capturedPiece.HasValue)
                reward += .1f;
            
            newState = board.GetCurrentBoardState();

            if(newState.checkmate != Team.None)
                reward += 1f;
            else if(newState.check != Team.None) 
                reward += 1f;

            cachedMoves.Clear();

            AddReward(reward);
        }
        else
        {
            AddReward(-.1f);
            RequestDecision();
        }
    }
}