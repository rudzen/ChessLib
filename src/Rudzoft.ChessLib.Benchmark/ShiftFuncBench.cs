using System.Collections.Frozen;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
// ReSharper disable once ClassCanBeSealed.Global
public class ShiftFuncBench
{
    private static readonly FrozenDictionary<Direction, Func<BitBoard, BitBoard>> ShiftFuncs = MakeShiftFuncs();

    private static readonly Direction[] AllDirections =
    {
        Direction.North, Direction.South, Direction.East, Direction.West,
        Direction.NorthEast, Direction.NorthWest, Direction.SouthEast, Direction.SouthWest,
        Direction.NorthDouble, Direction.SouthDouble, Direction.SouthFill, Direction.NorthFill,
        Direction.None
    };

    [Benchmark]
    public BitBoard ShiftFuncLookup()
    {
        var bb = BitBoards.EmptyBitBoard;
        foreach (var direction in AllDirections)
            bb = ShiftF(BitBoards.Center, direction);

        return bb;
    }

    [Benchmark]
    public BitBoard ShiftFunc2Lookup()
    {
        var bb = BitBoards.EmptyBitBoard;
        foreach (var direction in AllDirections)
            bb = ShiftF2(BitBoards.Center, direction);

        return bb;
    }

    [Benchmark]
    public BitBoard BasicLookup()
    {
        var bb = BitBoards.EmptyBitBoard;
        foreach (var direction in AllDirections)
            bb = Shift(BitBoards.Center, direction);

        return bb;
    }

    private static BitBoard ShiftF(in BitBoard bb, Direction direction)
    {
        if (ShiftFuncs.TryGetValue(direction, out var func))
            return func(bb);

        throw new ArgumentException("Invalid shift argument.", nameof(direction));
    }

    private static BitBoard ShiftF2(in BitBoard bb, Direction direction)
    {
        if (ShiftFuncs.TryGetValue(direction, out var func))
            return func(bb);
        
        throw new ArgumentException("Invalid shift argument.", nameof(direction));
    }

    private static BitBoard Shift(in BitBoard bb, Direction direction)
    {
        if (direction == Direction.North)
            return bb.NorthOne();
        if (direction == Direction.South)
            return bb.SouthOne();
        if (direction == Direction.SouthEast)
            return bb.SouthEastOne();
        if (direction == Direction.SouthWest)
            return bb.SouthWestOne();
        if (direction == Direction.NorthEast)
            return bb.NorthEastOne();
        if (direction == Direction.NorthWest)
            return bb.NorthWestOne();
        if (direction == Direction.NorthDouble)
            return bb.NorthOne().NorthOne();
        if (direction == Direction.SouthDouble)
            return bb.SouthOne().SouthOne();
        if (direction == Direction.East)
            return bb.EastOne();
        if (direction == Direction.West)
            return bb.WestOne();
        if (direction == Direction.NorthFill)
            return bb.NorthFill();
        if (direction == Direction.SouthFill)
            return bb.SouthFill();
        if (direction == Direction.None)
            return bb;
        throw new ArgumentException("Invalid shift argument.", nameof(direction));
    }

    private static FrozenDictionary<Direction, Func<BitBoard, BitBoard>> MakeShiftFuncs()
    {
        return new Dictionary<Direction, Func<BitBoard, BitBoard>>(13)
        {
            { Direction.None, static board => board },
            { Direction.North, static board => board.NorthOne() },
            { Direction.NorthDouble, static board => board.NorthOne().NorthOne() },
            { Direction.NorthEast, static board => board.NorthEastOne() },
            { Direction.NorthWest, static board => board.NorthWestOne() },
            { Direction.NorthFill, static board => board.NorthFill() },
            { Direction.South, static board => board.SouthOne() },
            { Direction.SouthDouble, static board => board.SouthOne().SouthOne() },
            { Direction.SouthEast, static board => board.SouthEastOne() },
            { Direction.SouthWest, static board => board.SouthWestOne() },
            { Direction.SouthFill, static board => board.SouthFill() },
            { Direction.East, static board => board.EastOne() },
            { Direction.West, static board => board.WestOne() }
        }.ToFrozenDictionary();
    }
}