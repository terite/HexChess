using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : MonoBehaviour, IPiece
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
        int offset = location.row % 2;

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

        // Top Left
        for(
            (int row, int col, int i) = (location.row + 1, location.col - offset, 0); 
            row <= boardSpawner.hexGrid.rows && col >= 0; 
            row++, i++
        ){
            if(!CanMove(boardSpawner, boardState, row, col, ref possible))
                break;

            if(i % 2 == offset)
                col--;
        }
        // Top Right
        for(
            (int row, int col, int i) = (location.row + 1, location.col + Mathf.Abs(1 - offset), 0);
            row <= boardSpawner.hexGrid.rows && col <= boardSpawner.hexGrid.cols;
            row++, i++
        ){
            if(!CanMove(boardSpawner, boardState, row, col, ref possible))
                break;

            if(i % 2 != offset)
                col++;
        }
        // Bottom Left
        for(
            (int row, int col, int i) = (location.row - 1, location.col - offset, 0);
            row >= 0 && col >= 0;
            row--, i++
        ){
            if(!CanMove(boardSpawner, boardState, row, col, ref possible))
                break;

            if(i % 2 == offset)
                col--;
        }
        // Bottom Right
        for(
            (int row, int col, int i) = (location.row - 1, location.col + Mathf.Abs(1 - offset), 0);
            row >= 0 && col <= boardSpawner.hexGrid.cols;
            row--, i++
        ){
            if(!CanMove(boardSpawner, boardState, row, col, ref possible))
                break;

            if(i % 2 != offset)
                col++;
        }

        return possible;
    }

    private bool CanMove(HexSpawner boardSpawner, BoardState boardState, int row, int col, ref List<Hex> possible)
    {
        Hex hex = boardSpawner.GetHexIfInBounds(row, col);
        if(hex != null)
        {
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
        return false;
    }

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.hexIndex;
    }
}
