using System.Collections;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BoardTests
{
    [UnityTest]
    public IEnumerator KingOnlyTest()
    {
        SceneManager.LoadScene("Scenes/SandboxMode");
        yield return null;
        var board = GameObject.FindObjectOfType<Board>();
        Assert.AreEqual(38, board.activePieces.Count);

        var piecePositions = new BidirectionalDictionary<(Team, Piece), Index>();
        piecePositions[(Team.White, Piece.King)] = new Index(5, 'E');
        board.SetBoardState(new BoardState()
        {
            allPiecePositions = piecePositions,
            check = Team.None,
            checkmate = Team.None,
            currentMove = Team.White,
            executedAtTime = 1,
        });

        Assert.AreEqual(1, board.activePieces.Count);
        var kingMoves = board.activePieces[(Team.White, Piece.King)]
            .GetAllPossibleMoves(board, board.GetCurrentBoardState()).Select(m => (m.Item1.index, m.Item2))
            .ToArray();

        Assert.AreEqual(6, kingMoves.Length);
        // ORDER {Up, UpRight, DownRight, Down, DownLeft, UpLeft};
        Assert.AreEqual((new Index(6, 'E'), MoveType.Move), kingMoves[0]);
        Assert.AreEqual((new Index(6, 'F'), MoveType.Move), kingMoves[1]);
        Assert.AreEqual((new Index(5, 'F'), MoveType.Move), kingMoves[2]);
        Assert.AreEqual((new Index(4, 'E'), MoveType.Move), kingMoves[3]);
        Assert.AreEqual((new Index(5, 'D'), MoveType.Move), kingMoves[4]);
        Assert.AreEqual((new Index(6, 'D'), MoveType.Move), kingMoves[5]);
    }
    [UnityTest]
    public IEnumerator KingCheckDetectionTest()
    {
        SceneManager.LoadScene("Scenes/SandboxMode");
        yield return null;
        var board = GameObject.FindObjectOfType<Board>();
        Assert.AreEqual(38, board.activePieces.Count);

        var piecePositions = new BidirectionalDictionary<(Team, Piece), Index>();
        piecePositions[(Team.Black, Piece.King)] = new Index(5, 'E');
        piecePositions[(Team.White, Piece.KingsRook)] = new Index(1, 'E');
        piecePositions[(Team.White, Piece.King)] = new Index(1, 'A');
        var newState = new BoardState()
        {
            allPiecePositions = piecePositions,
            check = Team.None,
            checkmate = Team.None,
            currentMove = Team.Black,
            executedAtTime = 1,
        };
        board.SetBoardState(newState);
        board.turnHistory.Add(newState);
        // board.AdvanceTurn(newState, false);

        Assert.True(board.IsChecking(board.GetCurrentBoardState(), Team.White));
        Assert.False(board.IsChecking(board.GetCurrentBoardState(), Team.Black));

    }
}
