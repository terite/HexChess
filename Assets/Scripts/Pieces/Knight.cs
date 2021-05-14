using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : MonoBehaviour, IPiece
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
    public ushort value {get => 4; set{}}
    private Vector3? targetPos = null;
    public float speed = 15f;
    
    public void Init(Team team, Piece piece, Index startingLocation)
    {
        this.team = team;
        this.piece = piece;
        this.location = startingLocation;
    }

    public List<(Hex, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState, bool includeBlocking = false)
    {
        List<(Hex, MoveType)> possible = new List<(Hex, MoveType)>();
        int offset = location.row % 2 == 0 ? 1 : -1;

        possible.Add((board.GetHexIfInBounds(location.row + 5, location.col), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 5, location.col + offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 4, location.col + offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 4, location.col - offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 1, location.col + (2 * offset)), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 1, location.col - offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 1, location.col - offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 1, location.col + (2 * offset)), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 4, location.col - offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 4, location.col + offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 5, location.col + offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 5, location.col), MoveType.Move));

        for(int i = possible.Count - 1; i >= 0; i--)
        {
            (Hex possibleHex, MoveType moveType) = possible[i];
            if(possibleHex == null)
            {
                possible.RemoveAt(i);
                continue;
            }
            
            if(boardState.allPiecePositions.ContainsKey(possibleHex.index))
            {
                (Team occupyingTeam, Piece occupyingType) = boardState.allPiecePositions[possibleHex.index];
                if(occupyingTeam == team && !includeBlocking)
                    possible.RemoveAt(i);
                else
                    possible[i] = (possibleHex, MoveType.Attack);
            }
        }

        return possible;
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

    public string GetPieceString() => "Knight";
}