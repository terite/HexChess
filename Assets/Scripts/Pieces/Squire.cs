using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squire : MonoBehaviour, IPiece
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
    public ushort value {get => 2; set{}}
    private Vector3? targetPos = null;
    public float speed = 15f;

    public void Init(Team team, Piece piece, Index startingLocation)
    {
        this.team = team;
        this.piece = piece;
        this.location = startingLocation;
    }

    public IEnumerable<(Index, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState, bool includeBlocking = false)
    {
        int squireOffset = location.row % 2 == 0 ? 1 : -1;
        var possible = new (int row, int col)[] {
            (location.row + 3, location.col + squireOffset),
            (location.row - 3, location.col + squireOffset),
            (location.row + 3, location.col),
            (location.row - 3, location.col),
            (location.row, location.col + 1),
            (location.row, location.col - 1)
        };

        foreach ((int row, int col) in possible)
        {
            Index index = new Index(row, col);
            Hex hex = board.GetHexIfInBounds(index);
            if (hex == null)
                continue;

            if (boardState.allPiecePositions.ContainsKey(index))
            {
                (Team occupyingTeam, Piece occupyingType) = boardState.allPiecePositions[index];
                if (occupyingTeam == team && !includeBlocking)
                    continue;

                yield return (index, MoveType.Attack);
            }
            else
                yield return (index, MoveType.Move);
        }
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

    public string GetPieceString() => "Squire";
}