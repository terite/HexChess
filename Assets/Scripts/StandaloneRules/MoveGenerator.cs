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

    public static IEnumerable<(Index, MoveType)> GetAllPossibleSquireMoves(Index location, Team team, BoardState boardState, bool includeBlocking = false)
    {
        int squireOffset = location.row % 2 == 0 ? 1 : -1;
        var possible = new (int row, int col)[] {
            (location.row + 3, location.col + squireOffset),
            (location.row - 3, location.col + squireOffset),
            (location.row + 3, location.col),
            (location.row - 3, location.col),
            (location.row, location.col + 1),
            (location.row, location.col - 1)
        };

        foreach((int row, int col) in possible)
        {
            Index index = new Index(row, col);
            if(!index.IsInBounds)
                continue;

            if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
            {
                if (occupier.team == team && !includeBlocking)
                    continue;

                yield return (index, MoveType.Attack);
            }
            else
                yield return (index, MoveType.Move);
        }
    }

    public static IEnumerable<(Index, MoveType)> GetAllPossibleKnightMoves(Index location, Team team, BoardState boardState, bool includeBlocking = false)
    {
        int offset = location.row % 2 == 0 ? 1 : -1;
        var possibleMoves = new (int row, int col)[] {
            (location.row + 5, location.col),
            (location.row + 5, location.col + offset),
            (location.row + 4, location.col + offset),
            (location.row + 4, location.col - offset),
            (location.row + 1, location.col + (2 * offset)),
            (location.row + 1, location.col - offset),
            (location.row - 1, location.col - offset),
            (location.row - 1, location.col + (2 * offset)),
            (location.row - 4, location.col - offset),
            (location.row - 4, location.col + offset),
            (location.row - 5, location.col + offset),
            (location.row - 5, location.col),
        };

        foreach ((int row, int col) in possibleMoves)
        {
            Index index = new Index(row, col);
            if (!index.IsInBounds)
                continue;

            if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
            {
                if(includeBlocking || occupier.team != team)
                    yield return (index, MoveType.Attack);
            }
            else
            {
                yield return(index, MoveType.Move);
            }
        }
    }
}
