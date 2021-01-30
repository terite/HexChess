using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

public class HexSpawner : SerializedMonoBehaviour
{
    [SerializeField] private GameObject hexPrefab;
    [SerializeField] public HexGrid hexGrid;
    [OdinSerialize] public List<List<Hex>> hexes = new List<List<Hex>>();
    
    private void MaybeNewHex()
    {
        Hex[] selectedHexes = Selection.GetFiltered<Hex>(SelectionMode.Unfiltered);
        Debug.Log(selectedHexes.Length);
    }

    [Button("Spawn Hexes")]
    private void SpawnHexes()
    {
        if(hexes.Count > 0)
            ClearHexes();
        
        for(int row = 0; row < hexGrid.rows; row++) 
        {
            hexes.Add(new List<Hex>());
            for(int col = 0; col < hexGrid.cols; col++)
            {
                if(col == hexGrid.cols - 1 && row % 2 == 0)
                    continue;

                GameObject newGo = Instantiate(
                    original: hexPrefab,
                    position: new Vector3(
                        x: hexGrid.radius * 3 * col + Get_X_Offset(row),
                        y: UnityEngine.Random.Range(hexGrid.minHeight, hexGrid.maxHeight),
                        z: row * hexGrid.Apothem
                    ),
                    rotation: Quaternion.identity,
                    parent: transform
                );

                Hex newHex = newGo.GetComponent<Hex>();

                newHex.transform.localScale = new Vector3(
                    x: newHex.transform.localScale.x * hexGrid.radius,
                    y: newHex.transform.localScale.y * hexGrid.height,
                    z: newHex.transform.localScale.z * hexGrid.radius
                );

                newHex.AssignIndex(new Index(row, col));

                hexes[row].Add(newHex);
                float randShade = UnityEngine.Random.Range(0f,1f);
                // GetColor(row, col);
                newHex.SetColor(new Color(randShade, randShade, randShade));
            }
        }
    }

    // public Color GetColor(int row, int col)
    // {
        
    // }

    private float Get_X_Offset(int row) => row % 2 == 0 ? hexGrid.radius * 1.5f : 0f;

    [Button("Clear Hexes")]
    private void ClearHexes()
    {
        for(int row = 0; row < hexes.Count; row++)
        {
            for(int col = 0; col < hexes[row].Count; col++)
            {
#if UNITY_EDITOR
                DestroyImmediate(hexes[row][col].gameObject);
#elif !UNITY_EDITOR
                Destroy(hexes[row][col].gameObject);
#endif                
            }
        }
        hexes = new List<List<Hex>>();
    }

    public Hex GetNeighborAt(Index source, HexNeighborDirection direction)
    {
        (int row, int col) offsets = GetOffsetInDirection(source.row % 2 == 0, direction);
        return GetHexIfInBounds(source.row + offsets.row, source.col + offsets.col);
    }

    private Hex GetHexIfInBounds(int row, int col)
    {
        if(col == hexGrid.cols - 1 && row % 2 == 0)
            return null;
        return hexGrid.IsInBounds(row, col) ? hexes[row][col] : null;
    }

    private (int row, int col) GetOffsetInDirection(bool isEven, HexNeighborDirection direction)
    {
        switch(direction)
        {
            case HexNeighborDirection.Up:
                return (2, 0);
            case HexNeighborDirection.UpRight:
                return isEven ? (1, 1) : (1, 0);
            case HexNeighborDirection.DownRight:
                return isEven ? (-1, 1) : (-1, 0);
            case HexNeighborDirection.Down:
                return (-2, 0);
            case HexNeighborDirection.DownLeft:
                return isEven ? (-1, 0) : (-1, -1);
            case HexNeighborDirection.UpLeft:
                return isEven ? (1, 0) : (1, -1);
        }
        return (0, 0);
    }

}

public enum HexNeighborDirection{Up, UpRight, DownRight, Down, DownLeft, UpLeft};