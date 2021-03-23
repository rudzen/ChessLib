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

namespace Rudz.Chess.Polyglot
{
    using Enums;
    using Extensions;
    using MoveGeneration;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Types;

    public sealed class Book : IDisposable
    {
        private static readonly uint[] PieceMapping = { 0, 11, 9, 7, 5, 3, 1, 0, 0, 10, 8, 6, 4, 2, 0, 0 };

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

        public Move probe(bool pickBest = true)
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

            Dictionary<ushort, List<ushort>> map_move_score = new Dictionary<ushort, List<ushort>>();

            while (e.key == key)
            {
                best = best > e.count
                    ? best
                    : e.count;

                // Map all moves with its score
                // then chose randomly between best scores

                if (!map_move_score.ContainsKey(e.count))
                {
                    map_move_score.Add(e.count, new List<ushort>());
                }
                map_move_score[e.count].Add(e.move);

                e = ReadEntry();

            }

            if (map_move_score.Count > 0)
            {
                Random r = new Random();
                int random_index = r.Next(0, map_move_score[best].Count);
                polyMove = map_move_score[best][random_index];
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

                if (to == em.Move.ToSquare())
                {
                    var type = move.MoveType();
                    if (type != MoveTypes.Promotion || type == MoveTypes.Promotion && em.Move.IsPromotionMove())
                        return em.Move;
                }
            }

            return Move.EmptyMove;
        }

        private Entry ReadEntry()
        {
            Entry e;
            e.key = BitConverter.ToUInt64(_binaryReader.ReadBytes(8).Reverse().ToArray());
            e.move = BitConverter.ToUInt16(_binaryReader.ReadBytes(2).Reverse().ToArray());
            e.count = BitConverter.ToUInt16(_binaryReader.ReadBytes(2).Reverse().ToArray());
            e.learn = BitConverter.ToUInt32(_binaryReader.ReadBytes(4).Reverse().ToArray());
            return e;
        }

        private HashKey ComputePolyglotKey()
        {
            var k = new HashKey();
            var b = _pos.Pieces();

            /*while (b)
            {
                var s = BitBoards.PopLsb(ref b);
                var p = PieceMapping[_pos.GetPiece(s).AsInt()];

                // PolyGlot pieces are: BP = 0, WP = 1, BN = 2, ... BK = 10, WK = 11
                k ^= BookZobrist.psq[p, s.AsInt()];
            }*/

            for (int i = 0; i < 64; i++)
            {
                Piece p = _pos.GetPiece(i);

                if (p != Piece.EmptyPiece)
                {
                    int encode = 0;

                    switch (p.ToString())
                    {
                        case "p": encode = 0; break;
                        case "P": encode = 1; break;
                        case "n": encode = 2; break;
                        case "N": encode = 3; break;
                        case "b": encode = 4; break;
                        case "B": encode = 5; break;
                        case "r": encode = 6; break;
                        case "R": encode = 7; break;
                        case "q": encode = 8; break;
                        case "Q": encode = 9; break;
                        case "k": encode = 10; break;
                        case "K": encode = 11; break;


                    }

                    int file = i % 8;
                    int row = i / 8;

                    int offset_p = 64 * encode + 8 * row + file;

                    k ^= BookZobrist.piecemap[offset_p];

                }

            }


            if (_pos.State.CastlelingRights != CastlelingRights.None)
            {
                var bk = _pos.State.CastlelingRights & CastlelingRights.BlackOo;
                var bq = _pos.State.CastlelingRights & CastlelingRights.BlackOoo;
                var wk = _pos.State.CastlelingRights & CastlelingRights.WhiteOo;
                var wq = _pos.State.CastlelingRights & CastlelingRights.WhiteOoo;

                if (wk == CastlelingRights.WhiteOo)
                {
                    k ^= BookZobrist.castle[0];
                }
                if (wq == CastlelingRights.WhiteOoo)
                {
                    k ^= BookZobrist.castle[1];
                }
                if (bk == CastlelingRights.BlackOo)
                {
                    k ^= BookZobrist.castle[2];
                }
                if (bq == CastlelingRights.BlackOoo)
                {
                    k ^= BookZobrist.castle[3];
                }



            }

            if (_pos.EnPassantSquare != Square.None)
                k ^= BookZobrist.enpassant[_pos.EnPassantSquare.File.AsInt()];

            if (_pos.SideToMove.IsWhite)
                k ^= BookZobrist.turn;

            return k;
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