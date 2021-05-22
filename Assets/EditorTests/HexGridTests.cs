using NUnit.Framework;

public class HexGridTests
{
    [Test]
    public void InBoundsTest()
    {
        var grid = new HexGrid() { rows = 19, cols = 5 };

        Assert.True(grid.IsInBounds(new Index(0, 0)));
        Assert.True(grid.IsInBounds(new Index(18, 3)));
        Assert.True(grid.IsInBounds(new Index(17, 4)));
    }
    [Test]
    public void OutOfBoundsTest()
    {
        var grid = new HexGrid() { rows = 19, cols = 5 };

        Assert.False(grid.IsInBounds(new Index(-1, 0)));
        Assert.False(grid.IsInBounds(new Index(0, -1)));
        Assert.False(grid.IsInBounds(new Index(18, 4)));
    }
}
