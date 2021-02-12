public struct BoardState
{
    public Team currentMove;
    public BidirectionalDictionary<(Team, Piece), Index> biDirPiecePositions;
}