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

namespace Rudz.Chess.Polyglot;

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

        while (e.key == key)
        {
            best = best > e.count
                ? best
                : e.count;

            sum += e.count;

            // Choose book move according to its score. If a move has a very high score it has
            // higher probability to be choosen than a move with lower score. Note that first
            // entry is always chosen.
            if (sum > 0 && (rnd.Next() % sum) < e.count || pickBest && e.count == best)
                polyMove = e.move;
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

    private HashKey ComputePolyglotKey()
    {
        var k = new HashKey();
        var b = _pos.Pieces();

        while (b)
        {
            var s = BitBoards.PopLsb(ref b);
            var p = PieceMapping[_pos.GetPiece(s).AsInt()];

            // PolyGlot pieces are: BP = 0, WP = 1, BN = 2, ... BK = 10, WK = 11
            k ^= BookZobrist.Psq(p, s);
        }

        if (_pos.State.CastlelingRights != CastlelingRights.None)
            k = Enum.GetValues(_pos.State.CastlelingRights.GetType())
                .Cast<CastlelingRights>()
                .Where(f => _pos.State.CastlelingRights.HasFlagFast(f))
                .Aggregate(k, (current, validCastlelingFlag) => current ^ BookZobrist.Castle(validCastlelingFlag));

        if (_pos.EnPassantSquare != Square.None)
            k ^= BookZobrist.EnPassant(_pos.EnPassantSquare.File.AsInt());

        if (_pos.SideToMove.IsWhite)
            k ^= BookZobrist.Turn();

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
