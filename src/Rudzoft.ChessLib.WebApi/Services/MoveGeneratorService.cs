using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.WebApi.Queries;

namespace Rudzoft.ChessLib.WebApi.Services;

public sealed class MoveGeneratorService(ILogger<MoveGeneratorService> logger, IPosition pos) : IMoveGeneratorService
{
    private readonly ILogger<IMoveGeneratorService> _logger = logger;

    public IEnumerable<string> GenerateMoves(MoveQuery parameters)
    {
        _logger.LogInformation("Generating moves. fen={Fen},type={Type},mode={Mode}", parameters.Fen, parameters.Types, parameters.Mode);

        var state = new State();
        pos.Set(parameters.Fen, parameters.Mode, state);

        var ml = pos.GenerateMoves();
        ml.Generate(in pos);
        return ml.GetMoves().Select(static m => m.ToString());
    }
}