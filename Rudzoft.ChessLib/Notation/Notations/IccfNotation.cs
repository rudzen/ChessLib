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
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Notation.Notations;

public sealed class IccfNotation : Notation
{
    public IccfNotation(IPosition pos) : base(pos)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string Convert(Move move)
    {
        var (from, to) = move;

        Span<char> re = stackalloc char[5];
        var i = 0;

        re[i++] = (char)('1' + from.File.AsInt());
        re[i++] = (char)('1' + from.Rank.AsInt());
        re[i++] = (char)('1' + to.File.AsInt());
        re[i++] = (char)('1' + to.Rank.AsInt());

        // ReSharper disable once InvertIf
        if (move.IsPromotionMove())
        {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            var c = move.PromotedPieceType() switch
            {
                PieceTypes.Queen => 1,
                PieceTypes.Rook => 2,
                PieceTypes.Bishop => 3,
                PieceTypes.Knight => 4,
                _ => throw new NotImplementedException()
            };

            re[i++] = (char)('0' + c);
        }

        return new string(re[..i]);
    }
}