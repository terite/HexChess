using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class LastMoveTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    Board board;

    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        text.text = string.Empty;
        gameObject.SetActive(false);
    }

    public void UpdateText(Move move)
    {
        if(!gameObject.activeSelf)
            gameObject.SetActive(true);

        string from = move.from.GetKey();
        string to = move.to.GetKey();

        Team otherTeam = move.lastTeam == Team.White ? Team.Black : Team.White;
        string lastPieceString = board.activePieces[(move.lastTeam, move.lastPiece)].GetPieceString();
        
        IPiece capturedPiece = move.capturedPiece.HasValue 
            ? board.piecePrefabs[(otherTeam, move.capturedPiece.Value)].GetComponent<IPiece>()
            : null;

        string capturedPieceString = capturedPiece == null ? "" : capturedPiece.GetPieceString();
        
        if(capturedPieceString == "Pawn")
        {
            // If the piece captured was a pawn, it may have been promoted. To get the correct string, let's pull it from the promotion data instead of the Piece data
            IEnumerable<Promotion> applicablePromos = board.promotions.Where(promo => promo.team == otherTeam && promo.from == move.capturedPiece.Value);
            if(applicablePromos.Count() > 0)
            {
                Promotion promo = applicablePromos.First();
                // We have no way to get the IPiece for the captured piece. 
                // It's no longer in the activePieces dictionary, and it's been promoted, so it's not the same IPiece as the prefab.
                capturedPieceString = GetPieceString(promo.to);
            }
        }

        IPiece defendedPiece = move.defendedPiece.HasValue
            ? board.activePieces[(move.lastTeam, move.defendedPiece.Value)]
            : null;

        text.text = move.capturedPiece.HasValue
            ? $"{lastPieceString} {from} takes {capturedPieceString} {to}"
            : move.defendedPiece.HasValue 
                ? $"{lastPieceString} {from} defends {defendedPiece.GetPieceString()} {to}" 
                : $"{lastPieceString} {from} to {to}";

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