using Akka.Actor;

namespace Rudzoft.Perft.Actors;

/// <summary>
/// Generate PerftPositions from input.
/// Both FEN and (basic) EPD are supported.
/// </summary>
public sealed class PositionParserActor : ReceiveActor
{
    public PositionParserActor(IServiceProvider sp)
    {

    }
}