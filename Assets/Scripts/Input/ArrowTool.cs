using System.Collections.Generic;
using System.Linq;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class ArrowTool : MonoBehaviour
{
    private Camera cam;
    private Mouse mouse => Mouse.current;
    private Board board;
    private Multiplayer multiplayer;
    private PreviewMovesToggle singlePlayerMovesToggle;

    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private PromotionDialogue promotionDialogue;
    [SerializeField] private SelectPiece selectPiece;
    [ShowInInspector, ReadOnly] public bool arrowsVisible {get; private set;} = false;
    [ShowInInspector, ReadOnly] public List<Arrow> arrows {get; private set;} = new List<Arrow>();

    public LayerMask hexMask;
    public float arrowYOffset;
    public Color captureColor;
    public Color threatenedColor;
    public Color defendColor;
    public Color defaultArrowColor;
    
    private Hex startHex;

    private bool promoDialogueEnabled => promotionDialogue != null && promotionDialogue.gameObject.activeSelf;
    private bool handicapOverlayEnabled => (multiplayer != null && multiplayer.gameParams.showMovePreviews) || (singlePlayerMovesToggle != null && singlePlayerMovesToggle.toggle.isOn);

    private void Awake() 
    {
        cam = Camera.main;
        board = GameObject.FindObjectOfType<Board>();
        multiplayer = GameObject.FindObjectOfType<Multiplayer>();
        if(multiplayer == null)
            singlePlayerMovesToggle = GameObject.FindObjectOfType<PreviewMovesToggle>();
    }

    public void Input(CallbackContext context)
    {
        if(promoDialogueEnabled)
            return;
        
        if(Cursor.lockState == CursorLockMode.Locked || selectPiece.selectedPiece != null)
            return;

        if(context.started)
            StartDrawArrow();
        else if(context.canceled)
            EndDrawArrow();
    }

    private void StartDrawArrow()
    {
        if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, 100, hexMask))
        {
            if(hit.collider == null)
                return;
            
            if(hit.collider.TryGetComponent(out Hex hitHex))
                startHex = hitHex;
        }
    }

    private void EndDrawArrow()
    {
        // All arrows must have both a startHex and an end hex
        if(startHex == null)
            return;
        
        if(Cursor.lockState == CursorLockMode.Locked)
        {
            startHex = null;
            return;
        }

        if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, 100, hexMask))
        {
            if(hit.collider == null)
            {
                startHex = null;
                return;
            }
            
            if(hit.collider.TryGetComponent(out Hex endHex) && endHex != startHex)
            {
                // When handicap overlay is turned off, we don't want to color the arrows, just draw blue ones
                if(!handicapOverlayEnabled)
                    DrawArrow(startHex, endHex);
                else
                {
                    BoardState state = board.GetCurrentBoardState();
                    // When drawing an arrow between 2 hexes that have pieces on them, if those pieces belong to different teams, change the color of the arrow
                    if(state.TryGetPiece(startHex.index, out var hex1TeamedPiece))
                    {
                        if(state.TryGetPiece(endHex.index, out var hex2TeamedPiece))
                            DrawArrow(startHex, endHex, hex1TeamedPiece.team != hex2TeamedPiece.team ? captureColor : defendColor);
                        else
                        {
                            // check if any piece on the opposite team threatens endhex
                            if(board.GetAllValidMovesFromTeamConcerningHex(hex1TeamedPiece.team.Enemy(), endHex).Any())
                                DrawArrow(startHex, endHex, threatenedColor);
                            else
                                DrawArrow(startHex, endHex);
                        }
                    }
                    else
                        DrawArrow(startHex, endHex);
                }
            }
        }

        startHex = null;
    }

    public void DrawArrow(Hex from, Hex to, Color? alternateColor = null)
    {
        if(!arrowsVisible)
            arrowsVisible = true;
    
        Arrow newArrow = Instantiate(arrowPrefab);
        arrows.Add(newArrow);

        Vector3 offset = (Vector3.up * arrowYOffset);
        newArrow.Init(
            from: from.transform.position + offset, 
            to: to.transform.position + offset, 
            color: alternateColor == null ? defaultArrowColor : alternateColor.Value
        );
    }

    public void ClearArrows()
    {
        arrowsVisible = false;
        arrows.ForEach(arrow => Destroy(arrow.gameObject));
        arrows.Clear();
    }
}