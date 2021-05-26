using Sirenix.OdinInspector;
using UnityEngine;
using Extensions;
using System.Collections.Generic;
using System;

public struct HexGrid
{
    public const int cols = 5;
    public static int maxRow => rows - 1;
    public const int rows = 19;
    public static int maxCol => cols - 1;
    public float radius;
    public float height;
    [SerializeField, MinMaxSlider(-64, 64, true)] private Vector2 hexHeightVariance;
    [ShowInInspector, ReadOnly] public float minHeight => hexHeightVariance.x;
    [ShowInInspector, ReadOnly] public float maxHeight => hexHeightVariance.y;

    public List<Color> colors;

    public float Apothem =>
       (radius.Sqr() - (radius * 0.5f).Sqr()).Sqrt();

    public static bool IsInBounds(int row, int col) {
        bool cond1 = row * (row - maxRow) <= 0 && col* (col - maxCol) <= 0;
        bool cond2 = !(cols % 2 != 0 && col == cols - 1 && row % 2 == 0);
        return cond1 && cond2;
    }
    public static bool IsInBounds(Index index) => IsInBounds(index.row, index.col);

    public static bool GetValidIndex(int row, int col, out Index index)
    {
        if (!IsInBounds(row, col)) {
            index = default;
            return false;
        }
        index = new Index(row, col);
        return true;
    }

    public static Index? GetNeighborAt(Index source, HexNeighborDirection dir)
    {
        bool isEven = source.row % 2 == 0;
        (int row, int col) offsets = dir switch
        {
            HexNeighborDirection.Up => (2, 0),
            HexNeighborDirection.UpRight => isEven ? (1, 1) : (1, 0),
            HexNeighborDirection.DownRight => isEven ? (-1, 1) : (-1, 0),
            HexNeighborDirection.Down => (-2, 0),
            HexNeighborDirection.DownLeft => isEven ? (-1, 0) : (-1, -1),
            HexNeighborDirection.UpLeft => isEven ? (1, 0) : (1, -1),
            _ => (-100, -100)
        };

        Index neighbor = new Index(source.row + offsets.row, source.col + offsets.col);
        if (IsInBounds(neighbor))
            return neighbor;
        return null;
    }
}