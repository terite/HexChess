using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoardStateTests
{
    private static BoardState CreateBoardState(IEnumerable<(Index location, Team team, Piece piece)> pieces)
    {
        var allPiecePositions = new BidirectionalDictionary<(Team, Piece), Index>();
        foreach (var piece in pieces)
        {
            allPiecePositions.Add((piece.team, piece.piece), piece.location);
        }

        return new BoardState(allPiecePositions, Team.White, Team.None, Team.None, 0);
    }

    [Test]
    public void NoPiecesTest()
    {
        var bs = CreateBoardState(Array.Empty<(Index, Team, Piece)>());
        Assert.False(bs.IsChecking(Team.White, null));
        Assert.False(bs.IsChecking(Team.Black, null));
    }
    [Test]
    public void KingOnlyNoCheckTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(1, 'E'), Team.White, Piece.King),
            (new Index(9, 'E'), Team.Black, Piece.King),
        });

        Assert.False(bs.IsChecking(Team.White, null));
        Assert.False(bs.IsChecking(Team.Black, null));
    }

    [Test]
    public void KingCheckEachOtherTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(5, 'E'), Team.White, Piece.King),
            (new Index(6, 'E'), Team.Black, Piece.King),
        });

        Assert.True(bs.IsChecking(Team.White, null));
        Assert.True(bs.IsChecking(Team.Black, null));
    }

    [Test]
    public void PawnCheckTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(5, 'F'), Team.White, Piece.Pawn1),
        });

        Assert.True(bs.IsChecking(Team.White, null));
        Assert.False(bs.IsChecking(Team.Black, null));
    }

    [Test]
    public void PawnPromotionCheckTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(1, 'E'), Team.White, Piece.Pawn1),
        });

        Assert.False(bs.IsChecking(Team.Black, null));
        Assert.False(bs.IsChecking(Team.White, null));
        Assert.True(bs.IsChecking(Team.White, new List<Promotion>() { new Promotion(Team.White, Piece.Pawn1, Piece.Queen, 0) }));
        Assert.False(bs.IsChecking(Team.Black, new List<Promotion>() { new Promotion(Team.White, Piece.Pawn1, Piece.Queen, 0) }));
    }
}
