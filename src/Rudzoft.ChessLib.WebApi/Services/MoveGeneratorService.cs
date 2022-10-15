using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.WebApi.Queries;

namespace Rudzoft.ChessLib.WebApi.Services;

public sealed class MoveGeneratorService : IMoveGeneratorService
{
    private readonly ILogger<MoveGeneratorService> _logger;

    private readonly IPosition _position;

    private readonly State _state;

    public MoveGeneratorService(
        ILogger<MoveGeneratorService> logger,
        IGame game)
    {
        _logger = logger;
        _position = game.Pos;
        _state = new State();
    }

    public IEnumerable<string> GenerateMoves(MoveQuery parameters)
    {
        _logger.LogInformation("Generating moves. fen={Fen},type={Type}", parameters.Fen, parameters.Type);

        var fd = new FenData(parameters.Fen);
        _position.Set(in fd, ChessMode.Normal, _state);
        return _position.GenerateMoves(parameters.Type).Select(static em => em.Move.ToString());
    }
}