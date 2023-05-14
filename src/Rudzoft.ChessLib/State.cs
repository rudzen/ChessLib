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

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

// ReSharper disable MemberCanBeInternal
namespace Rudzoft.ChessLib;

public sealed class State : IEquatable<State>
{
    public HashKey MaterialKey { get; set; }

    public HashKey PawnKey { get; set; }

    public int Rule50 { get; set; }

    public int PliesFromNull { get; set; }

    public CastleRight CastlelingRights { get; set; }

    public Square EnPassantSquare { get; set; }


    // -----------------------------
    // Properties below this point are not copied from other state
    // since they are always recomputed
    // -----------------------------

    public HashKey PositionKey { get; set; }

    /// <summary>
    /// Represents checked squares for side to move
    /// </summary>
    public BitBoard Checkers { get; set; }

    public BitBoard[] BlockersForKing { get; }

    public BitBoard[] Pinners { get; }

    public BitBoard[] CheckedSquares { get; private set; }

    public PieceTypes CapturedPiece { get; set; }

    public Move LastMove { get; set; }

    public int Repetition { get; set; }

    public State Previous { get; private set; }

    /// <summary>
    /// Partial copy from existing state The properties not copied are re-calculated
    /// </summary>
    /// <param name="other">The current state</param>
    public State(State other)
    {
        PawnKey = other.PawnKey;
        MaterialKey = other.MaterialKey;
        CastlelingRights = other.CastlelingRights;
        Rule50 = other.Rule50;
        PliesFromNull = other.PliesFromNull;
        EnPassantSquare = other.EnPassantSquare;
        Previous = other;

        Checkers = BitBoard.Empty;
        BlockersForKing = new[] { BitBoard.Empty, BitBoard.Empty };
        Pinners = new[] { BitBoard.Empty, BitBoard.Empty };
        CheckedSquares = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
        CapturedPiece = PieceTypes.NoPieceType;
    }

    public State()
    {
        LastMove = Move.EmptyMove;
        CastlelingRights = CastleRight.None;
        EnPassantSquare = Square.None;
        Checkers = BitBoard.Empty;
        CheckedSquares = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
        Pinners = new[] { BitBoard.Empty, BitBoard.Empty };
        BlockersForKing = new[] { BitBoard.Empty, BitBoard.Empty };
        CapturedPiece = PieceTypes.NoPieceType;
    }

    public State CopyTo(State other)
    {
        other ??= new State();

        // copy over preserved values
        other.MaterialKey = MaterialKey;
        other.PawnKey = PawnKey;
        other.Rule50 = Rule50;
        other.PliesFromNull = PliesFromNull;
        other.CastlelingRights = CastlelingRights;
        other.EnPassantSquare = EnPassantSquare;
        other.Previous = this;

        // initialize the rest of the values
        other.CheckedSquares ??= new BitBoard[PieceTypes.PieceTypeNb.AsInt()];

        return other;
    }

    public void Clear()
    {
        LastMove = Move.EmptyMove;
        PawnKey = PositionKey = MaterialKey = HashKey.Empty;
        PliesFromNull = 0;
        Repetition = 0;
        CastlelingRights = CastleRight.None;
        EnPassantSquare = Square.None;
        CheckedSquares.Fill(BitBoard.Empty);
        Pinners[0] = Pinners[1] = BitBoard.Empty;
        BlockersForKing[0] = BlockersForKing[1] = BitBoard.Empty;
        CapturedPiece = PieceTypes.NoPieceType;
        Previous = null;
    }

    public void UpdateRepetition()
    {
        Repetition = 0;

        var end = End();

        if (end < 4)
            return;

        var statePrevious = Previous.Previous;
        for (var i = 4; i <= end; i += 2)
        {
            statePrevious = statePrevious.Previous.Previous;
            if (statePrevious.PositionKey != PositionKey)
                continue;
            Repetition = statePrevious.Repetition != 0 ? -i : i;
            break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int End() => Math.Min(Rule50, PliesFromNull);

    public bool Equals(State other)
    {
        if (other is null) return false;
        return LastMove.Equals(other.LastMove)
               && PositionKey.Equals(other.PositionKey)
               && PawnKey.Equals(other.PawnKey)
               && EnPassantSquare.Equals(other.EnPassantSquare)
               && CastlelingRights == other.CastlelingRights
               && PliesFromNull == other.PliesFromNull
               && Rule50 == other.Rule50
               && Pinners.Equals(other.Pinners)
               && Checkers.Equals(other.Checkers)
               && CapturedPiece == other.CapturedPiece
               && Equals(Previous, other.Previous);
    }

    public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is State other && Equals(other);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(LastMove);
        hashCode.Add(PawnKey);
        hashCode.Add(MaterialKey);
        hashCode.Add(PliesFromNull);
        hashCode.Add(Rule50);
        hashCode.Add(PositionKey);
        hashCode.Add(CastlelingRights.Rights.AsInt());
        hashCode.Add(EnPassantSquare);
        hashCode.Add(Checkers);
        hashCode.Add(Previous);
        hashCode.Add(CapturedPiece);
        hashCode.Add(Pinners);
        hashCode.Add(CheckedSquares);
        return hashCode.ToHashCode();
    }
}