using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Linq;

public class SelectPiece : MonoBehaviour
{
    Mouse mouse => Mouse.current;
    Multiplayer multiplayer;
    Camera cam;
    [SerializeField] private Board board;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Color selectedPieceColor;
    private IPiece selectedPiece;
    IEnumerable<(Hex, MoveType)> pieceMoves = Enumerable.Empty<(Hex, MoveType)>();
    public List<Color> moveTypeHighlightColors = new List<Color>();

    private void Awake() 
    {
        cam = Camera.main;
        multiplayer = GameObject.FindObjectOfType<Multiplayer>();
    }
    public void LeftClick(CallbackContext context)
    {
        if(context.started)
        {
            // Later allow players to queue a move, but for now, just prevent even clicking a piece when not their turn
            if(multiplayer != null && multiplayer.gameParams.localTeam != board.GetCurrentTurn())
                return;

            if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
            {
                if(hit.collider == null)
                    return;
                
                BoardState currentBoardState = board.GetCurrentBoardState();
                IPiece clickedPiece = hit.collider.GetComponent<IPiece>();
                if(clickedPiece != null && !clickedPiece.captured && clickedPiece.team == currentBoardState.currentMove)
                {
                    if(selectedPiece != null)
                        DeselectPiece(selectedPiece.location);

                    // Select new piece and highlight all of the places it can move to on the current board state
                    selectedPiece = clickedPiece;
                    pieceMoves = board.GetAllValidMovesForPiece(selectedPiece, currentBoardState);
                    // Highlight each possible move the correct color
                    foreach((Hex hex, MoveType moveType) in pieceMoves)
                    {
                        hex.SetOutlineColor(moveTypeHighlightColors[(int)moveType]);
                        hex.ToggleSelect();
                    }

                    Hex selectedHex = board.GetHexIfInBounds(selectedPiece.location);
                    selectedHex.SetOutlineColor(selectedPieceColor);
                    selectedHex.ToggleSelect();
                }
            }
        }
        else if(context.canceled)
        {
            if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
            {
                BoardState currentBoardState = board.GetCurrentBoardState();
                IPiece clickedPiece = hit.collider.GetComponent<IPiece>();

                if(clickedPiece != null && selectedPiece != null)
                {
                    // Rooks can defend (swap positions with a near by ally)
                    if(clickedPiece.team == selectedPiece.team && pieceMoves.Contains((board.GetHexIfInBounds(clickedPiece.location), MoveType.Defend)))
                    {
                        Defend(clickedPiece);
                        return;
                    }
                    else
                    {
                        Hex enemyHex = board.GetHexIfInBounds(clickedPiece.location);
                        // Check if this attack is within our possible moves
                        if(pieceMoves.Contains((enemyHex, MoveType.Attack)))
                            MoveOrAttack(enemyHex);
                    }
                }

                // Clicked on a hex
                Hex hitHex = hit.collider.GetComponent<Hex>();
                if(hitHex != null && selectedPiece != null)
                {
                    if(pieceMoves.Contains((hitHex, MoveType.Attack)) || pieceMoves.Contains((hitHex, MoveType.Move)))
                        MoveOrAttack(hitHex);
                    else if(pieceMoves.Contains((hitHex, MoveType.Defend)))
                        Defend(board.activePieces[currentBoardState.allPiecePositions[hitHex.index]]);
                    else if(pieceMoves.Contains((hitHex, MoveType.EnPassant)))
                        EnPassant(currentBoardState, hitHex);
                }
            }

            if(selectedPiece != null)
                DeselectPiece(selectedPiece.location);
        }
    }

    private void MoveOrAttack(Hex hitHex)
    {
        Index pieceStartLoc = selectedPiece.location;
        BoardState newState = board.MovePiece(selectedPiece, hitHex, board.GetCurrentBoardState());
        
        if(multiplayer != null)
        {
            // We skip sending a board here when a promotion is happening. That will be sent with the promotion after it's chosen
            if(!(selectedPiece is Pawn pawn) || pawn.goal != hitHex.index.row)
                multiplayer.SendBoard(newState);
        }

        board.AdvanceTurn(newState);
        DeselectPiece(pieceStartLoc);
    }

    private void Defend(IPiece pieceToDefend)
    {
        Index startLoc = selectedPiece.location;
        // Hex startHex = board.GetHexIfInBounds(startLoc.row, startLoc.col);
        BoardState newState = board.Swap(selectedPiece, pieceToDefend, board.GetCurrentBoardState());
        if(multiplayer != null)
            multiplayer.SendBoard(newState);
        board.AdvanceTurn(newState);
        DeselectPiece(startLoc);
    }

    private void EnPassant(BoardState currentBoardState, Hex hitHex)
    {
        int teamOffset = currentBoardState.currentMove == Team.White ? -2 : 2;
        Index enemyLoc = new Index(hitHex.index.row + teamOffset, hitHex.index.col);
        (Team enemyTeam, Piece enemyType) = currentBoardState.allPiecePositions[enemyLoc];
        BoardState newState = board.EnPassant((Pawn)selectedPiece, enemyTeam, enemyType, hitHex, currentBoardState);

        if (multiplayer != null)
            multiplayer.SendBoard(newState);

        board.AdvanceTurn(newState);
        DeselectPiece(selectedPiece.location);
    }

    private int GetGoal(Team team, int row) => team == Team.White ? 18 - (row % 2) : row % 2;

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