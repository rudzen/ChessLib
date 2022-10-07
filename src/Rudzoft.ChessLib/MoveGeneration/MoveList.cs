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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.MoveGeneration;

public sealed class MoveList : IMoveList
{
    private readonly ExtMove[] _moves;
    private int _cur;

    public MoveList() => _moves = new ExtMove[218];

    int IReadOnlyCollection<ExtMove>.Count => Length;

    public int Length { get; private set; }

    public Move CurrentMove => _moves[_cur].Move;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MoveList operator ++(MoveList moveList)
    {
        ++moveList._cur;
        return moveList;
    }

    public ExtMove this[int index]
    {
        get => _moves[index];
        set => _moves[index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in ExtMove item) => _moves[Length++] = item;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Move item) => _moves[Length++].Move = item;

    /// <inheritdoc />
    /// <summary>
    /// Reset the moves
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _cur = Length = 0;
        _moves[0] = ExtMove.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in ExtMove item)
        => Contains(item.Move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Move item)
    {
        for (var i = 0; i < Length; ++i)
            if (_moves[i].Move == item)
                return true;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Square from, Square to)
    {
        for (var i = 0; i < Length; ++i)
        {
            var move = _moves[i].Move;
            if (move.FromSquare() == from && move.ToSquare() == to)
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Generate(in IPosition pos, MoveGenerationType type = MoveGenerationType.Legal)
    {
        _cur = 0;
        Length = MoveGenerator.Generate(in pos, _moves.AsSpan(), 0, pos.SideToMove, type);
        _moves[Length] = ExtMove.Empty;
    }

    public static int GenerateMoveCount(in IPosition pos, MoveGenerationType type = MoveGenerationType.Legal)
    {
        Span<ExtMove> moves = stackalloc ExtMove[218];
        return MoveGenerator.Generate(in pos, moves, 0, pos.SideToMove, type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<ExtMove> Get() =>
        Length == 0
            ? ReadOnlySpan<ExtMove>.Empty
            : _moves.AsSpan()[..Length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<ExtMove> GetEnumerator()
        => _moves.Take(Length).GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}