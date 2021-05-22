using Sirenix.OdinInspector;
using UnityEngine;
using Extensions;
using System.Collections.Generic;

public struct HexGrid
{
    public int cols;
    [ShowInInspector, ReadOnly] public int maxRow => rows - 1;
    public int rows;
    [ShowInInspector, ReadOnly] public int maxCol => cols - 1;
    public float radius;
    public float height;
    [SerializeField, MinMaxSlider(-64, 64, true)] private Vector2 hexHeightVariance;
    [ShowInInspector, ReadOnly] public float minHeight => hexHeightVariance.x;
    [ShowInInspector, ReadOnly] public float maxHeight => hexHeightVariance.y;

    public List<Color> colors;

    public float Apothem =>
       (radius.Sqr() - (radius * 0.5f).Sqr()).Sqrt();

    public bool IsInBounds(int row, int col) {
        bool cond1 = row * (row - maxRow) <= 0 && col* (col - maxCol) <= 0;
        bool cond2 = !(cols % 2 != 0 && col == cols - 1 && row % 2 == 0);
        return cond1 && cond2;
    }
    public bool IsInBounds(Index index) => IsInBounds(index.row, index.col);
}