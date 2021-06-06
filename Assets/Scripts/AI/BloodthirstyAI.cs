using System.Collections.Generic;
using System.Linq;
using Extensions;

public class BloodthirstyAI : IHexAI
{
    System.Random random = new System.Random();

    public HexAIMove GetMove(Board board)
    {
        var moves = HexAIMove.GenerateAllValidMoves(board).ToArray();
        return moves
            .OrderByDescending(move => Evaluate(move, board))
            .First();
    }

    const int CheckBonus = 50;
    const int StalematePenalty = 50;
    const int AttackBonus = 100;
    const int EnPassantBonus = 110;

    private int Evaluate(HexAIMove move, Board board)
    {
        int score = random.Next(0, 25); // random move bonus for giggles
        var state = board.GetCurrentBoardState();
        Team ourTeam = state.currentMove;
        Team enemy = ourTeam.Enemy();

        (BoardState newState, List<Promotion> newPromotions) = move.Speculate(board);

        bool weAreChecking = newState.IsChecking(ourTeam, newPromotions);
        bool enemyHasMoves = newState.HasAnyValidMoves(enemy, newPromotions, state);

        if (weAreChecking && !enemyHasMoves)
            return int.MaxValue;

        if (weAreChecking)
            score += CheckBonus;

        if (!enemyHasMoves)
            score -= StalematePenalty;

        if (move.moveType == MoveType.Attack)
        {
            var attacker = state.GetRealPiece(move.start, board.promotions);
            var victim = state.GetRealPiece(move.target, board.promotions);
            score += AttackBonus;
            score += GetPieceValue(victim) - GetPieceValue(attacker);
        }
        else if (move.moveType == MoveType.EnPassant)
        {
            score += EnPassantBonus;
        }
        else if (move.moveType == MoveType.Move)
        {
            var mover = state.allPiecePositions[move.start];
            if (mover.piece.IsPawn())
            {
                int ranksForward = move.target.GetNumber() - move.start.GetNumber();
                if (ourTeam == Team.Black)
                    ranksForward = -ranksForward;

                ranksForward *= 5;

                score *= ranksForward;
            }
        }
        else
        {
            // Defend is pretty worthless for a bloodthirsty AI
            score -= 10;
        }

        return score;
    }

    private static int GetPieceValue(Piece piece)
    {
        switch (piece)
        {
            case Piece.King:
                return 6;
            case Piece.Queen:
                return 9;
            case Piece.KingsBishop:
            case Piece.QueensBishop:
                return 5;
            case Piece.KingsRook:
            case Piece.QueensRook:
                return 5;
            case Piece.KingsKnight:
            case Piece.QueensKnight:
                return 3;
            case Piece.WhiteSquire:
            case Piece.GraySquire:
            case Piece.BlackSquire:
                return 2;
            default:
                return 1;
        }
    }
}


