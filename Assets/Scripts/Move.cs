[System.Serializable]
public struct Move
{
    public Team lastTeam;
    public Piece lastPiece;
    public Index from;
    public Index to;
    public Piece? capturedPiece;
    public Piece? defendedPiece;

    public Move(Team lastTeam, Piece lastPiece, Index from, Index to, Piece? capturedPiece = null, Piece? defendedPiece = null)
    {
        this.lastTeam = lastTeam;
        this.lastPiece = lastPiece;
        this.from = from;
        this.to = to;
        this.capturedPiece = capturedPiece;
        this.defendedPiece = defendedPiece;
    }
}