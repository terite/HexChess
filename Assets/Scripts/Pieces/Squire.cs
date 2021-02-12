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
    public bool captured { get{ return _captured; } set{ _captured = value; } }
    private bool _captured = false;

    public void Init(Team team, PieceType type, Index startingLocation)
    {
        this.team = team;
        this.type = type;
        this.location = startingLocation;
    }

    public List<(Hex, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState)
    {
        List<(Hex, MoveType)> possible = new List<(Hex, MoveType)>();
        int squireOffset = location.row % 2 == 0 ? 1 : -1;
        possible.Add((board.GetHexIfInBounds(location.row + 3, location.col + squireOffset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 3, location.col + squireOffset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 3, location.col), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 3, location.col), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row, location.col + 1), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row, location.col - 1), MoveType.Move));

        for(int i = possible.Count - 1; i >= 0; i--)
        {
            (Hex possibleHex, MoveType moveType) = possible[i];
            if(possibleHex == null)
            {
                possible.RemoveAt(i);
                continue;
            }
            if(boardState.biDirPiecePositions.ContainsKey(possibleHex.hexIndex))
            {
                (Team occupyingTeam, PieceType occupyingType) = boardState.biDirPiecePositions[possibleHex.hexIndex];
                if(occupyingTeam == team)
                    possible.RemoveAt(i);
                else
                    possible[i] = (possibleHex, MoveType.Attack);
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