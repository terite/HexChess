using System.Collections.Generic;

[System.Serializable]
public struct SerializeableGame
{
    public List<(Team, List<SerializedPiece>, Team, Team, float)> serializedBoards;
    public List<Promotion> promotions;
    public Winner winner;
    public GameEndType endType;
    public float timerDuration;
    public bool hasClock;

    public SerializeableGame(
        List<(Team, List<SerializedPiece>, Team, Team, float)> serializedBoards, 
        List<Promotion> promotions, 
        Winner winner = Winner.Pending, 
        GameEndType endType = GameEndType.Pending,
        float timerDuration = 0,
        bool hasClock = false
    )
    {
        this.serializedBoards = serializedBoards;
        this.promotions = promotions;
        this.winner = winner;
        this.endType = endType;
        this.timerDuration = timerDuration;
        this.hasClock = hasClock;
    }
}