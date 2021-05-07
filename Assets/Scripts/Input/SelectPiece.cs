using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Linq;
using System;

public class SelectPiece : MonoBehaviour
{
    Mouse mouse => Mouse.current;
    Multiplayer multiplayer;
    PreviewMovesToggle singlePlayerMovesToggle;
    Camera cam;
    [SerializeField] private Board board;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Color selectedPieceColor;
    [SerializeField] private AudioSource audioSource;
    public AudioClip cancelNoise;
    public AudioClip pickupNoise;
    public IPiece selectedPiece {get; private set;}
    [SerializeField] private OnMouse onMouse;
    [SerializeField] private FreePlaceModeToggle freePlaceMode;
    private bool hoverExitedInitialHex = false;
    IEnumerable<(Hex, MoveType)> pieceMoves = Enumerable.Empty<(Hex, MoveType)>();
    IEnumerable<(Hex, MoveType)> previewMoves = Enumerable.Empty<(Hex, MoveType)>();
    public List<Color> moveTypeHighlightColors = new List<Color>();
    public Color greenColor;
    public Color redColor;
    public Color orangeColor;
    public Color hoverColor;

    IEnumerable<IPiece> threateningPieces = Enumerable.Empty<IPiece>();
    IEnumerable<IPiece> guardingPieces = Enumerable.Empty<IPiece>();
    MeshRenderer lastChangedRenderer;
    IPiece lastChangedPiece;
    public Color whiteColor;
    public Color blackColor;

