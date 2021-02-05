using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPiece
{
    GameObject obj {get; set;}
    Team team {get; set;}
    PieceType type {get; set;}
    Index location {get; set;}
    List<Hex> GetAllPossibleMoves(HexSpawner boardSpawner, BoardState boardState);
    void MoveTo(Hex hex);
    void Init(Team team, PieceType type, Index startingLocation);
}

public enum PieceType {
    King, Queen, KingsRook, QueensRook, KingsKnight, QueensKnight, KingsBishop, QueensBishop, WhiteSquire, GraySquire, BlackSquire, 
    Pawn1, Pawn2, Pawn3, Pawn4, Pawn5, Pawn6, Pawn7, Pawn8
}