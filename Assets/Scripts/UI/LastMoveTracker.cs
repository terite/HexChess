using UnityEngine;
using TMPro;
using Extensions;

public class LastMoveTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private void Awake() {
        text.text = string.Empty;
        gameObject.SetActive(false);
    }

    public void UpdateText(Move move)
    {
        if(!gameObject.activeSelf)
            gameObject.SetActive(true);

        string from = move.from.GetKey();
        string to = move.to.GetKey();

        text.text = move.capturedPiece.HasValue
            ? $"{GetPieceString(move.lastPiece)} {from} takes {GetPieceString(move.capturedPiece.Value)} {to}"
            : move.defendedPiece.HasValue 
                ? $"{GetPieceString(move.lastPiece)} {from} defends {GetPieceString(move.defendedPiece.Value)} {to}" 
                : $"{GetPieceString(move.lastPiece)} {from} to {to}";

        text.color = move.lastTeam == Team.White ? Color.white : Color.black;
    }
    
    public string GetPieceString(Piece piece) => piece switch {
        Piece.King => "King",
        Piece.Queen => "Queen",
        Piece p when (p == Piece.KingsKnight || p == Piece.QueensKnight) => "Knight",
        Piece p when (p == Piece.KingsRook || p == Piece.QueensRook) => "Rook",
        Piece p when (p == Piece.KingsBishop || p == Piece.QueensBishop) => "Bishop",
        Piece p when (p == Piece.WhiteSquire || p == Piece.GraySquire || p == Piece.BlackSquire) => "Squire",
        Piece p when (
            p == Piece.Pawn1 || p == Piece.Pawn2 ||
            p == Piece.Pawn3 || p == Piece.Pawn4 ||
            p == Piece.Pawn5 || p == Piece.Pawn6 ||
            p == Piece.Pawn7 || p == Piece.Pawn8
        ) => "Pawn",
        _ => ""
    };
}