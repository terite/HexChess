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
    
    public void Init(Team team, PieceType type, Index startingLocation)
    {
        this.team = team;
        this.type = type;
        this.location = startingLocation;
        startLoc = startingLocation;
    }

    public List<Hex> GetAllPossibleMoves(HexSpawner boardSpawner, BoardState boardState)
    {
        List<Hex> possible = new List<Hex>();
        int pawnOffset = team == Team.White ? 2 : -2;

        int attackOffset = location.row % 2 == 0 ? 1 : -1;

        // Check takes
        Hex take1 = boardSpawner.GetHexIfInBounds(location.row + (pawnOffset/2), location.col + attackOffset);
        if(CanTake(take1, boardState))
            possible.Add(take1);
        
        Hex take2 = boardSpawner.GetHexIfInBounds(location.row + (pawnOffset/2), location.col);
        if(CanTake(take2, boardState))
            possible.Add(take2);
        
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

    private bool CanMove(Hex hex, BoardState boardState, ref List<Hex> possible)
    {
        if(hex == null)
            return false;
        
        if(boardState.biDirPiecePositions.ContainsKey(hex.hexIndex))
            return true;
        
        possible.Add(hex);
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

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.hexIndex;
    }
}