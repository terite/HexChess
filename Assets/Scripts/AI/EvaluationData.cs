using System;
using System.Collections.Generic;

public class EvaluationData
{
    public BitsBoard WhitePieces;
    public BitsBoard BlackPieces;
    public BitsBoard WhitePawns;
    public BitsBoard BlackPawns;
    public BitsBoard WhiteThreats;
    public BitsBoard BlackThreats;
    public BitsBoard WhitePawnThreats;
    public BitsBoard BlackPawnThreats;
    public BitsBoard WhitePawnDefends;
    public BitsBoard BlackPawnDefends;

    readonly List<FastMove> threatMoveCache = new List<FastMove>();
    public void Prepare(FastBoardNode node)
    {
        var whitePieces = new BitsBoard();
        var blackPieces = new BitsBoard();

        var whitePawns = new BitsBoard();
        var blackPawns = new BitsBoard();

        var whiteThreats = new BitsBoard();
        var blackThreats = new BitsBoard();

        var moves = threatMoveCache;
        moves.Clear();
        for (byte b = 0; b < node.positions.Length; ++b)
        {
            var piece = node.positions[b];
            if (piece.team == Team.None)
                continue;
            else if (piece.team == Team.White)
                whitePieces[b] = true;
            else
                blackPieces[b] = true;

            if (piece.piece == FastPiece.Pawn)
            {
                if (piece.team == Team.White)
                    whitePawns[b] = true;
                else
                    blackPawns[b] = true;
            }
            else
            {
                var index = FastIndex.FromByte(b);
                FastPossibleMoveGenerator.AddAllPossibleMoves(moves, index, piece.piece, piece.team, node, generateQuiet: false);

                BitsBoard threats = default;
                foreach (var move in moves)
                {
                    if (move.moveType == MoveType.Attack)
                        threats[move.target.ToByte()] = true;
                }

                if (piece.team == Team.White)
                {
                    whiteThreats |= threats;
                }
                else if (piece.team == Team.Black)
                {
                    blackThreats |= threats;
                }
            }
        }

        var whitePawnAttacks = whitePawns.Shift(HexNeighborDirection.UpLeft) | whitePawns.Shift(HexNeighborDirection.UpRight);
        var blackPawnAttacks = blackPawns.Shift(HexNeighborDirection.DownLeft) | blackPawns.Shift(HexNeighborDirection.DownRight);

        var whitePawnThreats = whitePawnAttacks & blackPieces;
        var blackPawnThreats = blackPawnAttacks & whitePieces;

        WhitePawnDefends = whitePawnAttacks & whitePieces;
        BlackPawnDefends = blackPawnAttacks & blackPieces;

        WhitePawns = whitePawns;
        BlackPawns = blackPawns;

        WhitePieces = whitePieces;
        BlackPieces = blackPieces;

        WhiteThreats = whiteThreats | whitePawnThreats;
        BlackThreats = blackThreats | blackPawnThreats;

        WhitePawnThreats = whitePawnThreats;
        BlackPawnThreats = blackPawnThreats;
    }
}
