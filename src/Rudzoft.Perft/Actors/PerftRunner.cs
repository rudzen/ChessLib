using Akka.Actor;
using Rudzoft.Perft.Options;

namespace Rudzoft.Perft.Actors;

public sealed class PerftRunner : ReceiveActor
{
    public PerftRunner()
    {
        Receive<EpdOptions>(epd =>
        {
            
        });
        
        Receive<FenOptions>(epd =>
        {
            
        });
    }
    
}