using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : MonoBehaviour, IPiece
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
    public ushort value {get => 7; set{}}
    private Vector3? targetPos = null;
    public float speed;
    
    public void Init(Team team, Piece piece, Index startingLocation)
    {
        this.team = team;
        this.piece = piece;
        this.location = startingLocation;
    }

    public IEnumerable<(Index, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState, bool includeBlocking = false)
    {
        List<(Index, MoveType)> possibleMoves = new List<(Index, MoveType)>();
        foreach(HexNeighborDirection dir in EnumArray<HexNeighborDirection>.Values)
        {
            Hex hex = board.GetNeighborAt(location, dir);
            if(hex == null)
                continue;

            if(boardState.allPiecePositions.ContainsKey(hex.index))
            {
                (Team occuypingTeam, Piece occupyingType) = boardState.allPiecePositions[hex.index];
                
                if(includeBlocking || occuypingTeam != team)
                {
                    possibleMoves.Add((hex.index, MoveType.Attack));
                    continue;
                }
                else
                    continue;
            }
            possibleMoves.Add((hex.index, MoveType.Move));
        }
        return possibleMoves;
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

    public string GetPieceString() => "King";
}