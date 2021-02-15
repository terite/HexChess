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

    public void UpdateText(Team team, Piece piece, Index from, Index to)
    {
        if(!gameObject.activeSelf)
            gameObject.SetActive(true);

        string sFrom = $"{GetLetter(from)}{GetNumber(from)}";
        string sTo = $"{GetLetter(to)}{GetNumber(to)}";
        text.text = $"{GetPieceString(piece)} {sFrom} to {sTo}";
        text.color = team == Team.White ? Color.white : Color.black;
    }

    public int GetNumber(Index i) => ((float)i.row/2f).Floor() + 1;

    public string GetLetter(Index i)
    {
        bool isEven = i.row % 2 == 0;

        return i.col switch{
            0 when !isEven => "A", 0 when isEven => "B",
            1 when !isEven => "C", 1 when isEven => "D",
            2 when !isEven => "E", 2 when isEven => "F",
            3 when !isEven => "G", 3 when isEven => "H",
            4 => "I", _ => ""
        };
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