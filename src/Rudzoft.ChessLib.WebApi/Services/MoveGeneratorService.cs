using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.WebApi.Queries;

namespace Rudzoft.ChessLib.WebApi.Services;

public sealed class MoveGeneratorService : IMoveGeneratorService
{
    private readonly ILogger<IMoveGeneratorService> _logger;
    private readonly IPosition _position;

    public MoveGeneratorService(ILogger<IMoveGeneratorService> logger, IPosition pos)
    {
        _logger = logger;
        _position = pos;
    }

    public IEnumerable<string> GenerateMoves(MoveQuery parameters)
    {
        _logger.LogInformation("Generating moves. fen={Fen},type={Type},mode={Mode}", parameters.Fen, parameters.Type, parameters.Mode);

        var fd = new FenData(parameters.Fen);
        var state = new State();
        _position.Set(in fd, parameters.Mode, state);
        return _position.GenerateMoves(parameters.Type).Select(static em => em.Move.ToString());
    }
}