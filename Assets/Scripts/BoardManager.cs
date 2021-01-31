using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoardManager : SerializedMonoBehaviour
{
    public Dictionary<(Team, PieceType), (int, int)> startingPositions = new Dictionary<(Team, PieceType), (int, int)>();
    public Dictionary<(Team, PieceType), GameObject> prefabs = new Dictionary<(Team, PieceType), GameObject>();
    [SerializeField] private HexSpawner boardSpawner;

    private void Awake() {
        SpawnTeams();
    }

    private void SpawnTeams()
    {
        foreach(Team team in Enum.GetValues(typeof(Team)))
        {
            foreach(PieceType type in Enum.GetValues(typeof(PieceType)))
            {
                (int row, int col) = startingPositions[(team, type)];
                Vector3 hexLoc = boardSpawner.hexes[row][col].transform.position;
                GameObject newPiece = Instantiate(prefabs[(team, type)], hexLoc + Vector3.up, Quaternion.identity);
            }
        }
    }

}