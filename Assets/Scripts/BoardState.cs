using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Serialization;

public struct BoardState
{
    public Team currentMove;
    [FormerlySerializedAs("biDirPiecePositions")]
    public BidirectionalDictionary<(Team, Piece), Index> allPiecePositions;

    public Team check;
    public Team checkmate;

    public static (Team lastTeam, Piece lastPiece, Index from, Index to) GetLastMove(List<BoardState> history)
    {
        if(history.Count > 1)
        {
            BoardState lastState = history[history.Count - 2];
            BoardState nowState = history[history.Count - 1];
            foreach(KeyValuePair<(Team, Piece), Index> kvp in lastState.allPiecePositions)
            {
                if(!nowState.allPiecePositions.Contains(kvp.Key))
                    continue;
                Index nowPos = nowState.allPiecePositions[kvp.Key];
                if(kvp.Value != nowPos)
                    return (kvp.Key.Item1, kvp.Key.Item2, kvp.Value, nowPos);
            }
        }
        return(Team.None, Piece.King, default(Index), default(Index));
    }

    // Json (including newtonsoft) can not properly serialize a dictionary that uses a key that is any type other than than a string.
    // To get around this, we convert our Bidirectional dictionary into a list, then serialze that list.
    public List<SerializedPiece> GetSerializeable()
    {
        List<SerializedPiece> list = new List<SerializedPiece>();
        foreach(KeyValuePair<(Team, Piece), Index> kvp in allPiecePositions)
        {
            (Team team, Piece piece) = kvp.Key;
            list.Add(new SerializedPiece{t = team, p = piece, i = kvp.Value});
        }
        return list;
    }

    // When deserializing from json, because of the before mentioned dictionary issues, we must deserialize as a list, then construct our dictionary from it.
    public static BoardState GetBoardStateFromDeserializedGame(List<SerializedPiece> list, Team currentMove, Team check, Team checkmate)
    {
        BidirectionalDictionary<(Team, Piece), Index> newDict = new BidirectionalDictionary<(Team, Piece), Index>();
        foreach(SerializedPiece tpl in list)
            newDict.Add((tpl.t, tpl.p), tpl.i);

        return new BoardState{allPiecePositions = newDict, currentMove = currentMove, check = check, checkmate = checkmate};
    }
}

[System.Serializable]
public struct SerializedPiece {
    public Team t;
    public Piece p;
    public Index i;
}