using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class SelectPiece : MonoBehaviour
{
    Mouse mouse => Mouse.current;
    Camera cam;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private HexSpawner boardSpawner;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Color selectedPieceColor;
    private Piece selectedPiece;
    List<Hex> pieceMoves = new List<Hex>();

    private void Awake() => cam = Camera.main;
    public void LeftClick(CallbackContext context)
    {
        if(!context.performed)
            return;
        
        if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
        {
            if(hit.collider == null)
                return;
            
            Piece piece = hit.collider.GetComponent<Piece>();
            if(piece != null && piece.team == boardManager.GetCurrentTurn())
            {
                if(selectedPiece == piece)
                    return;

                DeselectPiece();

                // Select new piece
                selectedPiece = piece;
                pieceMoves = boardManager.GetMovesOnCurrentBoardState(piece);
                foreach(Hex hex in pieceMoves)
                    hex.ToggleSelect();
                
                Index selectedLocation = selectedPiece.location;
                Hex selectedHex = boardSpawner.GetHexIfInBounds(selectedLocation.row, selectedLocation.col);
                selectedHex.ToggleSelect();
                selectedHex.SetOutlineColor(selectedPieceColor);
            }
        }
    }

    public void RightClick(CallbackContext context)
    {
        if(!context.performed)
            return;
        DeselectPiece();
    }
    public void DeselectPiece()
    {
        if(selectedPiece == null)
            return;

        foreach(Hex hex in pieceMoves)
            hex.ToggleSelect();
        pieceMoves.Clear();

        Index selectedLocation = selectedPiece.location;
        Hex selectedHex = boardSpawner.GetHexIfInBounds(selectedLocation.row, selectedLocation.col);
        selectedHex.SetOutlineColor(Color.green);
        selectedHex.ToggleSelect();
        
        selectedPiece = null;
    }
}
