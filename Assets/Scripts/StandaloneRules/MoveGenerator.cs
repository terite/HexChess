using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoveGenerator
{
    public static IEnumerable<(Index, MoveType)> GetAllPossibleKingMoves(Index location, Team team, BoardState boardState, bool includeBlocking = false)
    {
        foreach(HexNeighborDirection dir in EnumArray<HexNeighborDirection>.Values)
        {
            Index? maybeIndex = location.GetNeighborAt(dir);
            if(maybeIndex == null)
                continue;

            Index index = maybeIndex.Value;

            if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
            {
                if(includeBlocking || occupier.team != team)
                {
                    yield return (index, MoveType.Attack);
                    continue;
                }
                else
                    continue;
            }
            yield return (index, MoveType.Move);
        }
    }
}
