using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables.Transposition;

public struct PerftEntry
{
    public HashKey Hash;
    public UInt128 Nodes;
    public byte Depth;
    public byte Generation;
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

    private ushort _generation;

    public PertT(ulong size)
    {
        Resize(in size, Unsafe.SizeOf<PerftEntry>());
    }

    public (UInt128, bool) Get(in HashKey hash, int depth) {
        var index = (int)(hash.Key % Size) * NumTTBuckets;
        for (var i = 0; i < NumTTBuckets; i++)
        {
            ref var entry = ref Entries[index + i];
            if (entry.Hash == hash && entry.Depth == depth && entry.Generation == _generation)
                return (entry.Nodes, true);
        }
        return (default, false);
    }

    public void Set(in HashKey hash, byte depth, in UInt128 nodes)
    {
        var index = (int)(hash.Key % Size) * NumTTBuckets;
        for (var i = 0; i < NumTTBuckets; i++)
        {
            ref var entry = ref Entries[index + i];
            if (entry.Hash == hash && entry.Depth == depth)
            {
                entry.Nodes = nodes;
                entry.Generation = (byte)_generation;
                return;
            }
        }
        for (var i = 0; i < NumTTBuckets; i++)
        {
            ref var entry = ref Entries[index + i];
            if (entry.Generation != _generation)
            {
                entry.Hash = hash;
                entry.Depth = depth;
                entry.Nodes = nodes;
                entry.Generation = (byte)_generation;
                return;
            }
        }
        var replaceIdx = index;
        if (Entries[index + 1].Depth < Entries[index].Depth)
            replaceIdx = index + 1;
        ref var replaceEntry = ref Entries[replaceIdx];
        replaceEntry.Hash = hash;
        replaceEntry.Depth = depth;
        replaceEntry.Nodes = nodes;
        replaceEntry.Generation = (byte)_generation;
    }

    public void Resize(in ulong sizeInMB, int entrySize)
    {
        Size = (sizeInMB * 1024 * 1024) / (ulong)entrySize;
        Entries = new PerftEntry[Size * NumTTBuckets];
    }

    public ref PerftEntry Probe(in HashKey hash)
    {
        var index = (int)(hash.Key % Size) * NumTTBuckets;
        return ref Entries[index];
    }

    public ref PerftEntry Store(in HashKey hash, byte depth, byte currAge)
    {
        var index = (int)(hash.Key % Size) * NumTTBuckets;
        return ref Entries[index];
    }

    public void NewSearch() => _generation++;

    public void Uninitialize()
    {
        Entries = null;
        Size = 0;
    }

    public void Clear()
    {
        _generation++;
        if (_generation == 0)
        {
            for (ulong idx = 0; idx < Size * NumTTBuckets; idx++)
                Entries[idx] = default;
            _generation = 1;
        }
    }
}