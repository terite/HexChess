using System;
using System.Collections.Generic;
using System.Diagnostics;

public class TeriteAI : IHexAI
{
    public readonly Stopwatch moveGenTimer = new Stopwatch();
    public readonly Stopwatch moveSortTimer = new Stopwatch();
    public readonly Stopwatch moveValidateTimer = new Stopwatch();
    public readonly Stopwatch quiescenceTimer = new Stopwatch();
    public readonly Stopwatch evalTimer = new Stopwatch();
    public readonly Stopwatch evalThreatsTimer = new Stopwatch();
    public readonly Stopwatch applyTimer = new Stopwatch();
    public readonly Stopwatch getMoveTimer = new Stopwatch();

    public int boardEvaluations = 0;
    public int terminalBoardEvaluations = 0;
    public int invalidMoves = 0;

    readonly int maxSearchDepth;

    readonly List<FastMove>[] moveCache;

    public TeriteAI() : this(4)
    {
    }

    public TeriteAI(int maxSearchDepth = 4)
    {
        this.maxSearchDepth = maxSearchDepth;
        moveCache = new List<FastMove>[this.maxSearchDepth + 1 /* root */];

        for (int i = 0; i < moveCache.Length; i++)
            moveCache[i] = new List<FastMove>(100);
    }
    public HexAIMove GetMove(Game game)
    {
        var root = new FastBoardNode(game);
        return GetMove(root).ToHexMove();
    }

    public FastMove GetMove(FastBoardNode root)
    {
        moveGenTimer.Reset();
        moveSortTimer.Reset();
        moveValidateTimer.Reset();
        evalTimer.Reset();
        evalThreatsTimer.Reset();
        applyTimer.Reset();
        getMoveTimer.Restart();
        boardEvaluations = 0;
        invalidMoves = 0;

        int color = (root.currentMove == Team.White) ? 1 : -1;

        int bestMoveValue = -CheckmateValue * 3;
        FastMove bestMove = FastMove.Invalid;

        for (int searchDepth = 1; searchDepth <= maxSearchDepth; searchDepth++)
        {
            int alpha = -CheckmateValue * 2; // Best move for current player
            int beta = CheckmateValue * 2; // Best move our opponent will let us have

            (int moveValue, FastMove move) = Search(root, searchDepth, 0, alpha, beta, color);

            bestMoveValue = moveValue;
            bestMove = move;

            if (moveValue >= (CheckmateValue - searchDepth))
            {
                return bestMove;
            }
        }

        return bestMove;
    }

    (int value, FastMove move) Search(FastBoardNode node, int searchDepth, int plyFromRoot, int alpha, int beta, int color)
    {
        if (searchDepth == 0)
        {
            quiescenceTimer.Start();
            var res = (QuiescenceSearch(node, plyFromRoot, alpha, beta, color), FastMove.Invalid);
            quiescenceTimer.Stop();
            return res;
        }

        var moves = moveCache[searchDepth];
        moves.Clear();
        moveGenTimer.Start();
        node.AddAllPossibleMoves(moves, node.currentMove);
        moveGenTimer.Stop();

        moveSortTimer.Start();
        OrderMoves(node, moves);
        moveSortTimer.Stop();

        bool isTerminal = true;
        int value = int.MinValue;
        FastMove bestMove = FastMove.Invalid;

        foreach (var move in moves)
        {
            applyTimer.Start();
            node.DoMove(move);
            applyTimer.Stop();

            moveValidateTimer.Start();
            bool isKingVulnerable = node.IsChecking(node.currentMove);
            moveValidateTimer.Stop();
            if (isKingVulnerable)
            {
                invalidMoves++;
                applyTimer.Start();
                node.UndoMove(move);
                applyTimer.Stop();
                continue;
            }


            isTerminal = false;
            (int currentValue, FastMove _) = Search(node, searchDepth - 1, plyFromRoot + 1, -beta, -alpha, -color);

            applyTimer.Start();
            node.UndoMove(move);
            applyTimer.Stop();

            currentValue = -currentValue;
            if (currentValue > value)
            {
                bestMove = move;
                value = currentValue;
            }
            alpha = Math.Max(alpha, value);
            if (alpha >= beta)
                break;
        }

        if (isTerminal)
        {
            value = color * EvaluateTerminalBoard(node, plyFromRoot);
        }

        return (value, bestMove);
    }

