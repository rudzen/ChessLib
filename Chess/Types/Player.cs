/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

using System;
using System.Runtime.CompilerServices;
using Rudz.Chess.Extensions;

namespace Rudz.Chess.Types;

public enum Players
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

public readonly struct Player : IEquatable<Player>
{
    private static readonly Direction[] PawnPushDist = { Direction.North, Direction.South };

    private static readonly Direction[] PawnDoublePushDist = { Direction.NorthDouble, Direction.SouthDouble };

    private static readonly Direction[] PawnWestAttackDist = { Direction.NorthEast, Direction.SouthEast };

    private static readonly Direction[] PawnEastAttackDist = { Direction.NorthWest, Direction.SouthWest };

    private static readonly string[] PlayerColors = { "White", "Black" };

    private static readonly char[] PlayerFen = { 'w', 'b' };

    private static readonly Func<BitBoard, BitBoard>[] PawnPushModifiers = { BitBoards.NorthOne, BitBoards.SouthOne };

    public Player(byte side)
        : this() => Side = side;

    public Player(Player side)
        : this() => Side = side.Side;

    public Player(Players side)
        : this((byte)side) { }

    public readonly byte Side;

    public bool IsWhite => Side == 0;

    public bool IsBlack => Side != 0;

    public char Fen => PlayerFen[Side];

    public static Player White { get; } = new(Players.White);

    public static Player Black { get; } = new(Players.Black);

    public const int Count = (int)Players.PlayerNb;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Player(int value) => new((byte)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Player(uint value) => new((byte)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Player(Players value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Player(bool value) => new(value.AsByte());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Player operator ~(Player player) => new(player.Side ^ 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Player left, Player right) => left.Side == right.Side;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Player left, Player right) => left.Side != right.Side;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator <<(Player left, int right) => left.Side << right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator >>(Player left, int right) => left.Side >> right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Pieces operator +(PieceTypes pieceType, Player side) => (Pieces)pieceType + (byte)(side.Side << 3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is Player player && Equals(player);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Player other) => Side == other.Side;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => Side << 24;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => PlayerColors[Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOk() => Side.InBetween(White.Side, Black.Side);

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
    public BitBoard PawnPush(BitBoard bitBoard) => PawnPushModifiers[Side](bitBoard);
}
