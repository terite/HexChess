using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : MonoBehaviour, IPiece
{
    public GameObject obj {get => gameObject; set{}}
    public Team team { get{ return _team; } set{ _team = value; } }
    private Team _team;
    public Piece piece { get{ return _piece; } set{ _piece = value; } }
    private Piece _piece;
    public Index location { get{ return _location; } set{ _location = value; } }
    private Index _location;
    public bool captured { get{ return _captured; } set{ _captured = value; } }
    private bool _captured = false;
    public ushort value {get => 5; set{}}
    public List<Piece> defendableTypes = new List<Piece>();
    private Vector3? targetPos = null;
    public float speed = 15f;

    public void Init(Team team, Piece piece, Index startingLocation)
    {
        this.team = team;
        this.piece = piece;
        this.location = startingLocation;
    }

    public IEnumerable<(Index, MoveType)> GetAllPossibleMoves(BoardState boardState, bool includeBlocking = false)
    {
        List<(Index, MoveType)> possible = new List<(Index, MoveType)>();

        // Up
        for(int row = location.row + 2; row <= HexGrid.rows; row += 2)
            if(!CanMove(boardState, row, location.col, possible, includeBlocking))
                break;
        // Down
        for(int row = location.row - 2; row >= 0; row -= 2)
            if(!CanMove(boardState, row, location.col, possible, includeBlocking))
                break;
        // Left
        for(int col = location.col - 1; col >= 0; col--)
            if(!CanMove(boardState, location.row, col, possible, includeBlocking))
                break;
        // Right
        for(int col = location.col + 1; col <= HexGrid.cols - 2 + location.row % 2; col++)
            if(!CanMove(boardState, location.row, col, possible, includeBlocking))
                break;
            
        // Check defend
        foreach(HexNeighborDirection dir in EnumArray<HexNeighborDirection>.Values)
        {
            Index? maybeIndex = HexGrid.GetNeighborAt(location, dir);
            if (maybeIndex == null)
                continue;
            Index index = maybeIndex.Value;
            
            if(boardState.allPiecePositions.ContainsKey(index))
            {
                (Team occuypingTeam, Piece occupyingType) = boardState.allPiecePositions[index];
                if(occuypingTeam == team && defendableTypes.Contains(occupyingType))
                    possible.Add((index, MoveType.Defend));
            }
        }

        return possible;
    }

    private bool CanMove(BoardState boardState, int row, int col, List<(Index, MoveType)> possible, bool includeBlocking = false)
    {
        if (!HexGrid.GetValidIndex(row, col, out Index index))
            return false;
            
        if(boardState.allPiecePositions.ContainsKey(index))
        {
            (Team occupyingTeam, Piece occupyingType) = boardState.allPiecePositions[index];
            if(occupyingTeam != team || includeBlocking)
                possible.Add((index, MoveType.Attack));
            return false;
        }
        possible.Add((index, MoveType.Move));
        return true;
    }

    public void MoveTo(Hex hex, Action action = null)
    {
        targetPos = hex.transform.position + Vector3.up;
        location = hex.index;
    }

    private void Update() => MoveOverTime();

    private void MoveOverTime()
    {
        if(!targetPos.HasValue)
            return;

        transform.position = Vector3.Lerp(transform.position, targetPos.Value, Time.deltaTime * speed);
        if((transform.position - targetPos.Value).magnitude < 0.03f)
        {
            transform.position = targetPos.Value;
            targetPos = null;
        }
    }

    public void CancelMove()
    {
        targetPos = null;
    }

    public string GetPieceString() => "Rook";
}