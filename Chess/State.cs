/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Rudz.Chess
{
    using Enums;
    using Extensions;
    using System;
    using Types;

    public sealed class State : IEquatable<State>
    {
        public Move LastMove { get; set; }

        public IMaterial Material { get; set; }

        public HashKey PawnStructureKey { get; set; }

        public int PliesFromNull { get; set; }

        public int Rule50 { get; set; }

        public HashKey Key { get; set; }

        public CastlelingRights CastlelingRights { get; set; }

        public Square EnPassantSquare { get; set; }

        public Piece CapturedPiece { get; set; }

        public BitBoard[] BlockersForKing { get; set; }

        public BitBoard[] Pinners { get; set; }

        /// <summary>
        /// Represents checked squares for side to move
        /// </summary>
        public BitBoard Checkers { get; set; }

        public BitBoard[] CheckedSquares { get; set; }

        public State Previous { get; set; }

        public int Repetition { get; set; }

        /// <summary>
        /// Partial copy from existing state The properties not copied are re-calculated
        /// </summary>
        /// <param name="other">The current state</param>
        public State(State other)
        {
            PawnStructureKey = other.PawnStructureKey;
            CastlelingRights = other.CastlelingRights;
            Rule50 = other.Rule50;
            PliesFromNull = other.PliesFromNull;
            EnPassantSquare = other.EnPassantSquare;
            Previous = other;

            Material = new Material();
            Material.CopyFrom(other.Material);

            CheckedSquares = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
            Pinners = new BitBoard[2];
            BlockersForKing = new BitBoard[2];
        }

        public State()
        {
            LastMove = Move.EmptyMove;
            Material = new Material();
            CastlelingRights = CastlelingRights.None;
            EnPassantSquare = Square.None;
            Checkers = BitBoard.Empty;
            CheckedSquares = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
            Pinners = new BitBoard[2];
            BlockersForKing = new BitBoard[2];
            CapturedPiece = Piece.EmptyPiece;
        }

        public State CopyTo(State other)
        {
            // copy over preserved values
            other.PawnStructureKey = PawnStructureKey;
            other.CastlelingRights = CastlelingRights;
            other.Rule50 = Rule50;
            other.PliesFromNull = PliesFromNull;
            other.EnPassantSquare = EnPassantSquare;
            other.Previous = this;

            // copy over material
            other.Material = new Material();
            Material.CopyTo(other.Material);

            // initialize the rest of the values
            other.CheckedSquares = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
            other.Pinners = new BitBoard[2];
            other.BlockersForKing = new BitBoard[2];

            return other;
        }

        public void Clear()
        {
            LastMove = Move.EmptyMove;
            Material.Clear();
            PawnStructureKey = Key = 0ul;
            PliesFromNull = Repetition = 0;
            CastlelingRights = CastlelingRights.None;
            EnPassantSquare = Square.None;
            CheckedSquares.Fill(BitBoard.Empty);
            Pinners.Fill(BitBoard.Empty);
            BlockersForKing.Fill(BitBoard.Empty);
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
            if (ReferenceEquals(null, other)) return false;
            // if (ReferenceEquals(this, other)) return true;
            return LastMove.Equals(other.LastMove)
                   && Key.Equals(other.Key)
                   && PawnStructureKey.Equals(other.PawnStructureKey)
                   && EnPassantSquare.Equals(other.EnPassantSquare)
                   && CastlelingRights == other.CastlelingRights
                   && Equals(Material, other.Material)
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
            hashCode.Add(Material);
            hashCode.Add(PawnStructureKey);
            hashCode.Add(PliesFromNull);
            hashCode.Add(Rule50);
            hashCode.Add(Key);
            hashCode.Add((int)CastlelingRights);
            hashCode.Add(EnPassantSquare);
            hashCode.Add(Checkers);
            hashCode.Add(Previous);
            hashCode.Add(CapturedPiece);
            foreach (var pinner in Pinners)
                hashCode.Add(pinner);
            foreach (var checkedSquare in CheckedSquares)
                hashCode.Add(checkedSquare);
            return hashCode.ToHashCode();
        }
    }
}