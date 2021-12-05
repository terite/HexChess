using System;
using System.Collections.Generic;
using Extensions;

public readonly struct HexAIMove
{
    public readonly static HexAIMove Invalid = new HexAIMove(new Index(255, 255), new Index(255, 255), MoveType.None);
    public readonly Index start;
    public readonly Index target;
    public readonly MoveType moveType;
    public readonly Piece promoteTo;

    public HexAIMove(Index start, Index target, MoveType moveType)
    {
        this.start = start;
        this.target = target;
        this.moveType = moveType;
        this.promoteTo = Piece.Pawn1;
    }

    public HexAIMove(Index start, Index target, MoveType moveType, Piece promoteTo)
    {
        this.start = start;
        this.target = target;
        this.moveType = moveType;
        this.promoteTo = promoteTo;
    }

    public (BoardState state, List<Promotion> promotions) Speculate(Board board)
    {
        return board.currentGame.QueryMove(start, (target, moveType), board.GetCurrentBoardState(), promoteTo);
    }

    public override string ToString()
    {
        return $"{moveType}({start.GetKey()} -> {target.GetKey()})";
    }

    public static IEnumerable<HexAIMove> GenerateAllValidMoves(Board board)
    {
        var state = board.currentGame.turnHistory[board.currentGame.turnHistory.Count - 1];
        BoardState previousState = board.currentGame.turnHistory.Count > 1
            ? board.currentGame.turnHistory[board.currentGame.turnHistory.Count - 2]
            : default;

        foreach (var move in MoveGenerator.GenerateAllValidMoves(state.currentMove, board.promotions, state, previousState))
        {
            yield return new HexAIMove(move.start, move.target, move.moveType, move.promoteTo);
        }
    }
}
