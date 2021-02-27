using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public struct Game 
{
    public Winner winner;
    public List<BoardState> turnHistory;
    public int turns => Mathf.CeilToInt((float)turnHistory.Count / 2f);

    public Game(List<BoardState> history)
    {
        turnHistory = history;
        winner = Winner.Pending;
    }

    public static string Serialize(List<BoardState> turnHistory)
    {
        List<(Team, List<SerializedPiece>, Team, Team)> serializeableGame = new List<(Team, List<SerializedPiece>, Team, Team)>();
        foreach(BoardState bs in turnHistory)
        {
            List<SerializedPiece> serializeableBoardState = bs.GetSerializeable();
            serializeableGame.Add((bs.currentMove, serializeableBoardState, bs.check, bs.checkmate));
        }
        return JsonConvert.SerializeObject(serializeableGame);
    }

    public static Game Deserialize(string json)
    {
        List<BoardState> history = new List<BoardState>();
        
        List<(Team, List<SerializedPiece>, Team, Team)> boards = JsonConvert.DeserializeObject<List<(Team, List<SerializedPiece>, Team, Team)>>(json);
        foreach((Team, List<SerializedPiece>, Team, Team) board in boards)
            history.Add(BoardState.GetBoardStateFromDeserializedGame(board.Item2, board.Item1, board.Item3, board.Item4));
        return new Game(history);
    }
}

public enum Winner {
    Pending = 0,
    White = 1,
    Black = 2,
    Draw = 3
}