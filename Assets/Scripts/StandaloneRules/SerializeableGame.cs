using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

[System.Serializable]
public struct SerializeableGame
{
    public static readonly string defaultGameFileLoc = "Assets/Resources/DefaultBoardState.json";
    public static readonly SerializeableGame defaultGame = SerializeableGame.Deserialize(File.ReadAllText(defaultGameFileLoc));
    public static readonly (Team team, List<SerializedPiece> pieces, Team check, Team checkmate, float duration) defaultSerializedBoard = defaultGame.serializedBoards.FirstOrDefault();

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

    public static SerializeableGame Deserialize(string json) => JsonConvert.DeserializeObject<SerializeableGame>(json);

    public List<BoardState> GetHistory()
    {
        List<BoardState> history = new List<BoardState>();
        foreach((Team team, List<SerializedPiece> pieces, Team check, Team checkmate, float duration) in serializedBoards)
            history.Add(BoardState.GetBoardStateFromDeserializedBoard(pieces, team, check, checkmate, duration));
        return history;
    }
}