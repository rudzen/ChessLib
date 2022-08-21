namespace Rudz.Chess.Protocol.UCI;

public struct Clock
{
    public Clock()
    {
    }

    public ulong[] Inc { get; } = new ulong[2];

    public ulong[] Time { get; } = new ulong[2];
}