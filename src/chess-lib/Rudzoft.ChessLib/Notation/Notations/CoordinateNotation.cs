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
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Notation.Notations;

public sealed class CoordinateNotation : Notation
{
    public CoordinateNotation(ObjectPool<MoveList> moveLists) : base(moveLists) { }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string Convert(IPosition pos, Move move)
    {
        var (from, to) = move;

        Span<char> re = stackalloc char[8];
        var i = 0;

        re[i++] = char.ToUpper(from.FileChar);
        re[i++] = from.RankChar;
        re[i++] = '-';
        re[i++] = char.ToUpper(to.FileChar);
        re[i++] = to.RankChar;

        var captured = pos.GetPiece(to);

        // ReSharper disable once InvertIf
        if (captured != Piece.EmptyPiece)
        {
            re[i++] = '(';
            re[i++] = char.ToUpper(captured.GetPieceChar());
            re[i++] = ')';
        }

        return new(re[..i]);
    }
}