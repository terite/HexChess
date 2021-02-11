using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : MonoBehaviour, IPiece
{
    public GameObject obj {get => gameObject; set{}}
    public Team team { get{ return _team; } set{ _team = value; } }
    private Team _team;
    public PieceType type { get{ return _type; } set{ _type = value; } }
    private PieceType _type;
    public Index location { get{ return _location; } set{ _location = value; } }
    private Index _location;
    public bool captured { get{ return _captured; } set{ _captured = value; } }
    private bool _captured = false;
    
    public void Init(Team team, PieceType type, Index startingLocation)
    {
        this.team = team;
        this.type = type;
        this.location = startingLocation;
    }

    public List<(Hex, MoveType)> GetAllPossibleMoves(HexSpawner boardSpawner, BoardState boardState)
    {
        List<(Hex, MoveType)> possibleMoves = new List<(Hex, MoveType)>();
        foreach(HexNeighborDirection dir in Enum.GetValues(typeof(HexNeighborDirection)))
        {
            Hex hex = boardSpawner.GetNeighborAt(location, dir);
            if(hex == null)
                continue;

            if(boardState.biDirPiecePositions.ContainsKey(hex.hexIndex))
            {
                (Team occuypingTeam, PieceType occupyingType) = boardState.biDirPiecePositions[hex.hexIndex];
                if(occuypingTeam == team)
                    continue;
                else
                {
                    possibleMoves.Add((hex, MoveType.Attack));
                    continue;
                }
            }
            possibleMoves.Add((hex, MoveType.Move));
        }
        return possibleMoves;
    }

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.hexIndex;
    }
}
