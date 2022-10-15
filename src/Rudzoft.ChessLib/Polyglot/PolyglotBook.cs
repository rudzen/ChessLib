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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Polyglot;

public sealed class PolyglotBook : IDisposable
{
    // PolyGlot pieces are: BP = 0, WP = 1, BN = 2, ... BK = 10, WK = 11
    private static readonly int[] PieceMapping = { -1, 1, 3, 5, 7, 9, 11, -1, -1, 0, 2, 4, 6, 8, 10 };

    private static readonly CastleRight[] CastleRights =
    {
        CastleRight.WhiteKing,
        CastleRight.WhiteQueen,
        CastleRight.BlackKing,
        CastleRight.BlackQueen
    };

    private readonly IPosition _pos;
    private readonly FileStream _fileStream;
    private readonly BinaryReader _binaryReader;
    private readonly string _fileName;
    private readonly int _entrySize;
    private readonly Random _rnd;

    public unsafe PolyglotBook(IPosition pos)
    {
        _pos = pos;
        _entrySize = sizeof(PolyglotBookEntry);
        _rnd = new Random(DateTime.Now.Millisecond);
    }

    public string FileName
    {
        get => _fileName;
        init
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (_fileName == value)
                return;
            _fileName = value;
            _fileStream = new FileStream(value, FileMode.Open, FileAccess.Read);
            _binaryReader = new LittleEndianBinaryStreamReader(_fileStream);
        }
    }

    public Move Probe(IPosition pos, bool pickBest = true)
    {
        if (_fileName.IsNullOrEmpty() || _fileStream == null)
            return Move.EmptyMove;

        var polyMove = ushort.MinValue;
        var best = ushort.MinValue;
        var sum = uint.MinValue;
        var key = ComputePolyglotKey();
        var firstIndex = FindFirst(in key);

        _fileStream.Seek(firstIndex * _entrySize, SeekOrigin.Begin);

        var e = ReadEntry();

        while (e.Key == key.Key)
        {
            if (best <= e.Count)
                best = e.Count;

            sum += e.Count;

            // Choose book move according to its score. If a move has a very high score it has
            // higher probability to be chosen than a move with lower score. Note that first entry
            // is always chosen.
            if (sum > 0 && _rnd.Next() % sum < e.Count || pickBest && e.Count == best)
                polyMove = e.Move;

            // Stop if we wan't the top pick and move exists
            if (pickBest && polyMove != ushort.MinValue)
                break;
        }

        return polyMove == 0
            ? Move.EmptyMove
            : ConvertMove(pos, polyMove);
    }

    public void Dispose()
    {
        _fileStream?.Dispose();
        _binaryReader?.Dispose();
    }

    private Move ConvertMove(IPosition pos, ushort polyMove)
    {
        // A PolyGlot book move is encoded as follows:
        //
        // bit 0- 5: destination square (from 0 to 63)
        // bit 6-11: origin square (from 0 to 63)
        // bit 12-14: promotion piece (from KNIGHT == 1 to QUEEN == 4)
        //
        // In case book move is a non-normal move, the move have to be converted. Castleling moves
        // are especially converted to reflect castleling move format.

        Move move = polyMove;

        var (from, to) = move;

        // Promotion type move needs to be converted from PG format.
        var polyPt = (polyMove >> 12) & 7;

        if (polyPt > 0)
        {
            static PieceTypes PolyToPt(int pt) => (PieceTypes)(3 - pt);
            move = Move.Create(from, to, MoveTypes.Promotion, PolyToPt(polyPt));
        }

        var ml = _pos.GenerateMoves();

        // Iterate all known moves for current position to find a match.
        var emMoves = ml.Select(static em => em.Move)
            .Where(m => from == m.FromSquare())
            .Where(m => to == m.ToSquare())
            .Where(m =>
            {
                var type = move.MoveType();
                if (m.IsPromotionMove())
                    return type == MoveTypes.Promotion;
                else
                    return type != MoveTypes.Promotion;
            })
            .Where(m => !IsInCheck(pos, m));

        return emMoves.FirstOrDefault(Move.EmptyMove);
    }

    private static bool IsInCheck(IPosition pos, Move m)
    {
        pos.GivesCheck(m);
        var state = new State();
        pos.MakeMove(m, in state);
        var inCheck = pos.InCheck;
        pos.TakeMove(m);
        return inCheck;
    }

    private PolyglotBookEntry ReadEntry() => new(
        _binaryReader.ReadUInt64(),
        _binaryReader.ReadUInt16(),
        _binaryReader.ReadUInt16(),
        _binaryReader.ReadUInt32()
    );

    public HashKey ComputePolyglotKey()
    {
        var k = HashKey.Empty;
        var b = _pos.Pieces();

        while (b)
        {
            var s = BitBoards.PopLsb(ref b);
            var pc = _pos.GetPiece(s);
            var p = PieceMapping[pc.AsInt()];
            k ^= PolyglotBookZobrist.Psq(p, s);
        }

        foreach (var cr in CastleRights.Where(cr => _pos.State.CastlelingRights.Has(cr)))
            k ^= PolyglotBookZobrist.Castle(cr);

        if (_pos.State.EnPassantSquare != Square.None)
            k ^= PolyglotBookZobrist.EnPassant(_pos.State.EnPassantSquare.File);

        if (_pos.SideToMove.IsWhite)
            k ^= PolyglotBookZobrist.Turn();

        return k;
    }

    private long FindFirst(in HashKey key)
    {
        var low = 0L;
        var high = _fileStream.Length / _entrySize - 1;

        Debug.Assert(low <= high);

        while (low < high)
        {
            var mid = low.MidPoint(high);

            Debug.Assert(mid >= low && mid < high);

            _fileStream.Seek(mid * _entrySize, SeekOrigin.Begin);
            var e = ReadEntry();

            if (key.Key <= e.Key)
                high = mid;
            else
                low = mid + 1;
        }

        Debug.Assert(low == high);

        return low;
    }
}