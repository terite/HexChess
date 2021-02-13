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
    public List<Color> moveTypeHighlightColors = new List<Color>();

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
                if (selectedPiece == clickedPiece)
                    return;

                // Rooks can defend (swap positions with a near by ally)
                if (pieceMoves.Contains((board.GetHexIfInBounds(clickedPiece.location), MoveType.Defend)))
                {
                    Defend(clickedPiece);
                    return;
                }

                // Deselect any existing selection
                if (selectedPiece != null)
                    DeselectPiece(selectedPiece.location);

                // Select new piece and highlight all of the places it can move to on the current board state
                selectedPiece = clickedPiece;                
                pieceMoves = board.GetAllValidMovesForPiece(selectedPiece, currentBoardState);

                // Highlight each possible move the correct color
                foreach ((Hex hex, MoveType moveType) in pieceMoves)
                {
                    hex.ToggleSelect();
                    hex.SetOutlineColor(moveTypeHighlightColors[(int)moveType]);
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
                    Defend(board.activePieces[currentBoardState.biDirPiecePositions[hitHex.index]]);
                else if(pieceMoves.Contains((hitHex, MoveType.EnPassant)))
                {
                    Index startIndex = selectedPiece.location;
                    int teamOffset = currentBoardState.currentMove == Team.White ? -2 : 2;
                    Index enemyLoc = new Index(hitHex.index.row + teamOffset, hitHex.index.col);
                    (Team enemyTeam, Piece enemyType) = currentBoardState.biDirPiecePositions[enemyLoc];
                    BoardState newState = board.EnPassant((Pawn)selectedPiece, enemyTeam, enemyType, hitHex, currentBoardState);
                    board.AdvanceTurn(newState);
                    DeselectPiece(startIndex);
                }
            }
        }
    }

    private void Defend(IPiece pieceToDefend)
    {
        Index startLoc = selectedPiece.location;
        // Hex startHex = board.GetHexIfInBounds(startLoc.row, startLoc.col);
        BoardState newState = board.Swap(selectedPiece, pieceToDefend, board.GetCurrentBoardState());
        board.AdvanceTurn(newState);
        DeselectPiece(startLoc);
    }

    private void MoveOrAttack(Hex hitHex)
    {
        Index pieceStartLoc = selectedPiece.location;
        BoardState newState = board.SubmitMove(selectedPiece, hitHex, board.GetCurrentBoardState());
        board.AdvanceTurn(newState);
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

        foreach((Hex hex, MoveType moveType) in pieceMoves)
            hex.ToggleSelect();
        pieceMoves = Enumerable.Empty<(Hex, MoveType)>();

        board.GetHexIfInBounds(fromIndex).ToggleSelect();
        
        selectedPiece = null;
    }
}