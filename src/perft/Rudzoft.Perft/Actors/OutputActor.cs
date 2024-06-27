using Akka.Actor;
using Serilog;

namespace Rudzoft.Perft.Actors;

public sealed class OutputActor : UntypedActor
{
    private static readonly ILogger Log = Serilog.Log.ForContext<OutputActor>();

    public sealed record Output(string Out);



    protected override void OnReceive(object message)
    {
        if (message is Output output)
        {
            Console.WriteLine(output.Out);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}