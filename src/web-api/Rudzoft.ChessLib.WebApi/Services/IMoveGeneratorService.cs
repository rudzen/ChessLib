using Rudzoft.ChessLib.WebApi.Queries;

namespace Rudzoft.ChessLib.WebApi.Services;

public interface IMoveGeneratorService
{
    IEnumerable<string> GenerateMoves(MoveQuery parameters);
}