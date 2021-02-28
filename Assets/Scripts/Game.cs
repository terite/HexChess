using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public struct Game 
{
    public Winner winner;
    public List<BoardState> turnHistory;
    public List<Promotion> promotions;
    public int turns => Mathf.FloorToInt((float)turnHistory.Count / 2f) + 1;

    public Game(List<BoardState> history, List<Promotion> promotions = null)
    {
        turnHistory = history;
        winner = Winner.Pending;
        this.promotions = promotions == null ? new List<Promotion>() : promotions;
    }

    public static string Serialize(List<BoardState> turnHistory, List<Promotion> promotions)
    {
        List<(Team, List<SerializedPiece>, Team, Team)> serializeableBoards = new List<(Team, List<SerializedPiece>, Team, Team)>();
        foreach(BoardState bs in turnHistory)
        {
            List<SerializedPiece> serializeableBoardState = bs.GetSerializeable();
            serializeableBoards.Add((bs.currentMove, serializeableBoardState, bs.check, bs.checkmate));
        }

        return JsonConvert.SerializeObject(new SerializeableGame(serializeableBoards, promotions));
    }

    public static Game Deserialize(string json)
    {
        List<BoardState> history = new List<BoardState>();
        
        SerializeableGame game = JsonConvert.DeserializeObject<SerializeableGame>(json);
        foreach((Team, List<SerializedPiece>, Team, Team) board in game.serializedBoards)
            history.Add(BoardState.GetBoardStateFromDeserializedGame(board.Item2, board.Item1, board.Item3, board.Item4));
        return new Game(history,game.promotions);
    }
}

[System.Serializable]
public struct SerializeableGame
{
    public List<(Team, List<SerializedPiece>, Team, Team)> serializedBoards;
    public List<Promotion> promotions;

    public SerializeableGame(List<(Team, List<SerializedPiece>, Team, Team)> serializedBoards, List<Promotion> promotions)
    {
        this.serializedBoards = serializedBoards;
        this.promotions = promotions;
    }
}

[System.Serializable]
public struct Promotion
{
    public Team team;
    public Piece from;
    public Piece to;
}

public enum Winner {
    Pending = 0,
    White = 1,
    Black = 2,
    Draw = 3
}