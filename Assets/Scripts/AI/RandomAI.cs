using System;
using System.Linq;

public class RandomAI : IHexAI
{
    Random random = new Random();
    public HexAIMove GetMove(Game game)
    {
        var allmoves = HexAIMove.GenerateAllValidMoves(game).ToArray();
        return allmoves[random.Next(0, allmoves.Length)];
    }
}