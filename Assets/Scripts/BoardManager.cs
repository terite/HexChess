using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoardManager : SerializedMonoBehaviour
{
    public BoardState startingState;
    public Dictionary<(Team, PieceType), GameObject> prefabs = new Dictionary<(Team, PieceType), GameObject>();
    [SerializeField] private HexSpawner boardSpawner;

    private void Awake() => SpawnTeams();

    private void SpawnTeams()
    {
        foreach(KeyValuePair<(Team, PieceType), (int, int)> kvp in startingState.piecePositions)
        {
            (int row, int col) = kvp.Value;
            Vector3 hexLoc = boardSpawner.hexes[row][col].transform.position;
            GameObject newPiece = Instantiate(prefabs[kvp.Key], hexLoc + Vector3.up, Quaternion.identity);
        }
    }
}