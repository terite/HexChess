using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

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
    }
}
