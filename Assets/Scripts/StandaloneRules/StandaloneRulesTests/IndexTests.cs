using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class IndexTests
{
    [Test]
    public void RowColConstructorTest()
    {
        var b1 = new Index(0, 0);
        Assert.AreEqual("B1", b1.GetKey());
        var d1 = new Index(0, 1);
        Assert.AreEqual("D1", d1.GetKey());
        var f1 = new Index(0, 2);
        Assert.AreEqual("F1", f1.GetKey());
        var h1 = new Index(0, 3);
        Assert.AreEqual("H1", h1.GetKey());

        var a1 = new Index(1, 0);
        Assert.AreEqual("A1", a1.GetKey());
        var c1 = new Index(1, 1);
        Assert.AreEqual("C1", c1.GetKey());
        var e1 = new Index(1, 2);
        Assert.AreEqual("E1", e1.GetKey());
        var g1 = new Index(1, 3);
        Assert.AreEqual("G1", g1.GetKey());
        var i1 = new Index(1, 4);
        Assert.AreEqual("I1", i1.GetKey());
    }
    [Test]
    public void RankAndFileConstructorTest()
    {
        var a1 = new Index(1, 'A');
        Assert.AreEqual("A1", a1.GetKey());

        foreach (byte rank in new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            foreach (char file in new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' })
            {
                var index = new Index(rank, file);
                Assert.AreEqual($"{file}{rank}", index.GetKey());
            }
    }

    [Test]
    public void RankAndFileConstructorRangeTest()
    {
        // file must be uppercase
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Index(1, 'a');
        });


        // Rank must be 1-10
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Index(0, 'A');
        });
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Index(11, 'A');
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Index(1, 'J');
        });

        // Only valid rank 10 files are B, D, F, H
        var invalidFiles = new char[] { 'A', 'C', 'E', 'G', 'I' };
        foreach (var invalidFile in invalidFiles)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Index(10, invalidFile);
            });
        }

    }

    [Test]
    public void InBoundsTest()
    {
        Assert.True(new Index(0, 0).IsInBounds);
        Assert.True(new Index(18, 3).IsInBounds);
        Assert.True(new Index(17, 4).IsInBounds);
    }
    [Test]
    public void OutOfBoundsTest()
    {
        Assert.False(new Index(-1, 0).IsInBounds);
        Assert.False(new Index(0, -1).IsInBounds);
        Assert.False(new Index(18, 4).IsInBounds);
    }
    [Test]
    public void GetNeighborTests()
    {
        Index middle = new Index(5, 'E');

        Assert.AreEqual(new Index(6, 'E'), middle.GetNeighborAt(HexNeighborDirection.Up));
        Assert.AreEqual(new Index(6, 'F'), middle.GetNeighborAt(HexNeighborDirection.UpRight));
        Assert.AreEqual(new Index(5, 'F'), middle.GetNeighborAt(HexNeighborDirection.DownRight));
        Assert.AreEqual(new Index(4, 'E'), middle.GetNeighborAt(HexNeighborDirection.Down));
        Assert.AreEqual(new Index(5, 'D'), middle.GetNeighborAt(HexNeighborDirection.DownLeft));
        Assert.AreEqual(new Index(6, 'D'), middle.GetNeighborAt(HexNeighborDirection.UpLeft));
    }
}
