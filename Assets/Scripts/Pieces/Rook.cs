using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : MonoBehaviour, IPiece
{
    public GameObject obj {get => gameObject; set{}}
    public Team team { get{ return _team; } set{ _team = value; } }
    private Team _team;
    public PieceType type { get{ return _type; } set{ _type = value; } }
    private PieceType _type;
    public Index location { get{ return _location; } set{ _location = value; } }
    private Index _location;

    public void Init(Team team, PieceType type, Index startingLocation)
    {
        this.team = team;
        this.type = type;
        this.location = startingLocation;
    }

    public List<Hex> GetAllPossibleMoves(HexSpawner boardSpawner, BoardState boardState)
    {
        List<Hex> possible = new List<Hex>();

        // Up
        for(int row = location.row + 2; row <= boardSpawner.hexGrid.rows; row += 2)
            if(!CanMove(boardSpawner, boardState, row, location.col, ref possible))
                break;
        // Down
        for(int row = location.row - 2; row >= 0; row -= 2)
            if(!CanMove(boardSpawner, boardState, row, location.col, ref possible))
                break;
        // Left
        for(int col = location.col - 1; col >= 0; col--)
            if(!CanMove(boardSpawner, boardState, location.row, col, ref possible))
                break;
        // Right
        for(int col = location.col + 1; col <= boardSpawner.hexGrid.cols - 2 + location.row % 2; col++)
            if(!CanMove(boardSpawner, boardState, location.row, col, ref possible))
                break;
            
        // Check defend
        foreach(HexNeighborDirection dir in Enum.GetValues(typeof(HexNeighborDirection)))
        {
            Hex hex = boardSpawner.GetNeighborAt(location, dir);
            if(hex == null)
                continue;
            
            if(boardState.biDirPiecePositions.ContainsKey(hex.hexIndex))
            {
                (Team occuypingTeam, PieceType occupyingType) = boardState.biDirPiecePositions[hex.hexIndex];
                if(occuypingTeam == team)
                    possible.Add(hex);
            }
        }

        return possible;
    }

    private bool CanMove(HexSpawner board, BoardState boardState, int row, int col, ref List<Hex> possible)
    {
        Hex hex = board.GetHexIfInBounds(row, col);
        if(hex == null)
            return false;
            
        if(boardState.biDirPiecePositions.ContainsKey(hex.hexIndex))
        {
            (Team occupyingTeam, PieceType occupyingType) = boardState.biDirPiecePositions[hex.hexIndex];
            if(occupyingTeam != team)
                possible.Add(hex);
            return false;
        }
        possible.Add(hex);
        return true;
    }

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.hexIndex;
    }
}