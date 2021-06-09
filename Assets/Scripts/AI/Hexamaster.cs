using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Hexamaster : Agent
{

    [SerializeField] private Board board;
    List<(IPiece, Index, MoveType)> cachedMoves = new List<(IPiece, Index, MoveType)>();

    public override void OnEpisodeBegin()
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        BoardState state = board.GetCurrentBoardState();
        List<Promotion> promotions = board.promotions;
        List<float> froms = new List<float>();
        List<float> tos = new List<float>();

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
            cachedMoves.AddRange(moves.Select(move => (piece, move.target, move.moveType)));
            foreach(var move in moves)
            {
                froms.Add(piece.location.GetSingleVal());
                tos.Add(move.target.GetSingleVal());
            }
            tos.AddRange(moves.Select(move => (float)move.target.GetSingleVal()));
        }

        sensor.AddObservation((float)state.currentMove);
        sensor.AddObservation(froms);
        sensor.AddObservation(tos);
        sensor.AddObservation(tos.Count - 1);
        sensor.AddObservation(board.turnsSincePawnMovedOrPieceTaken);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        MakeDecision((int)vectorAction[0], (int)vectorAction[1]);
    }
    public override void Heuristic(float[] actionsOut)
    {
        MakeDecision((int)actionsOut[0], (int)actionsOut[1]);
    }

    private void MakeDecision(int index, int promoChoice)
    {
        float reward = 0;
        if(index >= 0 && index < cachedMoves.Count)
        {
            (IPiece piece, Index target, MoveType type) move = cachedMoves[index];

            // AI Played a promo
            if((move.piece is Pawn pawn) && pawn.GetGoalInRow(move.target.row) == move.target.row)
            {
                if(promoChoice <= 0 || promoChoice > 4)
                {
                    // Invalid promotion choice, punish and requery
                    AddReward(-0.1f);
                    RequestDecision();
                    return;
                }
                else
                {
                    // Reward the AI for promoting
                    Piece promoTo = GetPromoPiece(promoChoice);
                    move.piece = board.Promote(pawn, promoTo);
                    AddReward(0.1f);
                }
            }

            BoardState newState = board.GetCurrentBoardState();
            if(move.type == MoveType.Attack || move.type == MoveType.Move)
                newState = board.MovePiece(move.piece, move.target, newState);
            else if(move.type == MoveType.Defend && newState.TryGetPiece(move.target, out (Team team, Piece piece) otherPiece) && board.activePieces.ContainsKey(otherPiece))
                newState = board.Swap(move.piece, board.activePieces[otherPiece], newState);
            else if(move.type == MoveType.EnPassant && newState.TryGetPiece(move.target, out (Team team, Piece piece) enemyPiece))
                newState = board.EnPassant((Pawn)move.piece, enemyPiece.team, enemyPiece.piece, move.target, newState);
            
            // reward += .1f;

            board.AdvanceTurn(newState, true, true);
            Move m = BoardState.GetLastMove(board.turnHistory);
            reward += -0.00001f * m.turn;

            cachedMoves.Clear();
        }
        else
        {
            if(promoChoice > 0)
                reward -= 0.5f;
            reward -= 0.5f;

            RequestDecision();
        }
        AddReward(reward);
    }

    public Piece GetPromoPiece(int promo) => promo switch {
        1 => Piece.KingsRook,
        2 => Piece.KingsKnight,
        3 => Piece.BlackSquire,
        4 => Piece.Queen,
        _ => Piece.Pawn1
    };
}