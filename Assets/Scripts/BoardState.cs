using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoardState
{
    public Team currentMove;
    public Dictionary<(Team, PieceType), (int, int)> piecePositions;
}