    int QuiescenceSearch(FastBoardNode node, int plyFromRoot, int alpha, int beta, int color)
    {
        var moves = new List<FastMove>(10);
        moveGenTimer.Start();
        node.AddAllPossibleMoves(moves, node.currentMove, generateQuiet: false);
        moveGenTimer.Stop();

        moveSortTimer.Start();
        OrderMoves(node, moves);
        moveSortTimer.Stop();

        bool maybeTerminal = true;
        int value = int.MinValue;

        foreach (var move in moves)
        {
            applyTimer.Start();
            node.DoMove(move);
            applyTimer.Stop();

            moveValidateTimer.Start();
            bool isKingVulnerable = node.IsChecking(node.currentMove);
            moveValidateTimer.Stop();
            if (isKingVulnerable)
            {
                invalidMoves++;
                applyTimer.Start();
                node.UndoMove(move);
                applyTimer.Stop();
                continue;
            }

            maybeTerminal = false;
            int currentValue = QuiescenceSearch(node, plyFromRoot + 1, -beta, -alpha, -color);

            applyTimer.Start();
            node.UndoMove(move);
            applyTimer.Stop();

            currentValue = -currentValue;
            if (currentValue > value)
            {
                value = currentValue;
            }
            alpha = Math.Max(alpha, value);
            if (alpha >= beta)
                break;
        }

        if (maybeTerminal)
        {
            return color * EvaluateMaybeTerminalBoard(node, plyFromRoot);
        }

        return value;
    }

    private void OrderMoves(FastBoardNode node, List<FastMove> moves)
    {
        moves.Sort((FastMove a, FastMove b) =>
        {
            int valueA = MoveValuer(node, a);
            int valueB = MoveValuer(node, b);

            return valueB - valueA; // descending
        });
    }

    private static int MoveValuer(FastBoardNode node, FastMove move)
    {
        int value = 0;
        if (move.moveType == MoveType.Move)
        {
            var mover = node[move.start];
            if (mover.piece == FastPiece.Pawn)
                value += 1;

            if (mover.piece == FastPiece.King)
                value -= 1;
        }
        else if (move.moveType == MoveType.Attack)
        {
            var attacker = node[move.start];
            var victim = node[move.target];

            int attackValue = GetPieceValue(victim.piece) - GetPieceValue(attacker.piece);

            value += attackValue;
        }
        else if (move.moveType == MoveType.EnPassant)
        {
            value += 5;
        }

        return value;
    }

    #region Evaluation

    const int CheckBonusValue = 10;
    const int CheckmateValue = 10000;
    const int DrawValue = 0;

    static readonly int[] TeamMults = new[] { 0, 1, -1 };

    public int EvaluateTerminalBoard(FastBoardNode node, int plyFromRoot)
    {
        terminalBoardEvaluations++;
        bool whiteIsChecking = node.currentMove != Team.White && node.IsChecking(Team.White);
        if (whiteIsChecking)
        {
            return CheckmateValue - plyFromRoot;
        }

        bool blackIsChecking = node.currentMove != Team.Black && node.IsChecking(Team.Black);
        if (blackIsChecking)
        {
            return -CheckmateValue + plyFromRoot;
        }

        // Either stalemate, or 50 move rule draw
        return DrawValue;
    }
    public int EvaluateBoard(FastBoardNode node, int plyFromRoot)
    {
        return EvaluateMaybeTerminalBoard(node, plyFromRoot);
    }

