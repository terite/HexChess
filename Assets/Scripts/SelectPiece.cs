using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Linq;

public class SelectPiece : MonoBehaviour
{
    Mouse mouse => Mouse.current;
    Camera cam;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private HexSpawner boardSpawner;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Color selectedPieceColor;
    private IPiece selectedPiece;
    IEnumerable<Hex> pieceMoves;

    private void Awake() => cam = Camera.main;
    public void LeftClick(CallbackContext context)
    {
        if(!context.performed)
            return;
        
        if(Physics.Raycast(cam.ScreenPointToRay(mouse.position.ReadValue()), out RaycastHit hit, layerMask))
        {
            if(hit.collider == null)
                return;
            
            IPiece piece = hit.collider.GetComponent<IPiece>();
            if(piece != null && piece.team == boardManager.GetCurrentTurn())
            {
                if(selectedPiece == piece)
                    return;

                DeselectPiece();

                // Select new piece and highlight all of the places it can move to on the current board state
                selectedPiece = piece;
                pieceMoves = piece.GetAllPossibleMoves(boardSpawner, boardManager.GetCurrentBoardState());
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
        pieceMoves = Enumerable.Empty<Hex>();

        Index selectedLocation = selectedPiece.location;
        Hex selectedHex = boardSpawner.GetHexIfInBounds(selectedLocation.row, selectedLocation.col);
        selectedHex.SetOutlineColor(Color.green);
        selectedHex.ToggleSelect();
        
        selectedPiece = null;
    }
}
