using System.Collections.Generic;
using System.Linq;
using Extensions;
using TMPro;
using UnityEngine;

public class MovePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnNumberText;
    [SerializeField] private TextMeshProUGUI whiteText;
    [SerializeField] private TextMeshProUGUI blackText;
    Board board;
    private void Awake() => board = GameObject.FindObjectOfType<Board>();

    public void SetMove(Move move)
    {
        TextMeshProUGUI toChange = move.lastTeam == Team.White ? whiteText : blackText;
        
        string piece = GetStringForPotentialPromotion(move.lastPiece, move.lastTeam);
        string fromIndex = move.from.GetKey();
        string toIndex = move.to.GetKey();
        string type = move.capturedPiece.HasValue ? "x" : move.defendedPiece.HasValue ? "d" : "m";
        
        string otherPiece = type switch{
            "x" => move.capturedPiece.HasValue 
                ? GetStringForPotentialPromotion(move.capturedPiece.Value, move.lastTeam == Team.White ? Team.Black : Team.White) 
                : "",
            "d" => move.defendedPiece.HasValue 
                ? GetStringForPotentialPromotion(move.defendedPiece.Value, move.lastTeam) 
                : "",
            _ => ""
        };        

        toChange.text = $"{piece}{fromIndex}{type}{otherPiece}{toIndex}";

        if(move.lastTeam == Team.White)
            blackText.text = "";
    }

    public void SetTurnNumber(int val) => turnNumberText.text = $"{val}";

    public string GetStringForPotentialPromotion(Piece potentialPawn, Team team)
    {
        if(potentialPawn < Piece.Pawn1)
            return potentialPawn.GetPieceShortString();
        
        IEnumerable<Promotion> applicablePromotions = board.promotions.Where(promo => promo.team == team && promo.from == potentialPawn);
        if(applicablePromotions.Any())
            return $"{applicablePromotions.First().to.GetPieceShortString()}";
        
        return potentialPawn.GetPieceShortString();
    }
}