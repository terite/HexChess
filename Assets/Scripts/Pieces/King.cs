using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : MonoBehaviour, IPiece
{
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
        List<Hex> possibleMoves = new List<Hex>();
        foreach(HexNeighborDirection dir in Enum.GetValues(typeof(HexNeighborDirection)))
        {
            Hex hex = boardSpawner.GetNeighborAt(location, dir);
            if(hex == null)
                continue;

            if(boardState.bidPiecePositions.ContainsKey(hex.hexIndex))
            {
                (Team occuypingTeam, PieceType occupyingType) = boardState.bidPiecePositions[hex.hexIndex];
                if(occuypingTeam == team)
                    continue;
            }
            possibleMoves.Add(hex);
        }
        return possibleMoves;
    }

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.hexIndex;
    }
}
