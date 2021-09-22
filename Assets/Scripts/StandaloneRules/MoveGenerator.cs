using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

public static class MoveGenerator
{
    public static bool IsPromotionRank(Team team, Index target)
    {
        HexNeighborDirection forward = team == Team.White ? HexNeighborDirection.Up : HexNeighborDirection.Down;
        return !target.TryGetNeighbor(forward, out Index _);
    }

    public static readonly Piece[] DefendableTypes = new Piece[]
    {
        Piece.King,
        Piece.Queen,
        Piece.KingsKnight,
        Piece.QueensKnight,
        Piece.KingsBishop,
        Piece.QueensBishop,
        Piece.WhiteSquire,
        Piece.GraySquire,
        Piece.BlackSquire,
    };

    public static IEnumerable<(Index target, MoveType moveType)> GetAllTheoreticalAttacks(Index location, Piece piece, Team team, BoardState boardState, IEnumerable<Promotion> promos, bool includeBlocking = false) =>
        SkipInvalidMoves(GetTheroreticalAttacksIncludingInvalid(location, piece, team, boardState, promos, includeBlocking));

    public static IEnumerable<(Index target, MoveType moveType)> GetTheroreticalAttacksIncludingInvalid(Index location, Piece piece, Team team, BoardState boardState, IEnumerable<Promotion> promos, bool includeBlocking = false) => piece switch {
        Piece.King => GetAllKingMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece.Queen => GetAllQueenMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece p when (p == Piece.KingsRook || p == Piece.QueensRook) => GetAllRookMovesIncludingInvalid(location, team, boardState, promos, includeBlocking),
        Piece p when (p == Piece.KingsKnight || p == Piece.QueensKnight) => GetAllKnightMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece p when (p == Piece.KingsBishop || p == Piece. QueensBishop) => GetAllBishopMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece p when (p == Piece.BlackSquire || p == Piece.GraySquire || p == Piece.WhiteSquire) => GetAllSquireMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece p when (p >= Piece.Pawn1) => GetAllPawnTheoreticalAttacksIncludingInvalid(location, team, boardState, includeBlocking),
        _ => throw new ArgumentException($"Unhandled piece type: {piece}", nameof(piece))
    };

    public static IEnumerable<(Index target, MoveType moveType)> GetAllPossibleMoves(Index location, Piece piece, Team team, BoardState boardState, IEnumerable<Promotion> promos, bool includeBlocking = false) => 
        SkipInvalidMoves(GetAllMovesIncludingInvalid(location, piece, team, boardState, promos, includeBlocking));

    public static IEnumerable<(Index target, MoveType moveType)> GetAllMovesIncludingInvalid(Index location, Piece piece, Team team, BoardState boardState, IEnumerable<Promotion> promos, bool includeBlocking = false) => piece switch {
        Piece.King => GetAllKingMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece.Queen => GetAllQueenMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece p when (p == Piece.KingsRook || p == Piece.QueensRook) => GetAllRookMovesIncludingInvalid(location, team, boardState, promos, includeBlocking),
        Piece p when (p == Piece.KingsKnight || p == Piece.QueensKnight) => GetAllKnightMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece p when (p == Piece.KingsBishop || p == Piece. QueensBishop) => GetAllBishopMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece p when (p == Piece.BlackSquire || p == Piece.GraySquire || p == Piece.WhiteSquire) => GetAllSquireMovesIncludingInvalid(location, team, boardState, includeBlocking),
        Piece p when (p >= Piece.Pawn1) => GetAllPawnMovesIncludingInvalid(location, team, boardState, includeBlocking),
        _ => throw new ArgumentException($"Unhandled piece type: {piece}", nameof(piece))
    };

    public static IEnumerable<(Index, MoveType)> SkipInvalidMoves(IEnumerable<(Index index, MoveType type)> allMovesIncludingInvalid) => 
        allMovesIncludingInvalid.Where(move => move.type != MoveType.None);

    public static IEnumerable<(Index, MoveType)> GetAllKingMovesIncludingInvalid(Index location, Team team, BoardState boardState, bool includeBlocking = false)
    {
        foreach(HexNeighborDirection dir in EnumArray<HexNeighborDirection>.Values)
        {
            Index? maybeIndex = location.GetNeighborAt(dir);
            if(maybeIndex == null)
            {
                yield return (Index.invalid, MoveType.None);
                continue;
            }

            Index index = maybeIndex.Value;

            if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
            {
                if(includeBlocking || occupier.team != team)
                {
                    yield return (index, MoveType.Attack);
                    continue;
                }
                else
                {
                    yield return (Index.invalid, MoveType.None);
                    continue;
                }
            }
            yield return (index, MoveType.Move);
        }
    }


