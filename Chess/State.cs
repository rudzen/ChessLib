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
    using Types;

    public sealed class State
    {
        public Move LastMove { get; set; }

        public IMaterial Material { get; set; }

        public ulong PawnStructureKey { get; set; }

        public int ReversibleHalfMoveCount { get; set; }

        public int HalfMoveCount { get; set; }

        public int NullMovesInRow { get; set; }

        public int FiftyMoveRuleCounter { get; set; }

        public ulong Key { get; set; }

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
    }
}