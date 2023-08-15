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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Polyglot;

public sealed class PolyglotBook : IPolyglotBook
{
    private static readonly CastleRight[] CastleRights =
    {
        CastleRight.WhiteKing,
        CastleRight.WhiteQueen,
        CastleRight.BlackKing,
        CastleRight.BlackQueen
    };

    private readonly FileStream _fileStream;
    private readonly BinaryReader _binaryReader;
    private readonly string _bookFilePath;
    private readonly int _entrySize;
    private readonly Random _rnd;
    private readonly ObjectPool<IMoveList> _moveListPool;

    private PolyglotBook(ObjectPool<IMoveList> pool)
    {
        _entrySize = Unsafe.SizeOf<PolyglotBookEntry>();
        _rnd = new Random(DateTime.Now.Millisecond);
        _moveListPool = pool;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static PolyglotBook Create(ObjectPool<IMoveList> pool)
    {
        return new PolyglotBook(pool);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static PolyglotBook Create(ObjectPool<IMoveList> pool, string path, string file)
    {
        return new PolyglotBook(pool)
        {
            BookFile = Path.Combine(path, file)
        };
    }

    public string BookFile
    {
        get => _bookFilePath;
        init
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (_bookFilePath == value)
                return;
            _bookFilePath = value;
            _fileStream = new FileStream(value, FileMode.Open, FileAccess.Read);
            _binaryReader = BitConverter.IsLittleEndian
                ? new LittleEndianBinaryStreamReader(_fileStream)
                : new BinaryReader(_fileStream);
        }
    }

    public Move Probe(IPosition pos, bool pickBest = true)
    {
        if (_bookFilePath.IsNullOrEmpty() || _fileStream == null ||_binaryReader == null)
            return Move.EmptyMove;

        var polyMove = ushort.MinValue;
        var best = ushort.MinValue;
        var sum = uint.MinValue;
        var key = ComputePolyglotKey(pos);
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

        Move move = polyMove;

        var (from, to) = move;

        // Promotion type move needs to be converted from PG format.
        var polyPt = (polyMove >> 12) & 7;

        if (polyPt > 0)
        {
            static PieceTypes PolyToPt(int pt) => (PieceTypes)(3 - pt);
            move = Move.Create(from, to, MoveTypes.Promotion, PolyToPt(polyPt));
        }

        var ml = _moveListPool.Get();
        ml.Generate(in pos);
        var moves = ml.Get();

        var mm = SelectMove(in pos, from, to, move.MoveType(), moves);
        
        _moveListPool.Return(ml);

        return mm;
    }

    private static Move SelectMove(in IPosition pos, Square polyFrom, Square polyTo, MoveTypes polyType, ReadOnlySpan<ValMove> moves)
    {
        // Iterate all known moves for current position to find a match.
        foreach (var valMove in moves)
        {
            var m = valMove.Move;

            if (polyFrom != m.FromSquare() || polyTo != m.ToSquare())
                continue;

            var promotionMatches = m.IsPromotionMove()
                ? polyType == MoveTypes.Promotion
                : polyType != MoveTypes.Promotion;
            
            if (promotionMatches && !IsInCheck(pos, m))
                return m;
        }

        return Move.EmptyMove;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInCheck(IPosition pos, Move m)
    {
        return pos.GivesCheck(m);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PolyglotBookEntry ReadEntry() => new(
        _binaryReader.ReadUInt64(),
        _binaryReader.ReadUInt16(),
        _binaryReader.ReadUInt16(),
        _binaryReader.ReadUInt32()
    );

    public HashKey ComputePolyglotKey(in IPosition pos)
    {
        var k = HashKey.Empty;
        var b = pos.Pieces();

        while (b)
        {
            var s = BitBoards.PopLsb(ref b);
            var pc = pos.GetPiece(s);
            k ^= PolyglotBookZobrist.Psq(pc, s);
        }

        var crSpan = CastleRights.AsSpan();
        ref var crSpace = ref MemoryMarshal.GetReference(crSpan);
        for (var i = 0; i < CastleRights.Length; ++i)
        {
            var cr = Unsafe.Add(ref crSpace, i);
            if (pos.State.CastlelingRights.Has(cr))
                k ^= PolyglotBookZobrist.Castle(cr);
        }

        if (pos.State.EnPassantSquare != Square.None)
            k ^= PolyglotBookZobrist.EnPassant(pos.State.EnPassantSquare.File);

        if (pos.SideToMove.IsWhite)
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