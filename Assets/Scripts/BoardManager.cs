using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoardManager : SerializedMonoBehaviour
{
    public List<BoardState> turnHistory = new List<BoardState>();
    public Dictionary<(Team, PieceType), GameObject> piecePrefabs = new Dictionary<(Team, PieceType), GameObject>();
    public Dictionary<(Team, PieceType), Piece> activePieces = new Dictionary<(Team, PieceType), Piece>();
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
                Piece piece = activePieces[pieceAtLocation.Key];
                piece.transform.position = piecePosition;
                piece.location = index;
                continue;
            }

            // Spawn a new piece at the proper location
            Piece newPiece = Instantiate(piecePrefabs[pieceAtLocation.Key], piecePosition, Quaternion.identity).GetComponent<Piece>();
            (Team team, PieceType type) = pieceAtLocation.Key;
            newPiece.team = team;
            newPiece.type = type;
            newPiece.location = index;
            activePieces.Add(
                pieceAtLocation.Key, 
                newPiece
            );
        }
    }

    public Team GetCurrentTurn() => turnHistory[turnHistory.Count - 1].currentMove;

    public List<Hex> GetMovesOnCurrentBoardState(Piece piece)
    {
        List<Hex> hexList = new List<Hex>();

        switch(piece.type)
        {
            case PieceType.King:
                foreach(HexNeighborDirection dir in Enum.GetValues(typeof(HexNeighborDirection)))
                {
                    Hex hex = boardSpawner.GetNeighborAt(piece.location, dir);
                    if(hex == null)
                        continue;
                    if(CheckCanMoveToIndex(piece, hex.hexIndex))
                        hexList.Add(hex);
                }
                break;
            case PieceType.Queen:
                break;
            case PieceType.KingsKnight:
            case PieceType.QueensKnight:
                break;
            case PieceType.KingsRook:
            case PieceType.QueensRook:
                break;
            case PieceType.KingsBishop:
            case PieceType.QueensBishop:
                break;
            case PieceType.BlackSquire:
            case PieceType.GraySquire:
            case PieceType.WhiteSquire:
                Index loc = piece.location;
                List<Hex> possible = new List<Hex>();
                int squireOffset = piece.location.row % 2 == 0 ? 1 : -1;
                possible.Add(boardSpawner.GetHexIfInBounds(loc.row + 3, loc.col + squireOffset));
                possible.Add(boardSpawner.GetHexIfInBounds(loc.row - 3, loc.col + squireOffset));
                possible.Add(boardSpawner.GetHexIfInBounds(loc.row + 3, loc.col));
                possible.Add(boardSpawner.GetHexIfInBounds(loc.row - 3, loc.col));
                possible.Add(boardSpawner.GetHexIfInBounds(loc.row, loc.col + 1));
                possible.Add(boardSpawner.GetHexIfInBounds(loc.row, loc.col - 1));
                
                foreach(Hex hex in possible)
                {
                    if(hex != null && CheckCanMoveToIndex(piece, hex.hexIndex))
                        hexList.Add(hex);
                }
                break;
            case PieceType.Pawn1:
            case PieceType.Pawn2:
            case PieceType.Pawn3:
            case PieceType.Pawn4:
            case PieceType.Pawn5:
            case PieceType.Pawn6:
            case PieceType.Pawn7:
            case PieceType.Pawn8:
                int pawnOffset = piece.team == Team.White ? 2 : -2;
                Hex normHex = boardSpawner.GetHexIfInBounds(piece.location.row + pawnOffset, piece.location.col);
                if(normHex != null && CheckCanMoveToIndex(piece, normHex.hexIndex))
                    hexList.Add(normHex);
                
                // If a pawn has never moved, it can move 2 places
                Index startLoc = turnHistory[0].bidPiecePositions[(piece.team, piece.type)];
                if(piece.location == startLoc)
                {
                    Hex boostedHex = boardSpawner.GetHexIfInBounds(piece.location.row + (pawnOffset * 2), piece.location.col);
                    if(boostedHex != null && CheckCanMoveToIndex(piece, boostedHex.hexIndex))
                        hexList.Add(boostedHex);
                }
                break;
        }

        return hexList;
    }

    public bool CheckCanMoveToIndex(Piece piece, Index index)
    {
        if(turnHistory[turnHistory.Count - 1].bidPiecePositions.ContainsKey(index))
        {
            (Team occuypingTeam, PieceType occupyingType) = turnHistory[turnHistory.Count - 1].bidPiecePositions[index];
            if(occuypingTeam != piece.team)
                return true;
            return false;
        }
        return true;
    }
}