    private Hex lastHoveredHex = null;

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
            if(Cursor.visible && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
            {
                BoardState currentBoardState = board.GetCurrentBoardState();
                IPiece hoveredPiece = hit.collider.GetComponent<IPiece>();
                if(hoveredPiece == null)
                {
                    Hex hoveredHex = hit.collider.GetComponent<Hex>();
                    if(hoveredHex != null)
                    {
                        if(lastHoveredHex == hoveredHex)
                            return;
                        
                        lastHoveredHex = hoveredHex;
                        if(currentBoardState.allPiecePositions.ContainsKey(hoveredHex.index))
                        {
                            // Get the piece on that hex
                            hoveredPiece = board.activePieces[currentBoardState.allPiecePositions[hoveredHex.index]];
                            if(hoveredPiece != null && !hoveredPiece.captured)
                            {
                                IEnumerable<(Hex, MoveType)> incomingPreviewMoves = board.GetAllValidMovesForPiece(
                                    hoveredPiece,
                                    currentBoardState,
                                    true
                                );
                                if(incomingPreviewMoves != previewMoves)
                                {
                                    DisablePreview();
                                    previewMoves = incomingPreviewMoves;
                                    previewMoves = previewMoves.Append((hoveredHex, MoveType.None));
                                    
                                    threateningPieces = board.GetThreateningPieces(hoveredHex);
                                    guardingPieces = board.GetGuardingingPieces(hoveredHex);

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
                    {
                        lastHoveredHex = null;
                        DisablePreview();
                    }
                }
                else if(!hoveredPiece.captured)
                {
                    Hex hoveredPieceHex = board.GetHexIfInBounds(hoveredPiece.location);
                    if(lastHoveredHex == hoveredPieceHex)
                        return;

                    lastHoveredHex = hoveredPieceHex;
                    
                    IEnumerable<(Hex, MoveType)> incomingPreviewMoves = board.GetAllValidMovesForPiece(
                        hoveredPiece,
                        currentBoardState,
                        true
                    );

                    if(incomingPreviewMoves != previewMoves)
                    {
                        DisablePreview();
                        previewMoves = incomingPreviewMoves;

                        if(hoveredPieceHex != null)
                            previewMoves = previewMoves.Append((hoveredPieceHex, MoveType.None));

                        threateningPieces = board.GetThreateningPieces(hoveredPieceHex);
                        guardingPieces = board.GetGuardingingPieces(hoveredPieceHex);

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

    private void ColorizeThreat()
    {
        foreach(IPiece piece in threateningPieces)
        {
            MeshRenderer renderer = piece.obj.GetComponentInChildren<MeshRenderer>();
            renderer.material.SetColor("_BaseColor", orangeColor);
        }
    }

    private void ClearThreatHighlight()
    {
        foreach(IPiece piece in threateningPieces)
        {
            MeshRenderer renderer = piece.obj.GetComponentInChildren<MeshRenderer>();
            renderer.material.SetColor("_BaseColor", piece.team == Team.White ? whiteColor : blackColor);
        }
    }
    private void ColorizeGuard()
    {
        foreach(IPiece piece in guardingPieces)
        {
            MeshRenderer renderer = piece.obj.GetComponentInChildren<MeshRenderer>();
            renderer.material.SetColor("_BaseColor", greenColor);
        }
    }
    private void ClearGuardHighlight()
    {
        foreach(IPiece piece in guardingPieces)
        {
            MeshRenderer renderer = piece.obj.GetComponentInChildren<MeshRenderer>();
            renderer.material.SetColor("_BaseColor", piece.team == Team.White ? whiteColor : blackColor);
        }
    }
    

    private void EnablePreview()
    {
        foreach((Hex hex, MoveType moveType) in previewMoves)
        {
            hex.SetOutlineColor(hoverColor);
            hex.ToggleSelect();
        }

        ColorizeThreat();
        ColorizeGuard();
    }

    private void DisablePreview()
    {
        foreach ((Hex hex, MoveType moveType) in previewMoves)
            hex.ToggleSelect();
        previewMoves = Enumerable.Empty<(Hex, MoveType)>();

        ClearThreatHighlight();
        ClearGuardHighlight();
    }

    public void LeftClick(CallbackContext context)
    {
        if(context.started)
        {
            // Later allow players to queue a move, but for now, just prevent even clicking a piece when not their turn
            if(multiplayer != null && multiplayer.gameParams.localTeam != board.GetCurrentTurn())
                return;

            if(Cursor.visible && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
            {
                if(hit.collider == null)
                    return;
                
                BoardState currentBoardState = board.GetCurrentBoardState();
                IPiece clickedPiece = hit.collider.GetComponent<IPiece>();
                if(clickedPiece != null && clickedPiece.team == currentBoardState.currentMove)
                {
                    if(!clickedPiece.captured)
                        Select(currentBoardState, clickedPiece);
                    else if(!multiplayer && freePlaceMode.toggle.isOn)
                        Select(currentBoardState, clickedPiece, true);

                    audioSource.PlayOneShot(pickupNoise);
                }
            }
        }
        else if(context.canceled)
        {
            if(lastChangedRenderer != null)
                ResetLastChangedRenderer();

            if(Cursor.visible && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
            {
                BoardState currentBoardState = board.GetCurrentBoardState();
                IPiece hoveredPiece = hit.collider.GetComponent<IPiece>();

                if(hoveredPiece != null && selectedPiece != null)
                {
                    if(!hoveredPiece.captured)
                    {
                        if(!multiplayer && freePlaceMode.toggle.isOn && selectedPiece.captured)
                            GameObject.FindObjectsOfType<Jail>().Where(jail => jail.teamToPrison == selectedPiece.team).First().RemoveFromPrison(selectedPiece);

                        // Rooks can defend (swap positions with a near by ally)
                        if(hoveredPiece.team == selectedPiece.team && pieceMoves.Contains((board.GetHexIfInBounds(hoveredPiece.location), MoveType.Defend)))
                        {
                            Defend(hoveredPiece);
                            return;
                        }
                        else
                        {
                            Hex enemyHex = board.GetHexIfInBounds(hoveredPiece.location);
                            // Check if this attack is within our possible moves
                            if(pieceMoves.Contains((enemyHex, MoveType.Attack)))
                                MoveOrAttack(enemyHex);
                            else if(!multiplayer && freePlaceMode.toggle.isOn)
                            {
                                // Swap with ally, or take enemy, regardless of if the move is a potenial move
                                if(hoveredPiece.team == selectedPiece.team)
                                    Defend(hoveredPiece);
                                else
                                    MoveOrAttack(enemyHex);
                            }
                        }

                    }
                    // The piece was dropped on top of a piece in jail
                    else if(!multiplayer && freePlaceMode.toggle.isOn)
                    {
                        if(!selectedPiece.captured)
                            board.Enprison(selectedPiece);
                    }
                }

                // Clicked on a hex
                Hex hitHex = hit.collider.GetComponent<Hex>();
                if(hitHex != null && selectedPiece != null)
                {
                    IPiece otherPiece = currentBoardState.allPiecePositions.ContainsKey(hitHex.index) 
                        ? board.activePieces[currentBoardState.allPiecePositions[hitHex.index]] 
                        : null;

                    if(!multiplayer && freePlaceMode.toggle.isOn && selectedPiece.captured)
                        GameObject.FindObjectsOfType<Jail>().Where(jail => jail.teamToPrison == selectedPiece.team).First().RemoveFromPrison(selectedPiece);

                    if(pieceMoves.Contains((hitHex, MoveType.Attack)) || pieceMoves.Contains((hitHex, MoveType.Move)))
                        MoveOrAttack(hitHex);
                    else if(pieceMoves.Contains((hitHex, MoveType.Defend)))
                        Defend(otherPiece);
                    else if(pieceMoves.Contains((hitHex, MoveType.EnPassant)))
                        EnPassant(currentBoardState, hitHex);
                    else if(!multiplayer && freePlaceMode.toggle.isOn)
                    {
                        // Swap with ally, or take enemy, regardless of if the move is a potenial move
                        if(otherPiece != null && otherPiece.team == selectedPiece.team)
                            Defend(otherPiece);
                        else
                            MoveOrAttack(hitHex);
                    }
                }

                if(!multiplayer && freePlaceMode.toggle.isOn && selectedPiece != null)
                {
                    // Piece dropped on top of jail
                    Jail jail = hit.collider.GetComponent<Jail>();
                    if(jail && !selectedPiece.captured)
                        board.Enprison(selectedPiece);
                }
            }

            if(selectedPiece != null)
            {
                audioSource.PlayOneShot(cancelNoise);
                DeselectPiece(selectedPiece.location, selectedPiece.captured);
            }
        }
    }

    private void Select(BoardState currentBoardState, IPiece clickedPiece, bool fromJail = false)
    {
        if(selectedPiece != null)
            DeselectPiece(selectedPiece.location);

        // Select new piece and highlight all of the places it can move to on the current board state
        selectedPiece = clickedPiece;
        onMouse.PickUp(selectedPiece.obj);
        onMouse.SetColor(selectedPiece.team == Team.White ? whiteColor : blackColor);
        
        if(!fromJail)
        {
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

    private void MoveOrAttack(Hex hitHex)
    {
        Index pieceStartLoc = selectedPiece.location;

        bool fromJail = false;

        if(!board.activePieces.ContainsKey((selectedPiece.team, selectedPiece.piece)))
        {
            fromJail = true;
            board.activePieces.Add((selectedPiece.team, selectedPiece.piece), selectedPiece);
        }
        
        BoardState newState = board.MovePiece(selectedPiece, hitHex, board.GetCurrentBoardState());
        
        if(multiplayer != null)
        {
            // We skip sending a board here when a promotion is happening. That will be sent with the promotion after it's chosen
            if(!(selectedPiece is Pawn pawn) || pawn.goal != hitHex.index.row)
                multiplayer.SendBoard(newState);
        }

        board.AdvanceTurn(newState);
        DeselectPiece(pieceStartLoc, fromJail);
    }

    private void Defend(IPiece pieceToDefend)
    {
        Index startLoc = selectedPiece.location;
        bool fromJail = false;
        if(!board.activePieces.ContainsKey((selectedPiece.team, selectedPiece.piece)))
        {
            fromJail = true;
            board.activePieces.Add((selectedPiece.team, selectedPiece.piece), selectedPiece);
        }

        BoardState newState = board.Swap(selectedPiece, pieceToDefend, board.GetCurrentBoardState());
        
        if(multiplayer != null)
            multiplayer.SendBoard(newState);

        board.AdvanceTurn(newState);
        DeselectPiece(startLoc, fromJail);
    }

    private void EnPassant(BoardState currentBoardState, Hex hitHex)
    {
        int teamOffset = currentBoardState.currentMove == Team.White ? -2 : 2;
        Index enemyLoc = new Index(hitHex.index.row + teamOffset, hitHex.index.col);
        (Team enemyTeam, Piece enemyType) = currentBoardState.allPiecePositions[enemyLoc];
        BoardState newState = board.EnPassant((Pawn)selectedPiece, enemyTeam, enemyType, hitHex, currentBoardState);

        if(multiplayer != null)
            multiplayer.SendBoard(newState);

        board.AdvanceTurn(newState);
        DeselectPiece(selectedPiece.location);
    }

    private int GetGoal(Team team, int row) => team == Team.White ? 18 - (row % 2) : row % 2;

    public void DeselectPiece(Index fromIndex, bool fromJail = false)
    {
        if(selectedPiece == null)
            return;

        foreach((Hex hex, MoveType moveType) in pieceMoves)
            hex.ToggleSelect();
        pieceMoves = Enumerable.Empty<(Hex, MoveType)>();

        if(!fromJail)
            board.GetHexIfInBounds(fromIndex).ToggleSelect();

        onMouse.PutDown();
        hoverExitedInitialHex = false;
        
        selectedPiece = null;
    }
}