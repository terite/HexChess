using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoardManager : SerializedMonoBehaviour
{
    public List<BoardState> turnHistory = new List<BoardState>();
    public Dictionary<(Team, PieceType), GameObject> piecePrefabs = new Dictionary<(Team, PieceType), GameObject>();
    public Dictionary<(Team, PieceType), GameObject> activePieces = new Dictionary<(Team, PieceType), GameObject>();
    [SerializeField] private HexSpawner boardSpawner;

    private void Awake() => SetBoardState(turnHistory[0]);

    private void SetBoardState(BoardState newState)
    {
        foreach(KeyValuePair<(Team, PieceType), (int, int)> pieceAtLocation in newState.piecePositions)
        {
            (int row, int col) = pieceAtLocation.Value;
            Vector3 hexLoc = boardSpawner.hexes[row][col].transform.position;

            // If the piece already exists, move it
            if(activePieces.ContainsKey(pieceAtLocation.Key))
            {
                activePieces[pieceAtLocation.Key].transform.position = hexLoc + Vector3.up;
                continue;
            }

            // Spawn a new piece at the proper location
            activePieces.Add(
                pieceAtLocation.Key, 
                Instantiate(piecePrefabs[pieceAtLocation.Key], hexLoc + Vector3.up, Quaternion.identity)
            );
        }
    }
}