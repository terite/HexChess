using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPiece
{
    GameObject obj {get; set;}
    Team team {get; set;}
    PieceType type {get; set;}
    Index location {get; set;}
    bool captured {get; set;}
    List<(Hex, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState);
    void MoveTo(Hex hex);
    void Init(Team team, PieceType type, Index startingLocation);
}

public enum MoveType {
    Move = 0, Defend = 1, Attack = 2, EnPassant = 3
}

public enum PieceType {
    King, Queen, KingsRook, QueensRook, KingsKnight, QueensKnight, KingsBishop, QueensBishop, WhiteSquire, GraySquire, BlackSquire, 
    Pawn1, Pawn2, Pawn3, Pawn4, Pawn5, Pawn6, Pawn7, Pawn8
}