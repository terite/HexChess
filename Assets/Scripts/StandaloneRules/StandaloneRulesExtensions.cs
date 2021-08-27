using System;

namespace Extensions
{
    public static class StandaloneRulesExtensions 
    {
        public static HexNeighborDirection OppositeDirection(this HexNeighborDirection direction) =>
            (HexNeighborDirection)(((int)direction + 3) % 6);

        public static string GetPieceShortString(this Piece piece) => piece switch{
            Piece.King => "k", Piece.Queen => "q",
            Piece p when (p == Piece.KingsRook || p == Piece.QueensRook) => "r",
            Piece p when (p == Piece.KingsKnight || p == Piece.QueensKnight) => "n",
            Piece p when (p == Piece.KingsBishop || p == Piece.QueensBishop) => "b",
            Piece p when (p == Piece.BlackSquire || p == Piece.GraySquire || p == Piece.WhiteSquire) => "s",
            Piece p when (p >= Piece.Pawn1) => "p",
            _ => ""
        };

        public static string GetPieceLongString(this Piece piece) => piece switch {
            Piece.King => "King",
            Piece.Queen => "Queen",
            Piece p when (p == Piece.KingsKnight || p == Piece.QueensKnight) => "Knight",
            Piece p when (p == Piece.KingsRook || p == Piece.QueensRook) => "Rook",
            Piece p when (p == Piece.KingsBishop || p == Piece.QueensBishop) => "Bishop",
            Piece p when (p == Piece.WhiteSquire || p == Piece.GraySquire || p == Piece.BlackSquire) => "Squire",
            Piece p when (p >= Piece.Pawn1) => "Pawn",
            _ => ""
        };

        public static bool IsPawn(this Piece piece) => piece >= Piece.Pawn1;
        public static bool IsBishop(this Piece piece) => piece == Piece.KingsBishop || piece == Piece.QueensBishop;
        public static bool IsRook(this Piece piece) => piece == Piece.KingsRook || piece == Piece.QueensRook;
        public static bool IsKnight(this Piece piece) => piece == Piece.KingsKnight || piece == Piece.QueensKnight;
        public static bool IsSquire(this Piece piece) => piece == Piece.WhiteSquire || piece == Piece.GraySquire || piece == Piece.BlackSquire;

        public static Team Enemy(this Team team) => team == Team.White ? Team.Black : Team.White;

        public static int GetMaterialValue(this Piece piece) => piece switch {
            Piece.Queen => 9,
            Piece p when (p == Piece.QueensRook || p == Piece.KingsRook) => 5,
            Piece p when (p == Piece.QueensBishop || p == Piece.KingsBishop || p == Piece.QueensKnight) || p == Piece.KingsKnight=> 3,
            Piece p when (p == Piece.BlackSquire || p == Piece.GraySquire || p == Piece.WhiteSquire) => 2,
            Piece p when (p >= Piece.Pawn1) => 1,
            _ => 0
        };

        public static int GetMaxMoveCount(this Piece piece) => piece switch {
            Piece.King => 6,
            Piece.Queen => 58,
            Piece p when (p == Piece.BlackSquire || p == Piece.GraySquire || p == Piece.WhiteSquire) => 6,
            Piece p when (p == Piece.KingsBishop || p == Piece.QueensBishop) => 32,
            Piece p when (p == Piece.KingsRook || p == Piece.QueensRook) => 30,
            Piece p when (p == Piece.KingsKnight || p == Piece.QueensKnight) => 12,
            Piece p when (p >= Piece.Pawn1) => 4,
            _ => 0
        };

        public static Piece[] GetAlternates(this Piece piece) => piece switch{
            Piece.BlackSquire => new Piece[2]{Piece.WhiteSquire, Piece.GraySquire},
            Piece.WhiteSquire => new Piece[2]{Piece.BlackSquire, Piece.GraySquire},
            Piece.GraySquire => new Piece[2]{Piece.BlackSquire, Piece.WhiteSquire},
            Piece.KingsBishop => new Piece[1]{Piece.QueensBishop},
            Piece.QueensBishop => new Piece[1]{Piece.KingsBishop},
            Piece.KingsRook => new Piece[1]{Piece.QueensRook},
            Piece.QueensRook => new Piece[1]{Piece.KingsRook},
            Piece.KingsKnight => new Piece[1]{Piece.QueensKnight},
            Piece.QueensKnight => new Piece[1]{Piece.KingsKnight},
            _ => Array.Empty<Piece>()
        };
    }
}
