using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Linq;

public class SelectPiece : MonoBehaviour
{
    Mouse mouse => Mouse.current;
    Camera cam;
    [SerializeField] private Board board;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Color selectedPieceColor;
    private IPiece selectedPiece;
    IEnumerable<(Hex, MoveType)> pieceMoves = Enumerable.Empty<(Hex, MoveType)>();

    private void Awake() => cam = Camera.main;
    public void LeftClick(CallbackContext context)
    {
        if(!context.performed)
            return;
        
        if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
        {
            if(hit.collider == null)
                return;
            
            BoardState currentBoardState = board.GetCurrentBoardState();
            
            // Clicked on a piece
            IPiece clickedPiece = hit.collider.GetComponent<IPiece>();
            if(clickedPiece != null && !clickedPiece.captured && clickedPiece.team == currentBoardState.currentMove)
            {
                if(selectedPiece == clickedPiece)
                    return;

                // Rooks can defend (swap positions with a near by ally)
                if(pieceMoves.Contains((board.GetHexIfInBounds(clickedPiece.location), MoveType.Defend)))
                {
                    Defend(clickedPiece);
                    return;
                }

                // Deselect any existing selection
                if (selectedPiece != null)
                    DeselectPiece(selectedPiece.location);

                // Select new piece and highlight all of the places it can move to on the current board state
                selectedPiece = clickedPiece;
                pieceMoves = clickedPiece.GetAllPossibleMoves(board, currentBoardState);
                
                // Highlight each possible move the correct color
                foreach((Hex hex, MoveType moveType) moves in pieceMoves)
                {
                    moves.hex.ToggleSelect();
                    switch(moves.moveType)
                    {
                        case MoveType.Move:
                            moves.hex.SetOutlineColor(Color.green);
                            break;
                        case MoveType.Attack:
                            moves.hex.SetOutlineColor(Color.red);
                            break;
                        case MoveType.Defend:
                            moves.hex.SetOutlineColor(Color.green);
                            break;
                        case MoveType.EnPassant:
                            moves.hex.SetOutlineColor(Color.red);
                            break;
                    }
                }
                
                Hex selectedHex = board.GetHexIfInBounds(selectedPiece.location);
                selectedHex.ToggleSelect();
                selectedHex.SetOutlineColor(selectedPieceColor);
                return;
            }
            else if(clickedPiece != null && selectedPiece != null && clickedPiece.team != selectedPiece.team)
            {
                Hex enemyHex = board.GetHexIfInBounds(clickedPiece.location);
                // Check if this attack is within our possible moves
                if(pieceMoves.Contains((enemyHex, MoveType.Attack)))
                    MoveOrAttack(enemyHex);
            }

            // Clicked on a hex
            Hex hitHex = hit.collider.GetComponent<Hex>();
            if(hitHex != null && selectedPiece != null)
            {
                if(pieceMoves.Contains((hitHex, MoveType.Attack)) || pieceMoves.Contains((hitHex, MoveType.Move)))
                    MoveOrAttack(hitHex);
                else if(pieceMoves.Contains((hitHex, MoveType.Defend)))
                    Defend(board.activePieces[currentBoardState.biDirPiecePositions[hitHex.hexIndex]]);
                else if(pieceMoves.Contains((hitHex, MoveType.EnPassant)))
                {
                    Index startIndex = selectedPiece.location;
                    int teamOffset = currentBoardState.currentMove == Team.White ? -2 : 2;
                    Index enemyLoc = new Index(hitHex.hexIndex.row + teamOffset, hitHex.hexIndex.col);
                    (Team enemyTeam, PieceType enemyType) = currentBoardState.biDirPiecePositions[enemyLoc];
                    board.EnPassant((Pawn)selectedPiece, enemyTeam, enemyType, hitHex);
                    DeselectPiece(startIndex);
                }
            }
        }
    }

    private void Defend(IPiece pieceToDefend)
    {
        Index startLoc = selectedPiece.location;
        Hex startHex = board.GetHexIfInBounds(startLoc.row, startLoc.col);
        board.Swap(selectedPiece, pieceToDefend);
        DeselectPiece(startLoc);
    }

    private void MoveOrAttack(Hex hitHex)
    {
        Index pieceStartLoc = selectedPiece.location;
        board.SubmitMove(selectedPiece, hitHex);
        DeselectPiece(pieceStartLoc);
    }

    public void RightClick(CallbackContext context)
    {
        if(!context.performed)
            return;
        
        if(selectedPiece != null)
            DeselectPiece(selectedPiece.location);
    }

    public void DeselectPiece(Index fromIndex)
    {
        if(selectedPiece == null)
            return;

        foreach((Hex hex, MoveType moveType) moves in pieceMoves)
            moves.hex.ToggleSelect();
        pieceMoves = Enumerable.Empty<(Hex, MoveType)>();

        board.GetHexIfInBounds(fromIndex.row, fromIndex.col)
            .ToggleSelect();
        
        selectedPiece = null;
    }
}
