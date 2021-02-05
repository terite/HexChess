using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : MonoBehaviour, IPiece
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
        List<Hex> possible = new List<Hex>();
        int offset = location.row % 2 == 0 ? 1 : -1;

        possible.Add(boardSpawner.GetHexIfInBounds(location.row + 5, location.col));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row + 5, location.col + offset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row + 4, location.col + offset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row + 4, location.col - offset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row + 1, location.col + (2 * offset)));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row + 1, location.col - offset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row - 1, location.col - offset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row - 1, location.col + (2 * offset)));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row - 4, location.col - offset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row - 4, location.col + offset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row - 5, location.col + offset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row - 5, location.col));

        for(int i = possible.Count - 1; i >= 0; i--)
        {
            if(possible[i] == null)
            {
                possible.RemoveAt(i);
                continue;
            }
            
            if(boardState.bidPiecePositions.ContainsKey(possible[i].hexIndex))
            {
                (Team occupyingTeam, PieceType occupyingType) = boardState.bidPiecePositions[possible[i].hexIndex];
                if(occupyingTeam == team)
                    possible.RemoveAt(i);
            }
        }

        return possible;
    }

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.hexIndex;
    }
}
