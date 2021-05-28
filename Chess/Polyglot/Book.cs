/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2021 Rudy Alex Kohn

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

using System.Collections;
using System.Collections.Generic;

namespace Rudz.Chess.Polyglot
{
    using Enums;
    using Extensions;
    using MoveGeneration;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Types;

    public sealed class Book : IDisposable
    {
        private static readonly int[] PieceMapping =
        {
            /* no piece */     0,
            /* white pawn */   1,
            /* white knight */ 3,
            /* white bishop */ 5,
            /* white rook */   7,
            /* white queen */  9,
            /* white king */  11,
            /* no piece */     0,
            /* no piece */     0,
            /* black pawn */   0,
            /* black knight */ 2,
            /* black bishop */ 4,
            /* black rook */   6,
            /* black queen */  8,
            /* black king */  10,
            /* no piece */     0
        };

        private static readonly IDictionary<Piece, int> PieceMap;
        private static readonly IDictionary<CastlelingRights, int> CastlelingMap;

        static Book()
        {
            PieceMap = new Dictionary<Piece, int>(12)
            {
                {Pieces.BlackPawn, 0},
                {Pieces.WhitePawn, 1},
                {Pieces.BlackKnight, 2},
                {Pieces.WhiteKnight, 3},
                {Pieces.BlackBishop, 4},
                {Pieces.WhiteBishop, 5},
                {Pieces.BlackRook, 6},
                {Pieces.WhiteRook, 7},
                {Pieces.BlackQueen, 8},
                {Pieces.WhiteQueen, 9},
                {Pieces.BlackKing, 10},
                {Pieces.WhiteKing, 11}
            };

            CastlelingMap = new Dictionary<CastlelingRights, int>(4)
            {
                {CastlelingRights.BlackOo, 0},
                {CastlelingRights.WhiteOo, 1},
                {CastlelingRights.BlackOoo, 2},
                {CastlelingRights.WhiteOoo, 3}
            };

            /*
             *                 if (castlelingRights.HasFlagFast(CastlelingRights.WhiteOo))
                    k ^= BookZobrist.Castle(0);
                if (castlelingRights.HasFlagFast(CastlelingRights.WhiteOoo))
                    k ^= BookZobrist.Castle(1);
                if (castlelingRights.HasFlagFast(CastlelingRights.BlackOo))
                    k ^= BookZobrist.Castle(2);
                if (castlelingRights.HasFlagFast(CastlelingRights.BlackOoo))
                    k ^= BookZobrist.Castle(3);
             */
        }

        /*

         // PolyGlot pieces are: BP = 0, WP = 1, BN = 2, ... BK = 10, WK = 11


         *         NoPiece = 0,
        WhitePawn = 1,
        WhiteKnight = 2,
        WhiteBishop = 3,
        WhiteRook = 4,
        WhiteQueen = 5,
        WhiteKing = 6,
        BlackPawn = 9,
        BlackKnight = 10,
        BlackBishop = 11,
        BlackRook = 12,
        BlackQueen = 13,
        BlackKing = 14,
        PieceNb = 15
         */
        private readonly IPosition _pos;
        private FileStream _fileStream;
        private BinaryReader _binaryReader;
        private string _fileName;
        private readonly int _entrySize;

        private struct Entry
        {
            public ulong key;
            public ushort move;
            public ushort count;
            public uint learn;
        }

