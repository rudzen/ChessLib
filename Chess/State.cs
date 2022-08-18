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

// ReSharper disable MemberCanBeInternal
namespace Rudz.Chess;

using Enums;
using Extensions;
using System;
using System.Linq;
using Types;

public sealed class State : IEquatable<State>
{
    public Move LastMove { get; set; }

    public Value[] NonPawnMaterial { get; set; }

    public HashKey PawnStructureKey { get; set; }

    public HashKey MaterialKey { get; set; }

    public int PliesFromNull { get; set; }

    public int Rule50 { get; set; }

    public CastlelingRights CastlelingRights { get; set; }

    public Square EnPassantSquare { get; set; }

    public State Previous { get; set; }

    // -----------------------------
    // Properties below this point are not copied from other state
    // since they are always recomputed
    // -----------------------------

    public HashKey Key { get; set; }

    /// <summary>
    /// Represents checked squares for side to move
    /// </summary>
    public BitBoard Checkers { get; set; }

    public BitBoard[] BlockersForKing { get; set; }

    public BitBoard[] Pinners { get; set; }

    public BitBoard[] CheckedSquares { get; set; }

    public Piece CapturedPiece { get; set; }

    public int Repetition { get; set; }

    /// <summary>
    /// Partial copy from existing state The properties not copied are re-calculated
    /// </summary>
    /// <param name="other">The current state</param>
    public State(State other)
    {
        PawnStructureKey = other.PawnStructureKey;
        MaterialKey = other.MaterialKey;
        CastlelingRights = other.CastlelingRights;
        Rule50 = other.Rule50;
        PliesFromNull = other.PliesFromNull;
        EnPassantSquare = other.EnPassantSquare;
        NonPawnMaterial = new[] { other.NonPawnMaterial[0], other.NonPawnMaterial[1] };
        Previous = other;

        Checkers = BitBoard.Empty;
        BlockersForKing = new BitBoard[2];
        Pinners = new BitBoard[2];
        CheckedSquares = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
        CapturedPiece = Piece.EmptyPiece;
    }

    public State()
    {
        LastMove = Move.EmptyMove;
        NonPawnMaterial = new[] { Value.ValueZero, Value.ValueZero };
        CastlelingRights = CastlelingRights.None;
        EnPassantSquare = Square.None;
        Checkers = BitBoard.Empty;
        CheckedSquares = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
        Pinners = new[] { BitBoard.Empty, BitBoard.Empty };
        BlockersForKing = new[] { BitBoard.Empty, BitBoard.Empty };
        CapturedPiece = Piece.EmptyPiece;
    }

    public State CopyTo(State other)
    {
        // copy over preserved values
        other.PawnStructureKey = PawnStructureKey;
        other.MaterialKey = MaterialKey;
        other.CastlelingRights = CastlelingRights;
        other.Rule50 = Rule50;
        other.PliesFromNull = PliesFromNull;
        other.EnPassantSquare = EnPassantSquare;
        other.Previous = this;

        // copy over material
        Array.Copy(NonPawnMaterial, other.NonPawnMaterial, NonPawnMaterial.Length);

        // initialize the rest of the values
        if (other.CheckedSquares == null)
            other.CheckedSquares = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
        else
            other.CheckedSquares.Fill(BitBoard.Empty);

        other.Pinners = new[] { BitBoard.Empty, BitBoard.Empty };
        other.BlockersForKing = new[] { BitBoard.Empty, BitBoard.Empty };

        return other;
    }

    public void Clear()
    {
        LastMove = Move.EmptyMove;
        NonPawnMaterial.Clear();
        PawnStructureKey = Key = MaterialKey = 0UL;
        PliesFromNull = 0;
        Repetition = 0;
        CastlelingRights = CastlelingRights.None;
        EnPassantSquare = Square.None;
        CheckedSquares.Fill(BitBoard.Empty);
        Pinners[0] = Pinners[1] = BitBoard.Empty;
        BlockersForKing[0] = BlockersForKing[1] = BitBoard.Empty;
        CapturedPiece = Piece.EmptyPiece;
        Previous = null;
    }

    public void UpdateRepetition()
    {
        var end = Rule50 < PliesFromNull
            ? Rule50
            : PliesFromNull;

        Repetition = 0;

        if (end < 4)
            return;

        var statePrevious = Previous.Previous;
        for (var i = 4; i <= end; i += 2)
        {
            statePrevious = statePrevious.Previous.Previous;
            if (statePrevious.Key != Key)
                continue;
            Repetition = statePrevious.Repetition != 0 ? -i : i;
            break;
        }
    }

    public bool Equals(State other)
    {
        if (other is null) return false;
        return LastMove.Equals(other.LastMove)
               && Key.Equals(other.Key)
               && PawnStructureKey.Equals(other.PawnStructureKey)
               && EnPassantSquare.Equals(other.EnPassantSquare)
               && CastlelingRights == other.CastlelingRights
               && NonPawnMaterial[0] == other.NonPawnMaterial[0]
               && NonPawnMaterial[1] == other.NonPawnMaterial[1]
               && PliesFromNull == other.PliesFromNull
               && Rule50 == other.Rule50
               && Pinners.Equals(other.Pinners)
               && Checkers.Equals(other.Checkers)
               && CapturedPiece == other.CapturedPiece
               && Equals(Previous, other.Previous);
    }

    public override bool Equals(object obj)
        => ReferenceEquals(this, obj) || obj is State other && Equals(other);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(LastMove);
        hashCode.Add(NonPawnMaterial);
        hashCode.Add(PawnStructureKey);
        hashCode.Add(MaterialKey);
        hashCode.Add(PliesFromNull);
        hashCode.Add(Rule50);
        hashCode.Add(Key);
        hashCode.Add(CastlelingRights.AsInt());
        hashCode.Add(EnPassantSquare);
        hashCode.Add(Checkers);
        hashCode.Add(Previous);
        hashCode.Add(CapturedPiece);
        hashCode.Add(Pinners.Where(p => !p.IsEmpty));
        hashCode.Add(CheckedSquares.Where(csq => !csq.IsEmpty));
        return hashCode.ToHashCode();
    }
}
