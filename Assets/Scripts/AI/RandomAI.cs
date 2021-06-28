using System;
using System.Linq;
using System.Threading.Tasks;

public class RandomAI : IHexAI
{
    Random random = new Random();
    public Task<HexAIMove> GetMove(Board board)
    {
        var allmoves = HexAIMove.GenerateAllValidMoves(board).ToArray();
        return Task.FromResult(allmoves[random.Next(0, allmoves.Length)]);
    }

    public void CancelMove() { }
}
