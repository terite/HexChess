using System;
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
    IEnumerable<(Index target, MoveType moveType)> GetAllPossibleMoves(BoardState boardState, bool includeBlocking = false);
    void MoveTo(Hex hex, Action action = null);
    void CancelMove();
    void Init(Team team, Piece piece, Index startingLocation);
    string GetPieceString();
}