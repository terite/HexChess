using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Serialization;

public struct BoardState
{
    public Team currentMove;
    [FormerlySerializedAs("biDirPiecePositions")]
    public BidirectionalDictionary<(Team, Piece), Index> allPiecePositions;

    public Team check;
    public Team checkmate;

    public static Move GetLastMove(List<BoardState> history)
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
                    return new Move(kvp.Key.Item1, kvp.Key.Item2, kvp.Value, nowPos);
            }
        }
        return new Move(Team.None, Piece.King, default(Index), default(Index));
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

    public byte[] Serialize() =>
        Encoding.ASCII.GetBytes(
            JsonConvert.SerializeObject(new SerializedBoard{
                pieces = GetSerializeable(),
                currentMove = currentMove,
                check = check,
                checkmate = checkmate
            })
        );

    // When deserializing from json, because of the before mentioned dictionary issues, we must deserialize as a list, then construct our dictionary from it.
    public static BoardState GetBoardStateFromDeserializedBoard(List<SerializedPiece> list, Team currentMove, Team check, Team checkmate)
    {
        BidirectionalDictionary<(Team, Piece), Index> newDict = new BidirectionalDictionary<(Team, Piece), Index>();
        foreach(SerializedPiece tpl in list)
            newDict.Add((tpl.t, tpl.p), tpl.i);

        return new BoardState{allPiecePositions = newDict, currentMove = currentMove, check = check, checkmate = checkmate};
    }

    public static BoardState Deserialize(byte[] data)
    {
        string json = Encoding.ASCII.GetString(data);
        SerializedBoard board = JsonConvert.DeserializeObject<SerializedBoard>(json);
        
        BidirectionalDictionary<(Team, Piece), Index> newDict = new BidirectionalDictionary<(Team, Piece), Index>();
        foreach(SerializedPiece tpl in board.pieces)
            newDict.Add((tpl.t, tpl.p), tpl.i);

        return new BoardState {
            allPiecePositions = newDict,
            currentMove = board.currentMove,
            check = board.check,
            checkmate = board.checkmate
        };
    }
}

[System.Serializable]
public struct SerializedBoard {
    public List<SerializedPiece> pieces;
    public Team currentMove;
    public Team check;
    public Team checkmate;
}

[System.Serializable]
public struct SerializedPiece {
    public Team t;
    public Piece p;
    public Index i;
}