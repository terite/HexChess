using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour, IPiece
{
    public GameObject obj {get => gameObject; set{}}
    public Team team { get{ return _team; } set{ _team = value; } }
    private Team _team;
    public PieceType type { get{ return _type; } set{ _type = value; } }
    private PieceType _type;
    public Index location { get{ return _location; } set{ _location = value; } }
    private Index _location;
    private Index startLoc;

    public bool passantable = false;
    public int turnsPassed = 0;
    
    public void Init(Team team, PieceType type, Index startingLocation)
    {
        this.team = team;
        this.type = type;
        this.location = startingLocation;
        startLoc = startingLocation;
    }

    public List<(Hex, MoveType)> GetAllPossibleMoves(HexSpawner boardSpawner, BoardState boardState)
    {
        List<(Hex, MoveType)> possible = new List<(Hex, MoveType)>();
        int pawnOffset = team == Team.White ? 2 : -2;

        int attackOffset = location.row % 2 == 0 ? 1 : -1;

        // Check takes
        Hex take1 = boardSpawner.GetHexIfInBounds(location.row + (pawnOffset / 2), location.col + attackOffset);
        if(CanTake(take1, boardState))
            possible.Add((take1, MoveType.Attack));
        
        Hex take2 = boardSpawner.GetHexIfInBounds(location.row + (pawnOffset / 2), location.col);
        if(CanTake(take2, boardState))
            possible.Add((take2, MoveType.Attack));
        
        // Check en passant
        Hex passant1 = boardSpawner.GetHexIfInBounds(location.row - (pawnOffset / 2), location.col + attackOffset);
        if(CanPassant(passant1, boardState))
            possible.Add((take1, MoveType.EnPassant));
        
        Hex passant2 = boardSpawner.GetHexIfInBounds(location.row - (pawnOffset / 2), location.col);
        if(CanPassant(passant2, boardState))
            possible.Add((take2, MoveType.EnPassant));

        // One forward
        Hex normHex = boardSpawner.GetHexIfInBounds(location.row + pawnOffset, location.col);
        if(CanMove(normHex, boardState, ref possible))
            return possible; 
        
        // Two forward on 1st move
        if(location == startLoc)
        {
            Hex boostedHex = boardSpawner.GetHexIfInBounds(location.row + (pawnOffset * 2), location.col);
            if(CanMove(boostedHex, boardState, ref possible))
                return possible; 
        }
        return possible;
    }

    private bool CanMove(Hex hex, BoardState boardState, ref List<(Hex, MoveType)> possible)
    {
        if(hex == null)
            return false;
        
        if(boardState.biDirPiecePositions.ContainsKey(hex.hexIndex))
            return true;
        
        possible.Add((hex, MoveType.Move));
        return false;
    }

    private bool CanTake(Hex hex, BoardState boardState)
    {
        if(hex == null)
            return false;
        if(boardState.biDirPiecePositions.ContainsKey(hex.hexIndex))
        {
            (Team occupyingTeam, PieceType occupyingType) = boardState.biDirPiecePositions[hex.hexIndex];
            if(occupyingTeam != team)
                return true;
        }
        return false;
    }

    private bool CanPassant(Hex hex, BoardState boardState)
    {
        if(hex == null)
            return false;
        
        if(boardState.biDirPiecePositions.ContainsKey(hex.hexIndex))
        {
            (Team occupyingTeam, PieceType occupyingType) = boardState.biDirPiecePositions[hex.hexIndex];
            if(occupyingTeam == team)
                return false;
            
            BoardManager boardManager = GameObject.FindObjectOfType<BoardManager>();
            if(boardManager.activePieces.ContainsKey((occupyingTeam, occupyingType)))
            {
                IPiece piece = boardManager.activePieces[(occupyingTeam, occupyingType)];
                if(piece is Pawn)
                {
                    Pawn otherPawn = (Pawn)piece;
                    if(otherPawn.passantable)
                        return true;
                }
            }
        }
        return false;
    }

    public void MoveTo(Hex hex)
    {
        Index startLoc = location;
        int pawnOffset = team == Team.White ? 2 : -2;
        // If the pawn is moved to it's boosed location, it becomes open to an enpassant
        Index boostedLoc = new Index(location.row + (pawnOffset * 2), location.col);
        if(hex.hexIndex == boostedLoc)
        {
            BoardManager boardManager = GameObject.FindObjectOfType<BoardManager>();
            boardManager.EnPassantable(this);
            passantable = true;
        }

        transform.position = hex.transform.position + Vector3.up;
        location = hex.hexIndex;
        
        // If the pawn reaches the other side of the board, it can Promote
        int goal = team == Team.White ? 18 - (location.row % 2) : location.row % 2;
        if(location.row == goal)
        {
            // promote 
            BoardManager boardManager = GameObject.FindObjectOfType<BoardManager>();
            boardManager.QueryPromote(this);
        }
    }
}