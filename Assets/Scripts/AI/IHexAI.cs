#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IHexAI
{
    public Task<HexAIMove> GetMove(Board board);
    public void CancelMove();
    public IEnumerable<string>? GetDiagnosticInfo();
}
