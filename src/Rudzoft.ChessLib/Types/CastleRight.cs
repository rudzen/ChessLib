/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Runtime.CompilerServices;

namespace Rudzoft.ChessLib.Types;

[Flags]
public enum CastleRights : byte
{
    None = 0,                    // 0000
    WhiteKing = 1,               // 0001
    WhiteQueen = WhiteKing << 1, // 0010
    BlackKing = WhiteKing << 2,  // 0100
    BlackQueen = WhiteKing << 3, // 1000

    King = WhiteKing | BlackKing,    // 0101
    Queen = WhiteQueen | BlackQueen, // 1010
    White = WhiteKing | WhiteQueen,  // 0011
    Black = BlackKing | BlackQueen,  // 1100
    Any = White | Black,             // 1111

    Count = 16
}

public enum CastleSides : byte
{
    King = 0,
    Queen = 1,
    Center = 2
}

public enum CastlePerform
{
    Do,
    Undo
}

public static class CastleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetCastleString(Square toSquare, Square fromSquare) => toSquare < fromSquare ? "O-O-O" : "O-O";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsInt(this CastleRights value) => (int)value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRights Without(this CastleRights @this, CastleRights remove) => @this & ~remove;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRights MakeCastleRights(this CastleRights cs, Color c)
    {
        var isQueen = cs == CastleRights.Queen;
        if (c.IsWhite)
            return isQueen
                ? CastleRights.WhiteQueen
                : CastleRights.WhiteKing;
        return isQueen
            ? CastleRights.BlackQueen
            : CastleRights.BlackKing;
    }
}

public readonly record struct CastleSide(CastleSides Sides)
{
    public static CastleSide King { get; } = new(CastleSides.King);
    public static CastleSide Queen { get; } = new(CastleSides.Queen);
    public static CastleSide Center { get; } = new(CastleSides.Center);

    public static int Count => 3;

    public BitBoard SlotFile() => BitBoards.SlotFile(this);

    public static CastleSide[] AllCastleSides =>
    [
        King, Queen, Center
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(CastleSide left, CastleSides right) => left.Sides == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(CastleSide left, CastleSides right) => left.Sides != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator byte(CastleSide r) => (byte)r.Sides;
}

public readonly record struct CastleRight(CastleRights Rights)
{
    private CastleRight(int cr) : this((CastleRights)cr) { }

    public bool IsNone => Rights == CastleRights.None;

    public static CastleRight None { get; } = new(CastleRights.None);
    public static CastleRight WhiteKing { get; } = new(CastleRights.WhiteKing);
    public static CastleRight BlackKing { get; } = new(CastleRights.BlackKing);
    public static CastleRight WhiteQueen { get; } = new(CastleRights.WhiteQueen);
    public static CastleRight BlackQueen { get; } = new(CastleRights.BlackQueen);
    public static CastleRight King { get; } = new(CastleRights.King);
    public static CastleRight Queen { get; } = new(CastleRights.Queen);
    public static CastleRight White { get; } = new(CastleRights.White);
    public static CastleRight Black { get; } = new(CastleRights.Black);
    public static CastleRight Any { get; } = new(CastleRights.Any);

    public static CastleRight[] AllCastleRight =
    [
        None, WhiteKing, BlackKing, WhiteQueen, BlackQueen, King, Queen, White, Black, Any
    ];

    public const int Count = (int)CastleRights.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator CastleRight(CastleRights cr) => new(cr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator CastleRight(Color c) => Create(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRight Create(Color c) => new((CastleRights)((int)CastleRights.White << (c << 1)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(CastleRight other) => Rights == other.Rights;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => (int)Rights;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(CastleRight cr) => cr.Rights != CastleRights.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(CastleRight cr) => cr.Rights == CastleRights.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRight operator |(CastleRight cr1, CastleRight cr2) => new(cr1.Rights | cr2.Rights);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRight operator |(CastleRight cr1, CastleRights cr2) => new(cr1.Rights | cr2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRight operator ^(CastleRight cr1, CastleRight cr2) => new(cr1.Rights ^ cr2.Rights);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRight operator ^(CastleRight cr1, CastleRights cr2) => new(cr1.Rights ^ cr2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRight operator &(CastleRight cr1, CastleRight cr2) => new(cr1.Rights & cr2.Rights);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRight operator &(CastleRight cr1, CastleRights cr2) => new(cr1.Rights & cr2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastleRight operator ~(CastleRight cr) => new(~cr.Rights);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(CastleRights cr) => (Rights & cr) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(CastleRight cr) => (Rights & cr.Rights) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CastleRight Not(CastleRights cr) => new(Rights & ~cr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt() => Rights.AsInt();
}