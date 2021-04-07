using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Linq;

public class SelectPiece : MonoBehaviour
{
    Mouse mouse => Mouse.current;
    Multiplayer multiplayer;
    PreviewMovesToggle singlePlayerMovesToggle;
    Camera cam;
    [SerializeField] private Board board;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Color selectedPieceColor;
    public IPiece selectedPiece {get; private set;}
    [SerializeField] private OnMouse onMouse;
    private bool hoverExitedInitialHex = false;
    IEnumerable<(Hex, MoveType)> pieceMoves = Enumerable.Empty<(Hex, MoveType)>();
    IEnumerable<(Hex, MoveType)> previewMoves = Enumerable.Empty<(Hex, MoveType)>();
    public List<Color> moveTypeHighlightColors = new List<Color>();
    public Color previewColor;
    public Color greenColor;
    public Color redColor;

    MeshRenderer lastChangedRenderer;
    IPiece lastChangedPiece;
    public Color whiteColor;
    public Color blackColor;

    private void Awake() 
    {
        cam = Camera.main;
        multiplayer = GameObject.FindObjectOfType<Multiplayer>();
        if(multiplayer == null)
            singlePlayerMovesToggle = GameObject.FindObjectOfType<PreviewMovesToggle>();
    }
    private void Update()
    {
        MovePreviewsOnHover();
        ColorizeBasedOnMove();
    }

    private void ColorizeBasedOnMove()
    {
        if(selectedPiece != null && onMouse != null)
        {
            if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
            {
                IPiece hitPiece = hit.collider.GetComponent<IPiece>();
                if(hitPiece != null)
                {
                    Hex hex = board.GetHexIfInBounds(hitPiece.location);
                    if(hex != null)
                    {
                        SetOnMouseColor(hex);

                        #region Change color of other piece under mouse
                        MeshRenderer hitRenderer = hitPiece.obj.GetComponentInChildren<MeshRenderer>();
                        if(hitPiece != selectedPiece && hitRenderer != null)
                        {
                            if(hitRenderer != lastChangedRenderer)
                            {
                                if(lastChangedRenderer != null)
                                    ResetLastChangedRenderer();

                                Color toSet = hitPiece.team == Team.White ? whiteColor : blackColor;
                                if(pieceMoves.Contains((hex, MoveType.Attack)) || pieceMoves.Contains((hex, MoveType.EnPassant)))
                                    toSet = redColor;
                                else if(pieceMoves.Contains((hex, MoveType.Defend)) || pieceMoves.Contains((hex, MoveType.Move)))
                                    toSet = greenColor;

                                hitRenderer.material.SetColor("_BaseColor", toSet);
                                lastChangedPiece = hitPiece;
                                lastChangedRenderer = hitRenderer;
                            }
                        }
                        else if(lastChangedRenderer != null)
                            ResetLastChangedRenderer();
                        #endregion
                    }
                }
                else
                {
                    Hex hitHex = hit.collider.GetComponent<Hex>();
                    if(hitHex != null)
                    {
                        SetOnMouseColor(hitHex);

                        #region Change color of other piece under mouse
                        BoardState currentBoardState = board.GetCurrentBoardState();
                        if(currentBoardState.allPiecePositions.ContainsKey(hitHex.index))
                        {
                            (Team occupyingTeam, Piece occupyingPiece) = currentBoardState.allPiecePositions[hitHex.index];
                            if(board.activePieces.ContainsKey((occupyingTeam, occupyingPiece)))
                            {
                                IPiece piece = board.activePieces[(occupyingTeam, occupyingPiece)];
                                if(piece != selectedPiece)
                                {
                                    MeshRenderer hitPieceRenderer = piece.obj.GetComponentInChildren<MeshRenderer>();
                                    if(hitPieceRenderer != lastChangedRenderer)
                                    {
                                        if(lastChangedRenderer != null)
                                            ResetLastChangedRenderer();

                                        Color toSet = piece.team == Team.White ? whiteColor : blackColor;
                                        if(pieceMoves.Contains((hitHex, MoveType.Attack)) || pieceMoves.Contains((hitHex, MoveType.EnPassant)))
                                            toSet = redColor;
                                        else if(pieceMoves.Contains((hitHex, MoveType.Defend)) || pieceMoves.Contains((hitHex, MoveType.Move)))
                                            toSet = greenColor;

                                        hitPieceRenderer.material.SetColor("_BaseColor", toSet);
                                        lastChangedPiece = piece;
                                        lastChangedRenderer = hitPieceRenderer;
                                    }
                                }
                                else if(lastChangedRenderer != null)
                                    ResetLastChangedRenderer();
                            }
                            else if(lastChangedRenderer != null)
                                ResetLastChangedRenderer();
                        }
                        else if(lastChangedRenderer != null)
                            ResetLastChangedRenderer();
                        #endregion
                    }
                    else
                        SetOnMouseColor();
                }
            }
            else
                SetOnMouseColor();
        }
    }

