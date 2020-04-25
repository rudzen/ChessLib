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
    using System;
    using Types;

    public sealed class State : IEquatable<State>
    {
        public Move LastMove { get; set; }

        public IMaterial Material { get; set; }

        public HashKey PawnStructureKey { get; set; }

        public int ReversibleHalfMoveCount { get; set; }

        public int HalfMoveCount { get; set; }

        public int NullMovesInRow { get; set; }

        public int FiftyMoveRuleCounter { get; set; }

        public HashKey Key { get; set; }

        public CastlelingRights CastlelingRights { get; set; }

        public Square EnPassantSquare { get; set; }

        public Player SideToMove { get; set; }

        public BitBoard Pinned { get; set; }

        /// <summary>
        /// Represents checked squares for side to move
        /// </summary>
        public BitBoard Checkers { get; set; }

        /// <summary>
        /// Represents checked squares for opposition
        /// </summary>
        public BitBoard HiddenCheckers { get; set; }

        public bool InCheck { get; set; }

        public State Previous { get; set; }

        public State(State s)
        {
            LastMove = s.LastMove;
            Material = new Material();
            Material.CopyFrom(s.Material);
            PawnStructureKey = s.PawnStructureKey;
            ReversibleHalfMoveCount = s.ReversibleHalfMoveCount;
            HalfMoveCount = s.HalfMoveCount;
            NullMovesInRow = s.NullMovesInRow;
            FiftyMoveRuleCounter = s.FiftyMoveRuleCounter;
            Key = s.Key;
            CastlelingRights = s.CastlelingRights;
            EnPassantSquare = s.EnPassantSquare;
            SideToMove = s.SideToMove;
            Pinned = s.Pinned;
            Checkers = s.Checkers;
            InCheck = s.InCheck;
        }

        public State()
        {
            LastMove = MoveExtensions.EmptyMove;
            Material = new Material();
            CastlelingRights = CastlelingRights.None;
            EnPassantSquare = Squares.none;
            SideToMove = PlayerExtensions.White;
        }

        public void Clear()
        {
            LastMove = MoveExtensions.EmptyMove;
            Material.Clear();
            PawnStructureKey = Key = 0ul;
            ReversibleHalfMoveCount = NullMovesInRow = FiftyMoveRuleCounter = 0;
            CastlelingRights = CastlelingRights.None;
            EnPassantSquare = Squares.none;
            SideToMove = PlayerExtensions.White;
        }

        public bool Equals(State other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return LastMove.Equals(other.LastMove)
                   && Key.Equals(other.Key)
                   && PawnStructureKey.Equals(other.PawnStructureKey)
                   && EnPassantSquare.Equals(other.EnPassantSquare)
                   && CastlelingRights == other.CastlelingRights
                   && Equals(Material, other.Material)
                   && ReversibleHalfMoveCount == other.ReversibleHalfMoveCount
                   && HalfMoveCount == other.HalfMoveCount
                   && NullMovesInRow == other.NullMovesInRow
                   && FiftyMoveRuleCounter == other.FiftyMoveRuleCounter
                   && SideToMove.Equals(other.SideToMove)
                   && Pinned.Equals(other.Pinned)
                   && Checkers.Equals(other.Checkers)
                   && HiddenCheckers.Equals(other.HiddenCheckers)
                   && InCheck == other.InCheck
                   && Equals(Previous, other.Previous);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is State other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(LastMove);
            hashCode.Add(Material);
            hashCode.Add(PawnStructureKey);
            hashCode.Add(ReversibleHalfMoveCount);
            hashCode.Add(HalfMoveCount);
            hashCode.Add(NullMovesInRow);
            hashCode.Add(FiftyMoveRuleCounter);
            hashCode.Add(Key);
            hashCode.Add((int)CastlelingRights);
            hashCode.Add(EnPassantSquare);
            hashCode.Add(SideToMove);
            hashCode.Add(Pinned);
            hashCode.Add(Checkers);
            hashCode.Add(HiddenCheckers);
            hashCode.Add(InCheck);
            hashCode.Add(Previous);
            return hashCode.ToHashCode();
        }
    }
}