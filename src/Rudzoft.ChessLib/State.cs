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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

// ReSharper disable MemberCanBeInternal
namespace Rudzoft.ChessLib;

public sealed class State : IEquatable<State>
{
    [Description("Hash key for material")]
    public HashKey MaterialKey { get; set; }

    [Description("Hash key for pawns")]
    public HashKey PawnKey { get; set; }

    [Description("Number of half moves clock since the last pawn advance or any capture")]
    public int ClockPly { get; set; }

    public int NullPly { get; set; }

    public CastleRight CastleRights { get; set; } = CastleRight.None;

    [Description("En-passant -> 'in-passing' square")]
    public Square EnPassantSquare { get; set; } = Square.None;

    // -----------------------------
    // Properties below this point are not copied from other state
    // since they are always recomputed
    // -----------------------------

    public HashKey PositionKey { get; set; }

    /// <summary>
    /// Represents checked squares for side to move
    /// </summary>
    public BitBoard Checkers { get; set; } = BitBoard.Empty;

    public BitBoard[] BlockersForKing { get; } = [BitBoard.Empty, BitBoard.Empty];

    public BitBoard[] Pinners { get; } = [BitBoard.Empty, BitBoard.Empty];

    public BitBoard[] CheckedSquares { get; private set; } = new BitBoard[PieceType.Count];

    public PieceType CapturedPiece { get; set; } = PieceType.NoPieceType;

    public Move LastMove { get; set; } = Move.EmptyMove;

    public int Repetition { get; set; }

    public State Previous { get; private set; }

    public State CopyTo(State other)
    {
        other ??= new();

        // copy over preserved values
        other.MaterialKey = MaterialKey;
        other.PawnKey = PawnKey;
        other.ClockPly = ClockPly;
        other.NullPly = NullPly;
        other.CastleRights = CastleRights;
        other.EnPassantSquare = EnPassantSquare;
        other.Previous = this;

        // initialize the rest of the values
        other.CheckedSquares ??= new BitBoard[PieceType.NoPieceType];

        return other;
    }

    public void Clear()
    {
        LastMove = Move.EmptyMove;
        PawnKey = PositionKey = MaterialKey = HashKey.Empty;
        NullPly = 0;
        Repetition = 0;
        CastleRights = CastleRight.None;
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
    public int End() => Math.Min(ClockPly, NullPly);

    public bool Equals(State other)
    {
        if (other is null) return false;
        return LastMove.Equals(other.LastMove)
               && PositionKey.Equals(other.PositionKey)
               && PawnKey.Equals(other.PawnKey)
               && EnPassantSquare.Equals(other.EnPassantSquare)
               && CastleRights == other.CastleRights
               && NullPly == other.NullPly
               && ClockPly == other.ClockPly
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
        hashCode.Add(NullPly);
        hashCode.Add(ClockPly);
        hashCode.Add(PositionKey);
        hashCode.Add(CastleRights.Rights.AsInt());
        hashCode.Add(EnPassantSquare);
        hashCode.Add(Checkers);
        hashCode.Add(Previous);
        hashCode.Add(CapturedPiece);
        hashCode.Add(Pinners);
        hashCode.Add(CheckedSquares);
        return hashCode.ToHashCode();
    }
}