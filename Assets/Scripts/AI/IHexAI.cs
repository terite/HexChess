using System.Threading.Tasks;

public interface IHexAI
{
    public Task<HexAIMove> GetMove(Board board);
    public void CancelMove();
}
