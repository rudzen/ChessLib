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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.MoveGeneration;

/***
 * Holds a list of moves.
 * It works through an index pointer,
 * which means that re-use is never actually clearing any data, just resetting the index.
 */
public sealed class MoveList : IResettable
{
    private readonly ValMove[] _moves = new ValMove[218];
    private int _cur;

    public int Length { get; private set; }

    public ref ValMove CurrentMove => ref _moves[_cur];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MoveList operator ++(MoveList moveList)
    {
        ++moveList._cur;
        return moveList;
    }

    public ValMove this[int index]
    {
        get => _moves[index];
        set => _moves[index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in ValMove item) => _moves[Length++] = item;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Move item)
    {
        _moves[Length++].Move = item;
    }

    /// <inheritdoc />
    /// <summary>
    /// Reset the moves
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _cur = Length = 0;
        _moves[0] = ValMove.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ValMove item)
        => Contains(item.Move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Move move) => Contains(m => m == move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Square from, Square to) => Contains(m => m.FromSquare() == from && m.ToSquare() == to);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsType(MoveTypes type) => Contains(m => m.MoveType() == type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Predicate<Move> predicate)
    {
        var moveSpan = _moves.AsSpan()[..Length];
        if (moveSpan.IsEmpty)
            return false;

        ref var movesSpace = ref MemoryMarshal.GetReference(moveSpan);
        for (var i = 0; i < moveSpan.Length; ++i)
        {
            var m = Unsafe.Add(ref movesSpace, i).Move;
            if (predicate(m))
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Move> GetMoves(Predicate<Move> predicate)
    {
        for (var i = 0; i < Length; i++)
        {
            var m = this[i];
            if (predicate(m.Move))
                yield return m.Move;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Move> GetMoves()
    {
        for (var i = 0; i < Length; i++)
            yield return this[i].Move;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Generate(in IPosition pos, MoveGenerationTypes types = MoveGenerationTypes.Legal)
    {
        _cur = 0;
        ref var movesSpace = ref MemoryMarshal.GetReference(_moves.AsSpan());
        Length = MoveGenerator.Generate(in pos, ref movesSpace, 0, pos.SideToMove, types);
        _moves[Length] = ValMove.Empty;
    }

    [SkipLocalsInit]
    public static int GenerateMoveCount(in IPosition pos, MoveGenerationTypes types = MoveGenerationTypes.Legal)
    {
        Span<ValMove> moves = stackalloc ValMove[218];
        ref var movesSpace = ref MemoryMarshal.GetReference(moves);
        return MoveGenerator.Generate(in pos, ref movesSpace, 0, pos.SideToMove, types);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<ValMove> Get() =>
        Length == 0
            ? ReadOnlySpan<ValMove>.Empty
            : _moves.AsSpan()[..Length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReset()
    {
        Clear();
        return true;
    }
}