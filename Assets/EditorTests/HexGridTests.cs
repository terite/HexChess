using NUnit.Framework;

public class HexGridTests
{
    [Test]
    public void InBoundsTest()
    {
        Assert.True(HexGrid.IsInBounds(new Index(0, 0)));
        Assert.True(HexGrid.IsInBounds(new Index(18, 3)));
        Assert.True(HexGrid.IsInBounds(new Index(17, 4)));
    }
    [Test]
    public void OutOfBoundsTest()
    {
        Assert.False(HexGrid.IsInBounds(new Index(-1, 0)));
        Assert.False(HexGrid.IsInBounds(new Index(0, -1)));
        Assert.False(HexGrid.IsInBounds(new Index(18, 4)));
    }
    [Test]
    public void GetNeighborTests()
    {
        Index middle = new Index(5, 'E');

        Assert.AreEqual(new Index(6, 'E'), HexGrid.GetNeighborAt(middle, HexNeighborDirection.Up));
        Assert.AreEqual(new Index(6, 'F'), HexGrid.GetNeighborAt(middle, HexNeighborDirection.UpRight));
        Assert.AreEqual(new Index(5, 'F'), HexGrid.GetNeighborAt(middle, HexNeighborDirection.DownRight));
        Assert.AreEqual(new Index(4, 'E'), HexGrid.GetNeighborAt(middle, HexNeighborDirection.Down));
        Assert.AreEqual(new Index(5, 'D'), HexGrid.GetNeighborAt(middle, HexNeighborDirection.DownLeft));
        Assert.AreEqual(new Index(6, 'D'), HexGrid.GetNeighborAt(middle, HexNeighborDirection.UpLeft));
    }
}
