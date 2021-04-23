using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour, IPiece
{
    public GameObject obj {get => gameObject; set{}}
    public Team team {get => _team; set{_team = value;}}
    private Team _team;
    public Piece piece {get => _piece; set{_piece = value;}}
    private Piece _piece;
    public Index location {get => _location; set{_location = value;}}
    private Index _location;
    private Index startLoc;
    public bool captured {get => _captured; set{_captured = value;}}
    private bool _captured = false;
    public ushort value {get => 1; set{}}
    public bool passantable = false;
    public int turnsPassed = 0;
    public int goal => team == Team.White ? 18 - (location.row % 2) : location.row % 2;

    private Board board;
    bool isSingleplayer;
    public void Init(Team team, Piece piece, Index startingLocation)
    {
        this.team = team;
        this.piece = piece;
        this.location = startingLocation;
        startLoc = startingLocation;
        isSingleplayer = GameObject.FindObjectOfType<Multiplayer>() == null;
    }

    public List<(Hex, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState, bool includeBlocking = false)
    {
        List<(Hex, MoveType)> possible = new List<(Hex, MoveType)>();
        int pawnOffset = team == Team.White ? 2 : -2;
        int attackOffset = location.row % 2 == 0 ? 1 : -1;

        // Check takes
        Hex take1 = board.GetHexIfInBounds(location.row + (pawnOffset / 2), location.col + attackOffset);
        if(CanTake(take1, boardState, includeBlocking))
            possible.Add((take1, MoveType.Attack));
        
        Hex take2 = board.GetHexIfInBounds(location.row + (pawnOffset / 2), location.col);
        if(CanTake(take2, boardState, includeBlocking))
            possible.Add((take2, MoveType.Attack));
        
        // Check en passant
        Hex passant1 = board.GetHexIfInBounds(location.row - (pawnOffset / 2), location.col + attackOffset);
        if(CanPassant(passant1, boardState))
            possible.Add((take1, MoveType.EnPassant));
        
        Hex passant2 = board.GetHexIfInBounds(location.row - (pawnOffset / 2), location.col);
        if(CanPassant(passant2, boardState))
            possible.Add((take2, MoveType.EnPassant));

        // One forward
        Hex normHex = board.GetHexIfInBounds(location.row + pawnOffset, location.col);
        if(CanMove(normHex, boardState, ref possible))
            return possible; 
        
        // Two forward on 1st move
        if(location == startLoc)
        {
            Hex boostedHex = board.GetHexIfInBounds(location.row + (pawnOffset * 2), location.col);
            if(CanMove(boostedHex, boardState, ref possible))
                return possible; 
        }
        return possible;
    }

    private bool CanMove(Hex hex, BoardState boardState, ref List<(Hex, MoveType)> possible)
    {
        if(hex == null)
            return false;
        
        if(boardState.allPiecePositions.ContainsKey(hex.index))
            return true;
        
        possible.Add((hex, MoveType.Move));
        return false;
    }

    private bool CanTake(Hex hex, BoardState boardState, bool includeBlocking = false)
    {
        if(hex == null)
            return false;

        if(boardState.allPiecePositions.ContainsKey(hex.index))
        {
            (Team occupyingTeam, Piece occupyingType) = boardState.allPiecePositions[hex.index];
            if(occupyingTeam != team || includeBlocking)
                return true;
        }
        return false;
    }

    private bool CanPassant(Hex passantToHex, BoardState boardState)
    {
        if(passantToHex == null)
            return false;
        
        if(boardState.allPiecePositions.ContainsKey(passantToHex.index))
        {
            (Team occupyingTeam, Piece occupyingType) = boardState.allPiecePositions[passantToHex.index];
            if(occupyingTeam == team)
                return false;

            if(passantToHex.board.activePieces.ContainsKey((occupyingTeam, occupyingType)))
            {
                IPiece piece = passantToHex.board.activePieces[(occupyingTeam, occupyingType)];
                if(piece is Pawn otherPawn && otherPawn.passantable)
                    return true;
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
        if(hex.index == boostedLoc)
        {
            board = hex.board;
            board.newTurn += TurnPassed;
            passantable = true;
        }

        transform.position = hex.transform.position + Vector3.up;
        location = hex.index;
        
        // If the pawn reaches the other side of the board, it can Promote
        if(location.row == goal)
            hex.board.QueryPromote(this);
    }

    private void TurnPassed(BoardState newState)
    {
        // A pawn may only be EnPassanted on the enemies turn immediately after it used it's boosted move
        // So we track when the turn passes, on the 2nd pass (enemy returning control to us), clear the passant flag
        int count = isSingleplayer ? 1 : 2;
        if(turnsPassed >= count)
        {
            passantable = false;
            turnsPassed = 0;
            board.newTurn -= TurnPassed;
            board = null;
        }
        else
            turnsPassed++;
    }
}