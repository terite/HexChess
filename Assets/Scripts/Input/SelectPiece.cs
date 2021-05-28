using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Linq;
using System;
using TMPro;

public class SelectPiece : MonoBehaviour
{
    Mouse mouse => Mouse.current;
    Multiplayer multiplayer;
    PreviewMovesToggle singlePlayerMovesToggle;
    Camera cam;
    [SerializeField] private Board board;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask hexMask;
    [SerializeField] private LayerMask keysMask;
    [SerializeField] private Color selectedPieceColor;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private PromotionDialogue promotionDialogue;
    public AudioClip cancelNoise;
    public AudioClip pickupNoise;
    public IPiece selectedPiece {get; private set;}
    [SerializeField] private OnMouse onMouse;
    [SerializeField] private FreePlaceModeToggle freePlaceMode;
    private bool hoverExitedInitialHex = false;
    List<(Hex, MoveType)> pieceMoves = new List<(Hex, MoveType)>();
    List<(Hex, MoveType)> previewMoves = new List<(Hex, MoveType)>();
    public List<Color> moveTypeHighlightColors = new List<Color>();
    public Color greenColor;
    public Color redColor;
    public Color orangeColor;
    public Color hoverColor;
    public Color whiteColor;
    public Color blackColor;
    List<IPiece> attacksConcerningHex = new List<IPiece>();
    Dictionary<IPiece, List<IPiece>> attacksConcerningHexDict = new Dictionary<IPiece, List<IPiece>>();
    MeshRenderer lastChangedRenderer;
    IPiece lastChangedPiece;

    private Hex lastHoveredHex = null;
    private Hex lastHoveredHexForKeyHighlight = null;

    private Keys keys;

    private TextMeshPro lastHoveredKey = null;
    IEnumerable<Hex> keyHighlightedHexes = Enumerable.Empty<Hex>();

    private void Awake() 
    {
        cam = Camera.main;
        multiplayer = GameObject.FindObjectOfType<Multiplayer>();
        if(multiplayer == null)
            singlePlayerMovesToggle = GameObject.FindObjectOfType<PreviewMovesToggle>();
        
        keys = GameObject.FindObjectOfType<Keys>();

        board.newTurn += NewTurn;
    }

    private void NewTurn(BoardState newState) => attacksConcerningHexDict.Clear();

    private void Update()
    {
        MovePreviewsOnHover();
        ColorizeBasedOnMove();
        HighlightKeysOnHoverHex();
        HighlightHexOnHoverKey();
    }

