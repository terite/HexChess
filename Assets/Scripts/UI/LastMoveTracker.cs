using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using Extensions;

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

        Team otherTeam = move.lastTeam.Enemy();
        string lastPieceString = GetStringForPiece(move, move.lastPiece, move.lastTeam, board.promotions);
        IPiece capturedPiece = move.capturedPiece.HasValue 
            ? board.piecePrefabs[(otherTeam, move.capturedPiece.Value)].GetComponent<IPiece>()
            : null;

        string capturedPieceString = capturedPiece == null ? "" : capturedPiece.GetPieceString();
        
        if(capturedPieceString == "Pawn")
        {
            // If the piece captured was a pawn, it may have been promoted. To get the correct string, let's pull it from the promotion data instead of the Piece data
            IEnumerable<Promotion> applicablePromos = board.promotions.Where(promo => promo.team == otherTeam && promo.from == move.capturedPiece.Value);
            if(applicablePromos.Any())
            {
                Promotion promo = applicablePromos.First();
                // We have no way to get the IPiece for the captured piece. 
                // It's no longer in the activePieces dictionary, and it's been promoted, so it's not the same IPiece as the prefab.
                capturedPieceString = promo.to.GetPieceLongString();
            }
        }

        IPiece defendedPiece = move.defendedPiece.HasValue
            ? board.activePieces[(move.lastTeam, move.defendedPiece.Value)]
            : null;
    
        string promoStr = "";
        if(lastPieceString == "Pawn")
        {
            IEnumerable<Promotion> applicablePromos = board.promotions.Where(promo => promo.team == move.lastTeam && promo.from == move.lastPiece && promo.turnNumber <= move.turn);
            if(applicablePromos.Any())
            {
                Promotion promo = applicablePromos.First();
                promoStr = promo.to.GetPieceLongString();
            }
        }
        promoStr = string.IsNullOrEmpty(promoStr) ? promoStr : $" promoted to {promoStr}";

        // This is the default text to use
        string textToSet = move.capturedPiece.HasValue
            ? $"{lastPieceString} {from} takes {capturedPieceString} {to}{promoStr}"
            : move.defendedPiece.HasValue 
                ? $"{lastPieceString} {from} defends {defendedPiece.GetPieceString()} {to}{promoStr}" 
                : $"{lastPieceString} {from} to {to}{promoStr}";
  
        // No piece was moved - skipped move with free place mode
        if(move.from == Index.invalid && move.to == Index.invalid)
            textToSet = "Move skipped";
        // Put in jail with free place mode
        else if(move.to == Index.invalid)
            textToSet = $"{lastPieceString} {from} jailed";
        // Freed from jail with free place mode
        else if(move.from == Index.invalid)
            textToSet = $"Freed {lastPieceString} to {to}{promoStr}";

        text.text = textToSet;
        text.color = move.lastTeam == Team.White ? Color.white : Color.black;
    }

    private string GetStringForPiece(Move move, Piece potentialPawn, Team team, List<Promotion> promotions)
    {
        if(potentialPawn < Piece.Pawn1)
            return potentialPawn.GetPieceLongString();

        // The piece may habe been promoted. If so, we want to return the promoted piece. But only if it's not the turn the promo happened on   
        IEnumerable<Promotion> applicablePromotions = promotions.Where(promo => promo.team == team && promo.from == potentialPawn);
        if(applicablePromotions.Any())
        {
            Promotion applicablePromo = applicablePromotions.First();
            string result = applicablePromo.turnNumber <= move.turn - 1 && move.lastTeam == applicablePromo.team ? applicablePromo.to.GetPieceLongString() : potentialPawn.GetPieceLongString();
            return $"{result}";
        }
        
        return potentialPawn.GetPieceLongString();
    }
}