    private void SetOnMouseColor(Hex hex = null)
    {
        if(hex != null)
        {
            if(pieceMoves.Contains((hex, MoveType.Attack))
                || pieceMoves.Contains((hex, MoveType.Move))
                || pieceMoves.Contains((hex, MoveType.Defend))
                || pieceMoves.Contains((hex, MoveType.EnPassant))
            )
            {
                onMouse.SetColor(greenColor);
                hoverExitedInitialHex = true;
            }
            else if(selectedPiece.location == hex.index && !hoverExitedInitialHex)
                onMouse.SetColor(selectedPiece.team == Team.White ? whiteColor : blackColor);
            else
            {
                onMouse.SetColor(redColor);
                hoverExitedInitialHex = true;
            }
        }
        else
        {
            onMouse.SetColor(redColor);
            hoverExitedInitialHex = true;
        }
    }

    private void ResetLastChangedRenderer()
    {
        lastChangedRenderer?.material.SetColor("_BaseColor", lastChangedPiece?.team == Team.White ? whiteColor : blackColor);
        lastChangedPiece = null;
        lastChangedRenderer = null;
    }

    private void MovePreviewsOnHover()
    {
        if(multiplayer != null && !multiplayer.gameParams.showMovePreviews)
            return;
        else if(singlePlayerMovesToggle != null && !singlePlayerMovesToggle.toggle.isOn) 
            return;

        if(selectedPiece == null)
        {
            if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
            {
                BoardState currentBoardState = board.GetCurrentBoardState();
                IPiece hoveredPiece = hit.collider.GetComponent<IPiece>();
                if(hoveredPiece == null)
                {
                    Hex hoveredHex = hit.collider.GetComponent<Hex>();
                    if(hoveredHex != null)
                    {
                        if(currentBoardState.allPiecePositions.ContainsKey(hoveredHex.index))
                        {
                            // Get the piece on that hex
                            hoveredPiece = board.activePieces[currentBoardState.allPiecePositions[hoveredHex.index]];
                            if(hoveredPiece != null && !hoveredPiece.captured)
                            {
                                IEnumerable<(Hex, MoveType)> incomingPreviewMoves = board.GetAllValidMovesForPiece(
                                    hoveredPiece,
                                    currentBoardState
                                );
                                if(incomingPreviewMoves != previewMoves)
                                {
                                    DisablePreview();
                                    previewMoves = incomingPreviewMoves;
                                    previewMoves = previewMoves.Append((hoveredHex, MoveType.None));
                                    EnablePreview();
                                }
                            }
                            else if(previewMoves.Count() > 0)
                                DisablePreview();
                        }
                        else if(previewMoves.Count() > 0)
                            DisablePreview();
                    }
                    else if(previewMoves.Count() > 0)
                        DisablePreview();
                }
                else if(!hoveredPiece.captured)
                {
                    IEnumerable<(Hex, MoveType)> incomingPreviewMoves = board.GetAllValidMovesForPiece(hoveredPiece, currentBoardState);
                    if(incomingPreviewMoves != previewMoves)
                    {
                        DisablePreview();
                        previewMoves = incomingPreviewMoves;

                        Hex hoveredPieceHex = board.GetHexIfInBounds(hoveredPiece.location);
                        if(hoveredPieceHex != null)
                            previewMoves = previewMoves.Append((hoveredPieceHex, MoveType.None));

                        EnablePreview();
                    }
                }
                else if(previewMoves.Count() > 0)
                    DisablePreview();
            }
            else if(previewMoves.Count() > 0)
                DisablePreview();
        }
        else if(previewMoves.Count() > 0)
            DisablePreview();
    }

    private void EnablePreview()
    {
        foreach((Hex hex, MoveType moveType) in previewMoves)
        {
            hex.SetOutlineColor(previewColor);
            hex.ToggleSelect();
        }
    }

    private void DisablePreview()
    {
        foreach ((Hex hex, MoveType moveType) in previewMoves)
            hex.ToggleSelect();
        previewMoves = Enumerable.Empty<(Hex, MoveType)>();
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
                    onMouse.PickUp(selectedPiece.obj);
                    onMouse.SetColor(selectedPiece.team == Team.White ? whiteColor : blackColor);
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
            if(lastChangedRenderer != null)
                ResetLastChangedRenderer();

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
        onMouse.PutDown();
        hoverExitedInitialHex = false;
        
        selectedPiece = null;
    }
}