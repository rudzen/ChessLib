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

using System.Numerics;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Extensions;

namespace Rudzoft.ChessLib.Types;

public enum Colors
{
    White = 0,
    Black = 1,
    PlayerNb = 2
}

public enum PlayerTypes
{
    Engine = 0,
    Human = 1
}

public readonly record struct Color(byte Side) : ISpanFormattable, IMinMaxValue<Color>
{
    private static readonly Direction[] PawnPushDist = [Direction.North, Direction.South];
    private static readonly Direction[] PawnDoublePushDist = [Direction.NorthDouble, Direction.SouthDouble];
    private static readonly Direction[] PawnWestAttackDist = [Direction.NorthEast, Direction.SouthEast];
    private static readonly Direction[] PawnEastAttackDist = [Direction.NorthWest, Direction.SouthWest];

    private static readonly string[] PlayerColors = ["White", "Black"];
    private static readonly char[] PlayerFen = ['w', 'b'];
    private static readonly Func<BitBoard, BitBoard>[] PawnPushModifiers = [BitBoards.NorthOne, BitBoards.SouthOne];
    private static readonly int[] ScoreSign = [1, -1];

    public Color(Color c)
        : this(c.Side) { }

    public Color(Colors c)
        : this((byte)c) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out byte side) => side = Side;

    public        bool     IsWhite    => Side == byte.MinValue;
    public        bool     IsBlack    => Side != byte.MinValue;
    public        int      Sign       => ScoreSign[Side];
    public        char     Fen        => PlayerFen[Side];
    public static Color   White      { get; } = new(Colors.White);
    public static Color   Black      { get; } = new(Colors.Black);
    public static Color[] AllColors { get; } = [White, Black];
    public static Color   MaxValue   => White;
    public static Color   MinValue   => Black;

    public const int Count = (int)Colors.PlayerNb;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(int value) => new((byte)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(byte value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(uint value) => new((byte)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(Colors c) => new(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(bool value) => new(value.AsByte());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Create(Colors c) => new(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator ~(Color c) => new(c.Side ^ 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator <<(Color left, int right) => left.Side << right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator >>(Color left, int right) => left.Side >> right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator +(PieceType pt, Color c) => new((Pieces)(pt.Value + (byte)(c << 3)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator byte(Color c) => c.Side;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Color c) => Side == c.Side;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => Side << 24;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => PlayerColors[Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, IFormatProvider formatProvider) => ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        destination[0] = Fen;
        charsWritten = 1;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOk() => Side.IsBetween(White.Side, Black.Side);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetName() => PlayerColors[Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Direction PawnPushDistance() => PawnPushDist[Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Direction PawnDoublePushDistance() => PawnDoublePushDist[Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Direction PawnWestAttackDistance() => PawnWestAttackDist[Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Direction PawnEastAttackDistance() => PawnEastAttackDist[Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard PawnPush(in BitBoard bb) => PawnPushModifiers[Side](bb);
}