        public unsafe Book(IPosition pos)
        {
            _pos = pos;
            _entrySize = sizeof(Entry);
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName == value)
                    return;
                _fileName = value;
                _fileStream = new FileStream(value, FileMode.Open, FileAccess.Read);
                _binaryReader = new BinaryReader(_fileStream);
            }
        }

        public Move Probe(bool pickBest = true)
        {
            if (_fileName.IsNullOrEmpty() || _fileStream == null)
                return Move.EmptyMove;

            var rnd = new Random(DateTime.Now.Millisecond);

            ushort polyMove = 0;
            ushort best = 0;
            uint sum = 0;
            var key = ComputePolyglotKey();

            _fileStream.Seek(FindFirst(key) * _entrySize, SeekOrigin.Begin);
            var e = ReadEntry();

            while (e.key == key)
            {
                best = best > e.count
                    ? best
                    : e.count;

                sum += e.count;

                // Choose book move according to its score. If a move has a very high score it has
                // higher probability to be chosen than a move with lower score. Note that first
                // entry is always chosen.
                if (sum > 0 && rnd.Next() % sum < e.count || pickBest && e.count == best)
                    polyMove = e.move;

                e = ReadEntry();
            }

            return polyMove == 0
                ? Move.EmptyMove
                : ConvertMove(polyMove);
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
            _binaryReader?.Dispose();
        }

        private Move ConvertMove(ushort m)
        {
            // A PolyGlot book move is encoded as follows:
            //
            // bit 0- 5: destination square (from 0 to 63) bit 6-11: origin square (from 0 to 63)
            // bit 12-14: promotion piece (from KNIGHT == 1 to QUEEN == 4)
            //
            // In case book move is a non-normal move, the move have to be converted. Castleling
            // moves are especially converted to reflect Mirage castleling move format.

            Move move = m;

            var from = move.FromSquare();
            var to = move.ToSquare();

            static PieceTypes PolyToPt(int pt) => (PieceTypes)(3 - pt);

            // Promotion type move needs to be converted from PG to Mirage format.
            var polyPt = (m >> 12) & 7;

            move = polyPt > 0
                ? Move.Create(from, to, MoveTypes.Promotion, PolyToPt(polyPt))
                : Move.Create(from, to);

            var ml = _pos.GenerateMoves();

            // Iterate all known moves for current position to find a match.

            foreach (var em in ml)
            {
                if (from != em.Move.FromSquare())
                    continue;

                if (to != em.Move.ToSquare())
                    continue;

                var type = move.MoveType();
                if (type != MoveTypes.Promotion || type == MoveTypes.Promotion && em.Move.IsPromotionMove())
                    return em.Move;
            }

            return Move.EmptyMove;
        }

        private Entry ReadEntry()
        {
            Entry e;
            e.key = _binaryReader.ReadUInt64();
            e.move = _binaryReader.ReadUInt16();
            e.count = _binaryReader.ReadUInt16();
            e.learn = _binaryReader.ReadUInt32();
            return e;
        }

        private ulong ComputePolyglotKey()
        {
            var k = new HashKey();
            var k2 = new HashKey();
            var b = _pos.Pieces();

            while (b)
            {
                var s = BitBoards.PopLsb(ref b);
                var pc = _pos.GetPiece(s);
                var p = PieceMap[pc];
                var p2 = PieceMapping[pc.AsInt()];

                // PolyGlot pieces are: BP = 0, WP = 1, BN = 2, ... BK = 10, WK = 11
                k ^= BookZobrist.Psq(p, s);
                k2 ^= BookZobrist.Psq(p2, s);
            }

            b = _pos.State.CastlelingRights.AsInt();

            while (b)
            {
                var idx = BitBoards.PopLsb(ref b).AsInt();
                k ^= BookZobrist.Castle(idx);
            }

            // var castlelingRights = _pos.State.CastlelingRights;
            // if (castlelingRights != CastlelingRights.None)
            // {
            //     if (castlelingRights.HasFlagFast(CastlelingRights.WhiteOo))
            //         k ^= BookZobrist.Castle(CastlelingMap[CastlelingRights.WhiteOo]);
            //     if (castlelingRights.HasFlagFast(CastlelingRights.WhiteOoo))
            //         k ^= BookZobrist.Castle(CastlelingMap[CastlelingRights.WhiteOoo]);
            //     if (castlelingRights.HasFlagFast(CastlelingRights.BlackOo))
            //         k ^= BookZobrist.Castle(CastlelingMap[CastlelingRights.BlackOo]);
            //     if (castlelingRights.HasFlagFast(CastlelingRights.BlackOoo))
            //         k ^= BookZobrist.Castle(CastlelingMap[CastlelingRights.BlackOoo]);
            // }

            if (_pos.EnPassantSquare != Square.None)
                k ^= BookZobrist.EnPassant(_pos.EnPassantSquare.File.AsInt());

            if (_pos.SideToMove.IsWhite)
                k ^= BookZobrist.Turn();

            return k.Key;
        }

        private long FindFirst(HashKey key)
        {
            var low = 0L;
            var high = _fileStream.Length / _entrySize - 1;

            Debug.Assert(low <= high);

            while (low < high)
            {
                var mid = (low + high) >> 1;

                Debug.Assert(mid >= low && mid < high);

                _fileStream.Seek(mid * _entrySize, SeekOrigin.Begin);
                var e = ReadEntry();

                if (key.Key <= e.key)
                    high = mid;
                else
                    low = mid + 1;
            }
            Debug.Assert(low == high);

            return low;
        }
    }
}