    public int EvaluateMaybeTerminalBoard(FastBoardNode node, int plyFromRoot)
    {
        boardEvaluations++;

        evalTimer.Start();
        try
        {
            if (node.PlySincePawnMovedOrPieceTaken >= 100)
                return 0; // automatic draw due to 50 move rule.

            int boardValue = 0;
            bool whiteIsChecking = node.currentMove != Team.White && node.IsChecking(Team.White);
            if (whiteIsChecking)
            {
                boardValue += CheckBonusValue;
            }

            bool blackIsChecking = node.currentMove != Team.Black && node.IsChecking(Team.Black);
            if (blackIsChecking)
            {
                boardValue -= CheckBonusValue;
            }

            if (!node.HasAnyValidMoves(node.currentMove))
            {
                if (whiteIsChecking)
                    return CheckmateValue - plyFromRoot;
                else if (blackIsChecking)
                    return -CheckmateValue + plyFromRoot;
                else
                    return DrawValue;
            }

            for (byte i = 0; i < node.positions.Length; i++)
            {
                var piece = node.positions[i];
                if (piece.team == Team.None)
                    continue;

                int pieceValue = GetPieceValue(piece.piece);

                int rank = (i / 9) + 1;
                if (piece.team != Team.White)
                    rank = 9 - rank;

                if (piece.piece == FastPiece.Pawn)
                {
                    pieceValue += rank * 2;
                }
                else if (piece.piece != FastPiece.King)
                {
                    pieceValue += rank;
                }

                boardValue += TeamMults[(byte)piece.team] * pieceValue;
            }

            evalThreatsTimer.Start();
            BitsBoard whiteThreats;
            BitsBoard blackThreats;
            BitsBoard whitePawnThreats;
            BitsBoard blackPawnThreats;
            EvaluateThreats(node, out whiteThreats, out blackThreats, out whitePawnThreats, out blackPawnThreats);

            boardValue += whiteThreats.Count - blackThreats.Count;
            boardValue += (whitePawnThreats.Count * 2) - (blackPawnThreats.Count * 2);

            evalThreatsTimer.Stop();

            return boardValue;
        }
        finally {
            evalTimer.Stop();
        }
    }

    static int GetPieceValue(FastPiece piece)
    {
        switch (piece)
        {
            case FastPiece.King:
                return 900;

            case FastPiece.Queen:
                return 90;

            case FastPiece.Rook:
                return 40;

            case FastPiece.Knight:
                return 30;

            case FastPiece.Bishop:
                return 50;

            case FastPiece.Squire:
                return 20;

            case FastPiece.Pawn:
            default:
                return 10;
        }
    }

    static readonly List<FastMove> threatMoveCache = new List<FastMove>();
    static void EvaluateThreats(FastBoardNode node, out BitsBoard whiteThreats, out BitsBoard blackThreats, out BitsBoard whitePawnThreats, out BitsBoard blackPawnThreats)
    {
        whiteThreats = new BitsBoard();
        blackThreats = new BitsBoard();
        whitePawnThreats = new BitsBoard();
        blackPawnThreats = new BitsBoard();

        var moves = threatMoveCache;
        moves.Clear();
        for (byte b = 0; b < node.positions.Length; ++b)
        {
            var piece = node.positions[b];
            if (piece.team == Team.None)
                continue;

            var index = FastIndex.FromByte(b);
            FastPossibleMoveGenerator.AddAllPossibleMoves(moves, index, piece.piece, piece.team, node, generateQuiet: false);

            BitsBoard threats = default;
            foreach (var move in moves)
            {
                if (move.moveType == MoveType.Attack)
                    threats[move.target.ToByte()] = true;
            }

            if (piece.team == Team.White)
            {
                whiteThreats |= threats;
                if (piece.piece == FastPiece.Pawn)
                    whitePawnThreats |= threats;
            }
            else if (piece.team == Team.Black)
            {
                blackThreats |= threats;
                if (piece.piece == FastPiece.Pawn)
                    blackPawnThreats |= threats;
            }
        }
    }

    #endregion
}
