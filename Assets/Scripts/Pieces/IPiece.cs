using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPiece
{
    GameObject obj {get; set;}
    Team team {get; set;}
    Piece piece {get; set;}
    Index location {get; set;}
    bool captured {get; set;}
    ushort value {get; set;}
    List<(Hex, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState, bool includeBlocking = false);
    void MoveTo(Hex hex);
    void Init(Team team, Piece piece, Index startingLocation);
}

public enum Piece {
    King, Queen, KingsRook, QueensRook, KingsKnight, QueensKnight, KingsBishop, QueensBishop, WhiteSquire, GraySquire, BlackSquire, 
    Pawn1, Pawn2, Pawn3, Pawn4, Pawn5, Pawn6, Pawn7, Pawn8
}