[System.Serializable]
public struct Move
{
    public Team lastTeam;
    public Piece lastPiece;
    public Index from;
    public Index to;

    public Move(Team lastTeam, Piece lastPiece, Index from, Index to)
    {
        this.lastTeam = lastTeam;
        this.lastPiece = lastPiece;
        this.from = from;
        this.to = to;
    }
}