    public static IEnumerable<(Index, MoveType)> GetAllSquireMovesIncludingInvalid(Index location, Team team, BoardState boardState, bool includeBlocking = false)
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
            {
                yield return (Index.invalid, MoveType.None);
                continue;
            }

            if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
            {
                if(occupier.team == team && !includeBlocking)
                {
                    yield return (Index.invalid, MoveType.None);
                    continue;
                }

                yield return (index, MoveType.Attack);
            }
            else
                yield return (index, MoveType.Move);
        }
    }

    public static IEnumerable<(Index, MoveType)> GetAllKnightMovesIncludingInvalid(Index location, Team team, BoardState boardState, bool includeBlocking = false)
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
            if(!index.IsInBounds)
            {
                yield return (Index.invalid, MoveType.None);
                continue;
            }

            if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
            {
                if(includeBlocking || occupier.team != team)
                    yield return (index, MoveType.Attack);
                else
                    yield return (Index.invalid, MoveType.None);
            }
            else
                yield return(index, MoveType.Move);
        }
    }

    public static IEnumerable<(Index, MoveType)> GetAllBishopMovesIncludingInvalid(Index location, Team team, BoardState boardState, bool includeBlocking = false)
    {
        List<(Index, MoveType)> possible = new List<(Index, MoveType)>();
        int offset = location.row % 2;

        // Top Left
        for(
            (int row, int col, int i, bool blocked) = (location.row + 1, location.col - offset, 0, false);
            i < 8;
            row++, i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, row, col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));

            if(i % 2 == offset)
                col--;
        }

        // Top Right
        for(
            (int row, int col, int i, bool blocked) = (location.row + 1, location.col + Math.Abs(1 - offset), 0, false);
            i < 8;
            row++, i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, row, col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));

            if(i % 2 != offset)
                col++;
        }
        // Bottom Left
        for(
            (int row, int col, int i, bool blocked) = (location.row - 1, location.col - offset, 0, false);
            i < 8;
            row--, i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, row, col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));

            if(i % 2 == offset)
                col--;
        }
        // Bottom Right
        for(
            (int row, int col, int i, bool blocked) = (location.row - 1, location.col + Math.Abs(1 - offset), 0, false);
            i < 8;
            row--, i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, row, col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));

            if(i % 2 != offset)
                col++;
        }

        return possible;
    }

    public static IEnumerable<(Index, MoveType)> GetAllRookMovesIncludingInvalid(Index location, Team team, BoardState boardState, IEnumerable<Promotion> promos, bool includeBlocking = false)
    {
        List<(Index, MoveType)> possible = new List<(Index, MoveType)>();
        
        // Up
        for(
            (int i, bool blocked) = (1, false);
            i <= 9;
            i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, location.row + 2 * i, location.col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));
        }

        // Down
        for(
            (int i, bool blocked) = (1, false);
            i <= 9;
            i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, location.row - 2 * i, location.col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));
        }

        // Left
        for(
            (int i, bool blocked) = (1, false);
            i <= 4;
            i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, location.row, location.col - i, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));
        }

        // Right
        for(
            (int i, bool blocked) = (1, false);
            i <= 4;
            i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, location.row, location.col + i, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));
        }

        // Check defend
        foreach(HexNeighborDirection dir in EnumArray<HexNeighborDirection>.Values)
        {
            if(dir == HexNeighborDirection.Up || dir == HexNeighborDirection.Down)
            {
                // Up is always possible[0]
                // down is always possible[9]
                int i = dir == HexNeighborDirection.Up ? 0 : 9;

                Index? maybeIndex = location.GetNeighborAt(dir);
                if(maybeIndex == null)
                {
                    possible[i] = (Index.invalid, MoveType.None);
                    continue;
                }

                Index index = maybeIndex.Value;

                if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
                {
                    // can defend -- this should account for promoted pawns as well, todo
                    IEnumerable<Promotion> applicablePromos = promos.Where(promo => promo.team == occupier.team && promo.from == occupier.piece);
                    Piece realPiece = applicablePromos.Any() ? applicablePromos.First().from : occupier.piece;
                    
                    if(occupier.team == team)
                    {
                        if(Contains(DefendableTypes, realPiece))
                            possible[i] = (index, MoveType.Defend);
                        // allied, non-defendable piece
                        else if(includeBlocking)
                            possible[i] = (index, MoveType.Attack);
                        else
                            possible[i] = (Index.invalid, MoveType.None);
                    }
                    // else it's an attack, keep as is
                }
            }
            else
            {
                Index? maybeIndex = location.GetNeighborAt(dir);
                if(maybeIndex == null)
                {
                    possible.Add((Index.invalid, MoveType.None));
                    continue;
                }
                Index index = maybeIndex.Value;

                if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
                {
                    if(occupier.team == team && Contains(DefendableTypes, occupier.piece))
                        possible.Add((index, MoveType.Defend));
                    else
                        possible.Add((Index.invalid, MoveType.None));
                }
                else
                    possible.Add((Index.invalid, MoveType.None));
            }
        }

        return possible;
    }

    public static IEnumerable<(Index, MoveType)> GetAllQueenMovesIncludingInvalid(Index location, Team team, BoardState boardState, bool includeBlocking = false)
    {
        List<(Index, MoveType)> possible = new List<(Index, MoveType)>();
        int offset = location.row % 2;

        // Up
        for(
            (int i, bool blocked) = (1, false);
            i <= 9;
            i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, location.row + 2 * i, location.col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));
        }

        // Down
        for(
            (int i, bool blocked) = (1, false);
            i <= 9;
            i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, location.row - 2 * i, location.col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));
        }

        // Left
        for(
            (int i, bool blocked) = (1, false);
            i <= 4;
            i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, location.row, location.col - i, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));
        }

        // Right
        for(
            (int i, bool blocked) = (1, false);
            i <= 4;
            i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, location.row, location.col + i, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));
        }

        // Top Left
        for(
            (int row, int col, int i, bool blocked) = (location.row + 1, location.col - offset, 0, false);
            i < 8;
            row++, i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, row, col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));

            if(i % 2 == offset)
                col--;
        }

        // Top Right
        for(
            (int row, int col, int i, bool blocked) = (location.row + 1, location.col + Math.Abs(1 - offset), 0, false);
            i < 8;
            row++, i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, row, col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));

            if(i % 2 != offset)
                col++;
        }
        // Bottom Left
        for(
            (int row, int col, int i, bool blocked) = (location.row - 1, location.col - offset, 0, false);
            i < 8;
            row--, i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, row, col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));

            if(i % 2 == offset)
                col--;
        }
        // Bottom Right
        for(
            (int row, int col, int i, bool blocked) = (location.row - 1, location.col + Math.Abs(1 - offset), 0, false);
            i < 8;
            row--, i++
        ){
            if(!blocked)
                blocked = !RayCanMove(team, boardState, row, col, possible, includeBlocking);
            else
                possible.Add((Index.invalid, MoveType.None));

            if(i % 2 != offset)
                col++;
        }

        return possible;
    }

    #region Pawn
    public static IEnumerable<(Index, MoveType)> GetAllPawnMovesIncludingInvalid(Index location, Team team, BoardState boardState, bool includeBlocking = false)
    {
        bool isWhite = team == Team.White;
        Index? leftAttack = location.GetNeighborAt(isWhite ? HexNeighborDirection.UpLeft : HexNeighborDirection.DownLeft);
        Index? rightAttack = location.GetNeighborAt(isWhite ? HexNeighborDirection.UpRight : HexNeighborDirection.DownRight);

        // Check takes
        bool canTakeLeft = leftAttack.HasValue && PawnCanTake(team, leftAttack.Value, boardState, includeBlocking);
        bool canTakeRight = rightAttack.HasValue && PawnCanTake(team, rightAttack.Value, boardState, includeBlocking);

        // Check en passant
        Index? leftPassant = location.GetNeighborAt(isWhite ? HexNeighborDirection.DownLeft : HexNeighborDirection.UpLeft);
        Index? rightPassant = location.GetNeighborAt(isWhite ? HexNeighborDirection.DownRight : HexNeighborDirection.UpRight);
        bool canPassantLeft = leftAttack.HasValue && leftPassant.HasValue && PawnCanPassant(team, leftPassant.Value, boardState);
        bool canPassantRight = rightAttack.HasValue && rightPassant.HasValue && PawnCanPassant(team, rightPassant.Value, boardState);

        if(canTakeLeft)
            yield return (leftAttack.Value, MoveType.Attack);
        else if(canPassantLeft)
            yield return (leftAttack.Value, MoveType.EnPassant);
        else
            yield return (Index.invalid, MoveType.None);
        
        if(canTakeRight)
            yield return (rightAttack.Value, MoveType.Attack);
        else if(canPassantRight)
            yield return (rightAttack.Value, MoveType.EnPassant);
        else
            yield return (Index.invalid, MoveType.None);

        // One forward
        Index? forward = location.GetNeighborAt(isWhite ? HexNeighborDirection.Up : HexNeighborDirection.Down);
        if(forward.HasValue && !boardState.IsOccupied(forward.Value))
        {
            yield return (forward.Value, MoveType.Move);

            // Two forward on 1st move
            Index? twoForward = forward.Value.GetNeighborAt(isWhite ? HexNeighborDirection.Up : HexNeighborDirection.Down);
            if(twoForward.HasValue && PawnIsAtStart(team, location) && !boardState.IsOccupied(twoForward.Value))
                yield return (twoForward.Value, MoveType.Move);
            else
                yield return (Index.invalid, MoveType.None);
        }
        else
        {
            yield return (Index.invalid, MoveType.None);
            yield return (Index.invalid, MoveType.None);
        }
    }

    private static bool PawnCanTake(Team team, Index target, BoardState boardState, bool includeBlocking = false)
    {
        if(target == null)
            return false;

        if(boardState.TryGetPiece(target, out (Team team, Piece piece) occupier))
            return occupier.team != team || includeBlocking;

        return false;
    }

    private static bool PawnCanPassant(Team team, Index victimIndex, BoardState boardState) => boardState.TryGetPiece(victimIndex, out (Team team, Piece piece) occupier) 
        ? occupier.team != team && occupier.piece.IsPawn()
        : false;

    private static bool PawnIsAtStart(Team team, Index location) => team == Team.White 
        ? location.row == 3 || location.row == 4 
        : location.row == 14 || location.row == 15;

    public static IEnumerable<(Index, MoveType)> GetAllPawnTheoreticalAttacksIncludingInvalid(Index location, Team team, BoardState boardState, bool includeBlocking = false)
    {
        bool isWhite = team == Team.White;
        Index? leftAttack = location.GetNeighborAt(isWhite ? HexNeighborDirection.UpLeft : HexNeighborDirection.DownLeft);
        Index? rightAttack = location.GetNeighborAt(isWhite ? HexNeighborDirection.UpRight : HexNeighborDirection.DownRight);

        // Check takes
        bool canTakeLeft = leftAttack.HasValue;
        bool canTakeRight = rightAttack.HasValue;

        // Check en passant
        Index? leftPassant = location.GetNeighborAt(isWhite ? HexNeighborDirection.DownLeft : HexNeighborDirection.UpLeft);
        Index? rightPassant = location.GetNeighborAt(isWhite ? HexNeighborDirection.DownRight : HexNeighborDirection.UpRight);
        bool canPassantLeft = leftAttack.HasValue && leftPassant.HasValue;
        bool canPassantRight = rightAttack.HasValue && rightPassant.HasValue;

        if(canTakeLeft)
            yield return (leftAttack.Value, MoveType.Attack);
        else if(canPassantLeft)
            yield return (leftAttack.Value, MoveType.EnPassant);
        else
            yield return (Index.invalid, MoveType.None);
        
        if(canTakeRight)
            yield return (rightAttack.Value, MoveType.Attack);
        else if(canPassantRight)
            yield return (rightAttack.Value, MoveType.EnPassant);
        else
            yield return (Index.invalid, MoveType.None);

        // One forward
        // Index? forward = location.GetNeighborAt(isWhite ? HexNeighborDirection.Up : HexNeighborDirection.Down);
        // if(forward.HasValue && !boardState.IsOccupied(forward.Value))
        // {
        //     yield return (forward.Value, MoveType.Move);

        //     // Two forward on 1st move
        //     Index? twoForward = forward.Value.GetNeighborAt(isWhite ? HexNeighborDirection.Up : HexNeighborDirection.Down);
        //     if(twoForward.HasValue && PawnIsAtStart(team, location) && !boardState.IsOccupied(twoForward.Value))
        //         yield return (twoForward.Value, MoveType.Move);
        //     else
        //         yield return (Index.invalid, MoveType.None);
        // }
        // else
        // {
        //     yield return (Index.invalid, MoveType.None);
        //     yield return (Index.invalid, MoveType.None);
        // }
    }
    #endregion

    private static bool RayCanMove(Team team, BoardState boardState, int row, int col, List<(Index, MoveType)> possible, bool includeBlocking = false)
    {
        Index index = new Index(row, col);
        if(!index.IsInBounds)
        {
            possible.Add((Index.invalid, MoveType.None));
            return false;
        }

        if(boardState.TryGetPiece(index, out (Team team, Piece piece) occupier))
        {
            if(occupier.team != team || includeBlocking)
                possible.Add((index, MoveType.Attack));
            else
                possible.Add((Index.invalid, MoveType.None));
            return false;
        }
        possible.Add((index, MoveType.Move));
        return true;
    }

    private static bool Contains(Piece[] haystack, Piece needle)
    {
        return System.Array.IndexOf(haystack, needle) >= 0;
    }
}
