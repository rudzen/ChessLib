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

using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Exceptions;
using Rudzoft.ChessLib.Notation.Notations;

namespace Rudzoft.ChessLib.Notation;

/// <summary>
/// Constructs string representation of a move based on specified move notation type.
/// See https://en.wikipedia.org/wiki/Chess_notation
/// </summary>
public sealed class MoveNotation : IMoveNotation
{
    private readonly INotation[] _notations;

    private MoveNotation(IPosition pos)
    {
        _notations = new INotation[]
        {
            new SanNotation(pos),
            new FanNotation(pos),
            new LanNotation(pos),
            new RanNotation(pos),
            null, // Cran
            new SmithNotation(pos),
            null, // Descriptive
            new CoordinateNotation(pos),
            new IccfNotation(pos),
            new UciNotation(pos)
        };
    }

    public static IMoveNotation Create(IPosition pos)
        => new MoveNotation(pos);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public INotation ToNotation(MoveNotations moveNotation = MoveNotations.Fan)
    {
        var notation = _notations[(int)moveNotation];

        if (notation == null)
            throw new InvalidMove("Invalid move notation detected.");

        return notation;
    }
}