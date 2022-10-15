using Microsoft.AspNetCore.Mvc;
using Rudzoft.ChessLib.WebApi.Queries;
using Rudzoft.ChessLib.WebApi.Services;

namespace Rudzoft.ChessLib.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MovesController : ControllerBase
{
    private readonly ILogger<MovesController> _logger;
    private readonly IMoveGeneratorService _moveGeneratorService;

    public MovesController(ILogger<MovesController> logger, IMoveGeneratorService moveGeneratorService)
    {
        _logger = logger;
        _moveGeneratorService = moveGeneratorService;
    }

    [HttpGet(Name = "generate")]
    public IActionResult Get([FromQuery] MoveQuery parameters)
    {
        var m = _moveGeneratorService.GenerateMoves(parameters).ToList();
        _logger.LogInformation("Moves fetched. size={Count}", m.Count);
        return Ok(new Moves(m, m.Count));
    }
}