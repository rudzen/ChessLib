using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables.Transposition;

public struct PerftEntry
{
    public HashKey Hash;
    public UInt128 Nodes;
    public byte Depth;

    public HashKey GetHash() => Hash;
    public byte GetAge() => 1; // Age doesn't matter when considering perft transposition table entries
    public byte GetDepth() => Depth;
}

public sealed class PertT
{
    public const int DefaultTTSize = 64;
    public const int NumTTBuckets = 2;
    public const ulong SearchEntrySize = 16;
    public const ulong PerftEntrySize = 24;
    public const byte AlphaFlag = 1;
    public const byte BetaFlag = 2;
    public const byte ExactFlag = 3;
    public const int Checkmate = 9000;

    public PerftEntry[] Entries;
    public ulong Size;

    public PertT(ulong size)
    {
        Resize(in size, Unsafe.SizeOf<PerftEntry>());
    }

    public (UInt128, bool) Get(in HashKey hash, byte depth) {
        ref var entry = ref Probe(hash);
        if (entry.Hash == hash && entry.Depth == depth) {
            return (entry.Nodes, true);
        }
        return (default, false);
    }

    public void Set(in HashKey hash, byte depth, in ulong nodes)
    {
        ref var entry = ref Probe(in hash);
        entry.Hash = hash.Key;
        entry.Depth = depth;
        entry.Nodes = nodes;
    }

    public void Resize(in ulong sizeInMB, int entrySize)
    {
        Size = (sizeInMB * 1024 * 1024) / (ulong)entrySize;
        Entries = new PerftEntry[Size];
    }

    public ref PerftEntry Probe(in HashKey hash)
    {
        var index = hash.Key % Size;
        return ref Entries[index];
    }

    public ref PerftEntry Store(in ulong hash, byte depth, byte currAge)
    {
        var index = hash % Size;
        return ref Entries[index];
    }

    public void Uninitialize()
    {
        Entries = null;
        Size = 0;
    }

    public void Clear()
    {
        for (ulong idx = 0; idx < Size; idx++)
            Entries[idx] = default;
    }
}