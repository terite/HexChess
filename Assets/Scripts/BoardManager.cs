using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoardManager : SerializedMonoBehaviour
{
    public List<BoardState> turnHistory = new List<BoardState>();
    public Dictionary<(Team, PieceType), GameObject> piecePrefabs = new Dictionary<(Team, PieceType), GameObject>();
    public Dictionary<(Team, PieceType), IPiece> activePieces = new Dictionary<(Team, PieceType), IPiece>();
    [SerializeField] private HexSpawner boardSpawner;

    private void Awake() => SetBoardState(turnHistory[0]);

    private void SetBoardState(BoardState newState)
    {
        foreach(KeyValuePair<(Team, PieceType), Index> pieceAtLocation in newState.bidPiecePositions)
        {
            Index index = pieceAtLocation.Value;
            Vector3 piecePosition = boardSpawner.hexes[index.row][index.col].transform.position + Vector3.up;

            // If the piece already exists, move it
            if(activePieces.ContainsKey(pieceAtLocation.Key))
            {
                IPiece piece = activePieces[pieceAtLocation.Key];
                piece.MoveTo(boardSpawner.hexes[index.row][index.col]);
                continue;
            }

            // Spawn a new piece at the proper location
            IPiece newPiece = Instantiate(piecePrefabs[pieceAtLocation.Key], piecePosition, Quaternion.identity).GetComponent<IPiece>();
            (Team team, PieceType type) = pieceAtLocation.Key;
            newPiece.Init(team, type, index);
            activePieces.Add(
                pieceAtLocation.Key, 
                newPiece
            );
        }
    }

    public Team GetCurrentTurn() => turnHistory[turnHistory.Count - 1].currentMove;
    public BoardState GetCurrentBoardState() => turnHistory[turnHistory.Count - 1];
}