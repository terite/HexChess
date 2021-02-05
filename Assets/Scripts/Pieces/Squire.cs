using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squire : MonoBehaviour, IPiece
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
        int squireOffset = location.row % 2 == 0 ? 1 : -1;
        possible.Add(boardSpawner.GetHexIfInBounds(location.row + 3, location.col + squireOffset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row - 3, location.col + squireOffset));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row + 3, location.col));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row - 3, location.col));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row, location.col + 1));
        possible.Add(boardSpawner.GetHexIfInBounds(location.row, location.col - 1));

        for(int i = possible.Count - 1; i >= 0; i--)
        {
            if(possible[i] == null)
            {
                possible.RemoveAt(i);
                continue;
            }
            if(boardState.biDirPiecePositions.ContainsKey(possible[i].hexIndex))
            {
                (Team occupyingTeam, PieceType occupyingType) = boardState.biDirPiecePositions[possible[i].hexIndex];
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