    private void HighlightHexOnHoverKey()
    {
        if(onMouse.isPickedUp)
            return;
        else if(multiplayer != null && !multiplayer.gameParams.showMovePreviews)
            return;
        else if(singlePlayerMovesToggle != null && !singlePlayerMovesToggle.toggle.isOn) 
            return;

        if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, 100, keysMask))
        {
            TextMeshPro hitKey = hit.collider.GetComponent<TextMeshPro>();
            if(hitKey == null)
            {
                foreach(Hex hex in keyHighlightedHexes)
                    hex.ToggleSelect();
                
                keyHighlightedHexes = Enumerable.Empty<Hex>();
                lastHoveredKey = null;
                return;
            }
            
            if(lastHoveredKey == hitKey)
                return;

            foreach(Hex hex in keyHighlightedHexes)
                hex.ToggleSelect();
            
            keyHighlightedHexes = Enumerable.Empty<Hex>();
            
            if(int.TryParse(hitKey.text, out int hitNum))
            {
                // Fetch all hexes in proper rows
                (int desiredRow1, int desiredRow2) = hitNum switch {
                    1 => (0, 1), 2 => (2, 3),
                    3 => (4, 5), 4 => (6, 7),
                    5 => (8, 9), 6 => (10, 11),
                    7 => (12, 13), 8 => (14, 15),
                    9 => (16, 17), 10 => (18, 19),
                    _ => (-1, -1) 
                };

                if(desiredRow1 == -1)
                    return;
                
                IEnumerable<Hex> hexesInRow1 = board.hexes.Count > desiredRow1 ? board.hexes[desiredRow1] : Enumerable.Empty<Hex>();
                IEnumerable<Hex> hexesInRow2 = board.hexes.Count > desiredRow2 ? board.hexes[desiredRow2] : Enumerable.Empty<Hex>();
                IEnumerable<Hex> hexesInRow = hexesInRow1.Concat(hexesInRow2);

                foreach(Hex hex in hexesInRow)
                {
                    hex.SetOutlineColor(orangeColor);
                    hex.ToggleSelect();
                }
                keyHighlightedHexes = hexesInRow;
            }
            else
            {
                // Fetch all hexes in proper col
                (bool isEven, int c) = hitKey.text switch {
                    "A" => (false, 0), "B" => (true, 0),
                    "C" => (false, 1), "D" => (true, 1),
                    "E" => (false, 2), "F" => (true, 2),
                    "G" => (false, 3), "H" => (true, 3),
                    "I" => (false, 4), _ => (false, -1)
                };

                if(c == -1)
                    return;
                
                IEnumerable<Hex> hexesInCol = board.GetHexesInCol(c);
                hexesInCol = hexesInCol.Where(hex => hex.index.row % 2 == 0 == isEven);
                foreach(Hex hex in hexesInCol)
                {
                    hex.SetOutlineColor(orangeColor);
                    hex.ToggleSelect();
                }
                keyHighlightedHexes = hexesInCol;
            }

            lastHoveredKey = hitKey;
        }
        else
        {
            foreach(Hex hex in keyHighlightedHexes)
                hex.ToggleSelect();
            
            keyHighlightedHexes = Enumerable.Empty<Hex>();
            lastHoveredKey = null;
        }
    }

    private void HighlightKeysOnHoverHex()
    {
        if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, 100, hexMask))
        {
            if(hit.collider == null)
            {   
                if(lastHoveredHexForKeyHighlight != null)
                {
                    keys.Clear();
                    lastHoveredHexForKeyHighlight = null;
                }
                return;
            }
            Hex hoveredHex = hit.collider.GetComponent<Hex>();
            if(hoveredHex == null)
            {
                if(lastHoveredHexForKeyHighlight != null)
                {
                    keys.Clear();
                    lastHoveredHexForKeyHighlight = null;
                }
                return;
            }
            else if(hoveredHex == lastHoveredHexForKeyHighlight)
                return;
                
            keys.HighlightKeys(hoveredHex.index);
            lastHoveredHexForKeyHighlight = hoveredHex;
        }
        else if(lastHoveredHexForKeyHighlight != null)
        {
            keys.Clear();
            lastHoveredHexForKeyHighlight = null;
        }
    }

    private void ColorizeBasedOnMove()
    {
        if(selectedPiece != null && onMouse != null)
        {
            if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, 100, layerMask))
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

                                hitRenderer.material.SetColor("_HighlightColor", toSet);
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

                                        hitPieceRenderer.material.SetColor("_HighlightColor", toSet);
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
        lastChangedRenderer?.material.SetColor("_HighlightColor", lastChangedPiece?.team == Team.White ? whiteColor : blackColor);
        lastChangedPiece = null;
        lastChangedRenderer = null;
    }

    private void MovePreviewsOnHover()
    {
        if(multiplayer != null && !multiplayer.gameParams.showMovePreviews)
            return;
        else if(singlePlayerMovesToggle != null && !singlePlayerMovesToggle.toggle.isOn) 
            return;
        else if(promotionDialogue.gameObject.activeSelf)
            return;

        if(selectedPiece == null)
        {
            if(Cursor.visible && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, 100, hexMask))
            {
                BoardState currentBoardState = board.GetCurrentBoardState();
                IPiece hoveredPiece = null;
                Hex hoveredHex = hit.collider.GetComponent<Hex>();
                if(hoveredHex != null)
                {
                    if(lastHoveredHex == hoveredHex)
                        return;
                    
                    lastHoveredHex = hoveredHex;
                    if(currentBoardState.allPiecePositions.ContainsKey(hoveredHex.index))
                    {
                        // Get the piece on that hex
                        (Team t, Piece p) = currentBoardState.allPiecePositions[hoveredHex.index];
                        if(board.activePieces.ContainsKey((t, p)))
                            hoveredPiece = board.activePieces[(t, p)];
                        if(hoveredPiece != null && !hoveredPiece.captured)
                        {
                            IEnumerable<(Hex targetIndex, MoveType moveType)> incomingPreviewMoves = board.GetAllValidMovesForPiece(
                                hoveredPiece,
                                currentBoardState,
                                true
                            ).Select(kvp => (board.GetHexIfInBounds(kvp.target), kvp.moveType));

                            DisablePreview();
                            previewMoves = incomingPreviewMoves.ToList();
                            previewMoves.Add((hoveredHex, MoveType.None));

                            if (!attacksConcerningHexDict.ContainsKey(hoveredPiece))
                                attacksConcerningHexDict.Add(hoveredPiece, board.GetValidAttacksConcerningHex(hoveredHex).ToList());

                            EnablePreview();
                        }
                        else if(previewMoves.Count > 0)
                            DisablePreview();
                    }
                    else if(previewMoves.Count > 0)
                        DisablePreview();
                }
                else if(previewMoves.Count > 0)
                {
                    lastHoveredHex = null;
                    DisablePreview();
                }
                else if(previewMoves.Count > 0)
                    DisablePreview();
            }
            else if(previewMoves.Count > 0)
                DisablePreview();
        }
        else if(previewMoves.Count > 0)
            DisablePreview();
    }

    private void ColorizePieces()
    {
        IPiece hoveredPiece = board.activePieces[board.GetCurrentBoardState().allPiecePositions[lastHoveredHex.index]];
        if(!attacksConcerningHexDict.ContainsKey(hoveredPiece)) 
            return;

        attacksConcerningHex = attacksConcerningHexDict[hoveredPiece];
        foreach(IPiece piece in attacksConcerningHex)
        {
            MeshRenderer renderer = piece.obj.GetComponentInChildren<MeshRenderer>();
            renderer.material.SetColor("_HighlightColor", piece.team == hoveredPiece.team ? greenColor : orangeColor);
        }
    }


    private void ClearPiecesColorization(List<IPiece> set)
    {
        foreach(IPiece piece in set)
        {
            MeshRenderer renderer = piece.obj.GetComponentInChildren<MeshRenderer>();
            renderer.material.SetColor("_HighlightColor", piece.team == Team.White ? whiteColor : blackColor);
        }
    }
    

    private void EnablePreview()
    {
        foreach((Hex hex, MoveType moveType) in previewMoves)
        {
            hex.SetOutlineColor(hoverColor);
            hex.ToggleSelect();
        }
        
        ColorizePieces();
    }

    private void DisablePreview()
    {
        foreach ((Hex hex, MoveType moveType) in previewMoves)
            hex.ToggleSelect();
        previewMoves.Clear();

        ClearPiecesColorization(attacksConcerningHex);
    }

    public void LeftClick(CallbackContext context)
    {
        if(promotionDialogue.gameObject.activeSelf)
            return;
        
        BoardState currentBoardState = board.GetCurrentBoardState();
        if(context.started)
            MouseDown(currentBoardState);
        else if(context.canceled)
            ReleaseMouse(currentBoardState);
    }

    private void MouseDown(BoardState currentBoardState)
    {
        // Later allow players to queue a move, but for now, just prevent even clicking a piece when not their turn
        if(multiplayer != null && multiplayer.gameParams.localTeam != board.GetCurrentTurn())
            return;

        // When moveing pieces on the board, we determine what piece is being moved by the hex the player is hovering
        if(Cursor.visible && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hexHit, 100, hexMask))
        {
            if(hexHit.collider == null)
                return;

            if(hexHit.collider.TryGetComponent<Hex>(out Hex clickedHex) && currentBoardState.allPiecePositions.ContainsKey(clickedHex.index))
            {
                IPiece pieceOnHex = board.activePieces[currentBoardState.allPiecePositions[clickedHex.index]];
                if(pieceOnHex.team == currentBoardState.currentMove)
                {
                    Select(currentBoardState, pieceOnHex);
                    audioSource.PlayOneShot(pickupNoise);
                    return;
                }
            }
        }

        // But pulling pieces ouf of jail in free place mode doesn't have a hex for us to check, so let's cast a ray and check that instead.
        if(!multiplayer && freePlaceMode.toggle.isOn)
        {
            if(Cursor.visible && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit pieceHit, 100, layerMask))
            {
                if(pieceHit.collider == null)
                    return;
                
                if(pieceHit.collider.TryGetComponent<IPiece>(out IPiece clickedPiece) && clickedPiece.team == currentBoardState.currentMove && clickedPiece.captured)
                {
                    Select(currentBoardState, clickedPiece, true);
                    audioSource.PlayOneShot(pickupNoise);
                    return;
                }
            }
        }
    }

    private void ReleaseMouse(BoardState currentBoardState)
    {
        if(lastChangedRenderer != null)
            ResetLastChangedRenderer();

        if(Cursor.visible && Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, 100))
        {
            if(hit.collider.TryGetComponent<IPiece>(out IPiece hoveredPiece) && selectedPiece != null)
            {
                if(!hoveredPiece.captured)
                {
                    Hex otherPieceOccupiedHex = board.GetHexIfInBounds(hoveredPiece.location);
                    if(!multiplayer && freePlaceMode.toggle.isOn)
                    {
                        // Free place mode override
                        // If the hex is not empty, swap with ally, or take enemy, regardless of if the move is a potenial move
                        // Else the hex is empty, so just put the piece at the proper location
                        Jail jail = selectedPiece.captured 
                            ? GameObject.FindObjectsOfType<Jail>()
                                .Where(jail => jail.teamToPrison == selectedPiece.team)
                                .First()
                            : null;
                        
                        if(jail != null)
                            jail.RemoveFromPrison(selectedPiece);
                        
                        if(hoveredPiece.team == selectedPiece.team)
                        {
                            // If jail is no null, that means our selected piece was removed from jail.
                            // To swap with something in jail, we just need to send the piece there to jail first, then move to it
                            // If it's not in jail, just swap the 2 pieces
                            if(jail != null)
                            {
                                jail.Enprison(hoveredPiece);
                                MoveOrAttack(otherPieceOccupiedHex);
                            }
                            else
                                Defend(hoveredPiece);
                        }
                        else
                            MoveOrAttack(otherPieceOccupiedHex);
                    }
                    else if(pieceMoves.Contains((otherPieceOccupiedHex, MoveType.Attack)))
                        MoveOrAttack(otherPieceOccupiedHex);
                    else if(pieceMoves.Contains((otherPieceOccupiedHex, MoveType.Defend)))
                        Defend(hoveredPiece);
                    else if(pieceMoves.Contains((otherPieceOccupiedHex, MoveType.EnPassant)))
                        EnPassant(currentBoardState, otherPieceOccupiedHex);
                }
                // The piece was dropped on top of a piece in jail
                else if(!multiplayer && freePlaceMode.toggle.isOn)
                {
                    if(!selectedPiece.captured)
                        board.Enprison(selectedPiece);
                }
            }

            // Hovering on a hex
            if(hit.collider.TryGetComponent<Hex>(out Hex hitHex) && selectedPiece != null)
            {
                IPiece otherPiece = currentBoardState.allPiecePositions.ContainsKey(hitHex.index) 
                    ? board.activePieces[currentBoardState.allPiecePositions[hitHex.index]] 
                    : null;

                if(!multiplayer && freePlaceMode.toggle.isOn)
                {
                    // Free place mode override
                    // If the hex is not empty, swap with ally, or take enemy, regardless of if the move is a potenial move
                    // Else the hex is empty, so just put the piece at the proper location
                    Jail jail = selectedPiece.captured 
                        ? GameObject.FindObjectsOfType<Jail>()
                            .Where(jail => jail.teamToPrison == selectedPiece.team)
                            .First() 
                        : null;

                    if(jail != null)
                        jail.RemoveFromPrison(selectedPiece);
                        
                    if(otherPiece != null && otherPiece.team == selectedPiece.team)
                    {
                        // If jail is no null, that means our selected piece was removed from jail.
                        // To swap with something in jail, we just need to send the piece there to jail first, then move to it
                        // If it's not in jail, just swap the 2 pieces
                        if(jail != null)
                        {
                            jail.Enprison(otherPiece);
                            MoveOrAttack(hitHex);
                        }
                        else
                            Defend(otherPiece);
                    }
                    else
                        MoveOrAttack(hitHex);
                }
                else if(pieceMoves.Contains((hitHex, MoveType.Attack)) || pieceMoves.Contains((hitHex, MoveType.Move)))
                    MoveOrAttack(hitHex);
                else if(pieceMoves.Contains((hitHex, MoveType.Defend)))
                    Defend(otherPiece);
                else if(pieceMoves.Contains((hitHex, MoveType.EnPassant)))
                    EnPassant(currentBoardState, hitHex);
            }

            if(!multiplayer && freePlaceMode.toggle.isOn && selectedPiece != null)
            {
                // Piece dropped on top of jail
                if(hit.collider.TryGetComponent<Jail>(out Jail jail) && !selectedPiece.captured)
                    board.Enprison(selectedPiece);
                
                Hex fromHex = board.GetHexIfInBounds(selectedPiece.location);
                fromHex.ToggleSelect();
            }
        }
        if(selectedPiece != null)
        {
            audioSource.PlayOneShot(cancelNoise);
            DeselectPiece(selectedPiece.location, selectedPiece.captured);
        }
    }

    private void Select(BoardState currentBoardState, IPiece clickedPiece, bool fromJail = false)
    {
        if(selectedPiece != null)
            DeselectPiece(selectedPiece.location);

        // Select new piece and highlight all of the places it can move to on the current board state
        selectedPiece = clickedPiece;
        onMouse.PickUp(selectedPiece.obj);
        onMouse.SetBaseColor(selectedPiece.team == Team.White ? whiteColor : blackColor);
        
        if(!fromJail)
        {
            pieceMoves = board.GetAllValidMovesForPiece(selectedPiece, currentBoardState)
                .Select(kvp => (board.GetHexIfInBounds(kvp.target), kvp.moveType))
                .ToList();
            
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
        
        
        if((selectedPiece is Pawn pawn) && pawn.GetGoalInRow(hitHex.index.row) == hitHex.index.row)
        {
            // We don't send a boardstate right now when multiplayer, as the promotion will finish that for us
            board.MovePieceForPromotion(selectedPiece, hitHex, board.GetCurrentBoardState());
            DeselectPiece(pieceStartLoc, fromJail);
            return;
        }
        else
        {
            BoardState newState = board.MovePiece(selectedPiece, hitHex.index, board.GetCurrentBoardState());
            if(multiplayer != null)
                multiplayer.SendBoard(newState);
            board.AdvanceTurn(newState);
            DeselectPiece(pieceStartLoc, fromJail);
        }
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
        Index startLoc = selectedPiece.location;
        Index enemyLoc = HexGrid.GetNeighborAt(hitHex.index, currentBoardState.currentMove == Team.White ? HexNeighborDirection.Down : HexNeighborDirection.Up).Value;
        (Team enemyTeam, Piece enemyType) = currentBoardState.allPiecePositions[enemyLoc];
        BoardState newState = board.EnPassant((Pawn)selectedPiece, enemyTeam, enemyType, hitHex.index, currentBoardState);

        if(multiplayer != null)
            multiplayer.SendBoard(newState);

        board.AdvanceTurn(newState);
        DeselectPiece(startLoc);
    }

    private int GetGoal(Team team, int row) => team == Team.White ? 18 - (row % 2) : row % 2;

    public void DeselectPiece(Index fromIndex, bool fromJail = false)
    {
        if(selectedPiece == null)
            return;
        
        // Debug.Log($"Desllecting: {selectedPiece.obj.name}");

        foreach((Hex hex, MoveType moveType) in pieceMoves)
            hex.ToggleSelect();
        pieceMoves.Clear();

        if(!fromJail)
            board.GetHexIfInBounds(fromIndex).ToggleSelect();

        onMouse.PutDown();
        hoverExitedInitialHex = false;
        lastHoveredHex = null;
        
        selectedPiece = null;
    }
}