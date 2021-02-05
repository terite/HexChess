using System.Collections.Generic;
using Sirenix.OdinInspector;

public struct BoardState
{
    public Team currentMove;
    public BidirectionalDictionary<(Team, PieceType), Index> biDirPiecePositions;
}