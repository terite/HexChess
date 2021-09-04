using System.Collections.Generic;
using System.Linq;
using Extensions;

[System.Serializable]
public struct Move
{
    public int turn;
    public Team lastTeam;
    public Piece lastPiece;
    public Index from;
    public Index to;
    public Piece? capturedPiece;
    public Piece? defendedPiece;
    public float duration;

    public Move(int turn, Team lastTeam, Piece lastPiece, Index from, Index to, Piece? capturedPiece = null, Piece? defendedPiece = null, float duration = 0)
    {
        this.turn = turn;
        this.lastTeam = lastTeam;
        this.lastPiece = lastPiece;
        this.from = from;
        this.to = to;
        this.capturedPiece = capturedPiece;
        this.defendedPiece = defendedPiece;
        this.duration = duration;
    }

    public string GetNotation(List<Promotion> promotions, BoardState boardState, NotationType notationType = NotationType.LongForm)
    {
        if(lastTeam == Team.None)
            return "";

        string fromIndex = from.GetKey();
        string toIndex = to.GetKey();
        string piece = GetStringForPiece(lastPiece, lastTeam, promotions);
        string type = capturedPiece.HasValue ? "x" : defendedPiece.HasValue ? "d" : "m";
        string otherPiece = type switch{
            "x" => GetStringForPiece(capturedPiece.Value, lastTeam.Enemy(), promotions),
            "d" => GetStringForPiece(defendedPiece.Value, lastTeam, promotions),
            _ => ""
        };

        if(from == Index.invalid && to == Index.invalid)
        {   
            // No piece moved - skipped move with free place mode
            fromIndex = "skipped";
            toIndex = "";
            piece = "";
            type = "";
            otherPiece = "";
        }
        else if(to == Index.invalid)
        {
            // Put in jail with free place mode
            otherPiece = " ";
            type = "";
            toIndex = "jailed";
        }
        else if(from == Index.invalid)
        {
            // Freed from jail with free place mode
            fromIndex = toIndex;
            toIndex = "freed";
            otherPiece = " ";
            type = "";
        }
        
        string promo = "";
        Team lt = lastTeam;
        Piece lp = lastPiece;

        IEnumerable<Promotion> applicablePromotions = promotions.Where(promo => promo.team == lt && promo.from == lp);
        if(applicablePromotions.Any())
        {
            Promotion applicablePromo = applicablePromotions.First();
            promo = applicablePromo.turnNumber == turn && lastTeam == applicablePromo.team ? $"={applicablePromo.to.GetPieceShortString()}" : "";
        }

        string check = "";
        if(boardState.checkmate != Team.None)
            check = "#";
        else if(boardState.check != Team.None)
            check = "+";

        // modify for shortform
        if(notationType == NotationType.ShortForm)
        {
            if(piece == "p")
            {
                piece = "";
                otherPiece = "";
                type = type == "x" ? type : "";
                fromIndex = type == "x" ? $"{fromIndex.First()}" : "";
            }
            else
            {
                Piece[] alternatePieces = lp.GetAlternates();
                if(alternatePieces.Length > 0)
                {
                    // Check for the need for disambiguaty, if not needed, do the same as if there was no alternate pieces
                    // If alternatePieces exist on same team, and threaten toIndex, then there is ambiguaty between moves
                    // If the fromInxex file differs, use only file, else if the fromIndex rank differs, use only rank, else if neither differ (in the case of a promotion), use both rank and file
                    // AlternatePieces doesn't account for promoted pawns. Check for any promotions of the same type as lastPiece or as either of the alternatePieces
                    // For example, a pawn promoted to a KingsKnight while the KingsKnight is still on the board may create ambiguaty
                    bool fileAmbiguaty = false;
                    bool rankAmbiguaty = false;
                    foreach(Piece alternate in alternatePieces)
                    {
                        if(boardState.TryGetIndex((lt, lp), out Index index))
                        {
                            // Need a solution here, getting all valid moves would build in a board (and thus unity) dependency to Move, which is supposed to be standalone
                            // MoveGenerator.GetAllPossibleMoves()
                        }
                    }
                }
                else
                {
                    fromIndex = "";
                    type = type == "m" ? "" : type;
                }
            }
        }

        return $"{piece}{fromIndex}{type}{otherPiece}{toIndex}{promo}{check}";
    }

    private string GetStringForPiece(Piece potentialPawn, Team team, List<Promotion> promotions)
    {
        if(potentialPawn < Piece.Pawn1)
            return potentialPawn.GetPieceShortString();

        // The piece may habe been promoted. If so, we want to return the promoted piece. But only if it's not the turn the promo happened on   
        IEnumerable<Promotion> applicablePromotions = promotions.Where(promo => promo.team == team && promo.from == potentialPawn);
        if(applicablePromotions.Any())
        {
            Promotion applicablePromo = applicablePromotions.First();
            string result = applicablePromo.turnNumber < turn || lastTeam != applicablePromo.team ? applicablePromo.to.GetPieceShortString() : potentialPawn.GetPieceShortString();
            return $"{result}";
        }
        
        return potentialPawn.GetPieceShortString();
    }
}

public enum NotationType:int {LongForm = 0, ShortForm = 1}