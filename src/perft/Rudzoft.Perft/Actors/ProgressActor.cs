using System.Runtime.InteropServices;
using Akka.Actor;

namespace Rudzoft.Perft.Actors;

public sealed class ProgressActor : ReceiveActor, IWithTimers, IWithStash
{
    private sealed class ProgressInternal
    {
        public string Fen { get; init; }
        public int CurrentDepth { get; set; }
        public int MaxDepth { get; }
        public TimeSpan StartTime { get; }
    }

    public sealed record PerftProgress(string Fen, int Depth);

    private readonly Dictionary<string, ProgressInternal> _progress = new();

    public ITimerScheduler Timers { get; set; }
    public IStash Stash { get; set; }

    public ProgressActor()
    {
        Receive<PerftProgress>(progress =>
        {
            ref var existing = ref CollectionsMarshal.GetValueRefOrAddDefault(_progress, progress.Fen, out var exists);

            if (!exists)
            {
                existing = new()
                {
                    CurrentDepth = progress.Depth
                };
            }
            else
                existing = new();
        });
    }
}