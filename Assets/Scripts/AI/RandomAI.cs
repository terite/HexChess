using System;
using System.Linq;

public class RandomAI : IHexAI
{
    Random random = new Random();
    public HexAIMove GetMove(Board board)
    {
        var allmoves = HexAIMove.GenerateAllValidMoves(board).ToArray();
        return allmoves[random.Next(0, allmoves.Length)];
    }
}
