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

using System.Linq;

namespace Rudz.Chess
{
    using Enums;
    using Fen;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Transposition;
    using Types;
    using Piece = Types.Piece;
    using Square = Types.Square;

    public sealed class Game : IGame
    {
        private const int MaxPositions = 256;

        private readonly StringBuilder _output;

        private static readonly IDictionary<(HashKey, int), ulong> _perftCache;

        public Game(IPosition pos)
        {
            Pos = pos;
            _output = new StringBuilder(256);
        }

        static Game()
        {
            Table = new TranspositionTable(256);
            _perftCache = new Dictionary<(HashKey, int), ulong>(256);
        }

        public Action<Piece, Square> PieceUpdated => Pos.PieceUpdated;

        public int MoveNumber => 0;//(PositionIndex - 1) / 2 + 1;

        public BitBoard Occupied => Pos.Pieces();

        public IPosition Pos { get; }

        public GameEndTypes GameEndType { get; set; }

        public static TranspositionTable Table { get; set; }

        public bool IsRepetition => Pos.IsRepetition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenError NewGame(string fen = Fen.Fen.StartPositionFen) => Pos.SetFen(new FenData(fen), true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenData GetFen() => Pos.GenerateFen();

        public void UpdateDrawTypes()
        {
            var gameEndType = GameEndTypes.None;
            if (IsRepetition)
                gameEndType |= GameEndTypes.Repetition;
            // if (State.Material[Player.White.Side] <= 300 && State.Material[Player.Black.Side] <= 300)//&& Pos.BoardPieces[0].Empty && Pos.BoardPieces[8].Empty)
            //     gameEndType |= GameEndTypes.MaterialDrawn;
            if (Pos.Rule50 >= 100)
                gameEndType |= GameEndTypes.FiftyMove;

            var moveList = Pos.GenerateMoves().GetMoves();
            if (moveList.Any(move => !Pos.IsLegal(move)))
                gameEndType |= GameEndTypes.Pat;

            GameEndType = gameEndType;
        }

        public override string ToString()
            => Pos.ToString();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IEnumerator<Piece> GetEnumerator()
            => Pos.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard OccupiedBySide(Player c)
            => Pos.Pieces(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Player CurrentPlayer() => Pos.SideToMove;

        public ulong Perft(int depth, bool root = false)
        {
            if (depth == 1)
                return Pos.GenerateMoves().Count;
            
            var state = new State();
            ulong tot = 0;
            var leaf = depth == 2;

            for (var ml = Pos.GenerateMoves(); !ml.Move.IsNullMove(); ++ml)
            {
                var move = ml.Move;
                if (move.IsNullMove())
                {
                    continue;
                }
                Pos.MakeMove(move, state);
                tot += leaf ? Pos.GenerateMoves().Count : Perft(depth - 1);
                Pos.TakeMove(move);
            }
            return tot;
        }
    }
}