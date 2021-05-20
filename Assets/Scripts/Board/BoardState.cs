using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

public struct BoardState
{
    public Team currentMove;
    [FormerlySerializedAs("biDirPiecePositions")]
    public BidirectionalDictionary<(Team, Piece), Index> allPiecePositions;

    public Team check;
    public Team checkmate;
    public float executedAtTime;

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

                (Team previousTeamAtLocation, Piece? previousPieceAtLocation) = lastState.allPiecePositions.Contains(nowPos)
                    ? lastState.allPiecePositions[nowPos] 
                    : (Team.None, (Piece?)null);

                if(kvp.Value != nowPos)
                    return new Move(
                        turn: Mathf.FloorToInt((float)history.Count / 2f),
                        lastTeam: kvp.Key.Item1, 
                        lastPiece: kvp.Key.Item2, 
                        from: kvp.Value, 
                        to: nowPos, 
                        capturedPiece: previousTeamAtLocation == kvp.Key.Item1 ? null : previousPieceAtLocation, 
                        defendedPiece: previousTeamAtLocation != kvp.Key.Item1 ? null : previousPieceAtLocation,
                        duration: nowState.executedAtTime - lastState.executedAtTime
                    );
            }
        }
        return new Move(0, Team.None, Piece.King, default(Index), default(Index));
    }

    public static bool operator ==(BoardState a, BoardState b) => 
        a.currentMove == b.currentMove 
        && a.allPiecePositions.Count == b.allPiecePositions.Count 
        && !a.allPiecePositions.Except(b.allPiecePositions).Any();
    public static bool operator !=(BoardState a, BoardState b) => !(a == b);
    
    public override bool Equals(object obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    public override string ToString() => base.ToString();

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
                checkmate = checkmate,
                executedAtTime = executedAtTime
            })
        );

    // When deserializing from json, because of the before mentioned dictionary issues, we must deserialize as a list, then construct our dictionary from it.
    public static BoardState GetBoardStateFromDeserializedBoard(List<SerializedPiece> list, Team currentMove, Team check, Team checkmate, float executedAtTime)
    {
        BidirectionalDictionary<(Team, Piece), Index> newDict = new BidirectionalDictionary<(Team, Piece), Index>();
        foreach(SerializedPiece tpl in list)
            newDict.Add((tpl.t, tpl.p), tpl.i);

        return new BoardState{allPiecePositions = newDict, currentMove = currentMove, check = check, checkmate = checkmate, executedAtTime = executedAtTime};
    }

    public static BoardState Deserialize(byte[] data)
    {
        string json = Encoding.ASCII.GetString(data);
        SerializedBoard boardstate = JsonConvert.DeserializeObject<SerializedBoard>(json);
        
        BidirectionalDictionary<(Team, Piece), Index> newDict = new BidirectionalDictionary<(Team, Piece), Index>();
        foreach(SerializedPiece tpl in boardstate.pieces)
            newDict.Add((tpl.t, tpl.p), tpl.i);

        return new BoardState {
            allPiecePositions = newDict,
            currentMove = boardstate.currentMove,
            check = boardstate.check,
            checkmate = boardstate.checkmate,
            executedAtTime = boardstate.executedAtTime
        };
    }
}

[System.Serializable]
public struct SerializedBoard {
    public List<SerializedPiece> pieces;
    public Team currentMove;
    public Team check;
    public Team checkmate;
    public float executedAtTime;
}

[System.Serializable]
public struct SerializedPiece {
    public Team t;
    public Piece p;
    public Index i;
}