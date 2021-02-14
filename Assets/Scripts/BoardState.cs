using System.Collections.Generic;
using UnityEngine.Serialization;

public struct BoardState
{
    public Team currentMove;
    [FormerlySerializedAs("biDirPiecePositions")]
    public BidirectionalDictionary<(Team, Piece), Index> allPiecePositions;

    public Team check;
    public Team checkmate;

    // Json (including newtonsoft) can not properly serialize a dictionary that uses a key that is any type other than than a string.
    // To get around this, we convert our Bidirectional dictionary into a list, then serialze that list.
    public List<TeamPieceLoc> GetSerializeable()
    {
        List<TeamPieceLoc> list = new List<TeamPieceLoc>();
        foreach(KeyValuePair<(Team, Piece), Index> kvp in allPiecePositions)
        {
            (Team team, Piece piece) = kvp.Key;
            list.Add(new TeamPieceLoc{t = team, p = piece, i = kvp.Value});
        }
        return list;
    }

    // When deserializing from json, because of the before mentioned dictionary issues, we must deserialize as a list, then construct our dictionary from it.
    public static BoardState GetBoardStateFromDeserializedDict(List<TeamPieceLoc> list, Team currentMove, Team check, Team checkmate)
    {
        BidirectionalDictionary<(Team, Piece), Index> newDict = new BidirectionalDictionary<(Team, Piece), Index>();
        foreach(TeamPieceLoc tpl in list)
            newDict.Add((tpl.t, tpl.p), tpl.i);

        return new BoardState{allPiecePositions = newDict, currentMove = currentMove, check = check, checkmate = checkmate};
    }
}

[System.Serializable]
public struct TeamPieceLoc {
    public Team t;
    public Piece p;
    public Index i;
}