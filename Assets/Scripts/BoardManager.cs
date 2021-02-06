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

    public void SetBoardState(BoardState newState)
    {
        foreach(KeyValuePair<(Team, PieceType), Index> pieceAtLocation in newState.biDirPiecePositions)
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

    public void SubmitMove(IPiece piece, Hex targetLocation)
    {
        BoardState currentState = GetCurrentBoardState();
        if(currentState.biDirPiecePositions.Contains(targetLocation.hexIndex))
        {
            (Team occupyingTeam, PieceType occupyingType) = currentState.biDirPiecePositions[targetLocation.hexIndex];
            // take enemy pice, if needed
            if(occupyingTeam != piece.team)
            {
                IPiece occupyingPiece = activePieces[(occupyingTeam, occupyingType)];

                // Problem
                currentState.biDirPiecePositions.Remove((occupyingTeam, occupyingType));

                activePieces.Remove((occupyingTeam, occupyingType));
                Destroy(occupyingPiece.obj);
            }
        }

        // move piece
        piece.MoveTo(targetLocation);

        // update boardstate
        BidirectionalDictionary<(Team, PieceType), Index> allPositions = new BidirectionalDictionary<(Team, PieceType), Index>(currentState.biDirPiecePositions);
        allPositions.Remove((piece.team, piece.type));
        allPositions.Add((piece.team, piece.type), targetLocation.hexIndex);
        
        currentState.biDirPiecePositions = allPositions;
        currentState.currentMove = currentState.currentMove == Team.White ? Team.Black : Team.White;
        
        turnHistory.Add(currentState);
    }

    public void Swap(IPiece p1, IPiece p2)
    {
        Index p1StartLoc = p1.location;
        p1.MoveTo(boardSpawner.GetHexIfInBounds(p2.location));
        p2.MoveTo(boardSpawner.GetHexIfInBounds(p1StartLoc));

        BoardState currentState = GetCurrentBoardState();
        BidirectionalDictionary<(Team, PieceType), Index> allPositions = new BidirectionalDictionary<(Team, PieceType), Index>(currentState.biDirPiecePositions);
        allPositions.Remove((p1.team, p1.type));
        allPositions.Remove((p2.team, p2.type));
        allPositions.Add((p1.team, p1.type), p1.location);
        allPositions.Add((p2.team, p2.type), p2.location);
        
        currentState.biDirPiecePositions = allPositions;
        currentState.currentMove = currentState.currentMove == Team.White ? Team.Black : Team.White;
        
        turnHistory.Add(currentState);
    }
}