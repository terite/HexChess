using System.Collections.Generic;
using Sirenix.OdinInspector;

public struct BoardState
{
    public Team currentMove;
    public Dictionary<(Team, PieceType), Index> piecePositions;
    public BidirectionalDictionary<(Team, PieceType), Index> bidPiecePositions;

    [Button]
    public void CopyToBiDict()
    {
        bidPiecePositions.Clear();
        foreach(KeyValuePair<(Team, PieceType), Index> kvp in piecePositions)
            bidPiecePositions.Add(kvp);
    }
}