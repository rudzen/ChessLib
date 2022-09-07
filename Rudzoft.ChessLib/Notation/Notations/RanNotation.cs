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
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Notation.Notations;

public sealed class RanNotation : Notation
{
    public RanNotation(IPosition pos) : base(pos)
    {
    }

    /// <summary>
    /// <para>Converts a move to RAN notation.</para>
    /// </summary>
    /// <param name="move">The move to convert</param>
    /// <returns>RAN move string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string Convert(Move move)
    {
        var (from, to) = move;

        if (move.IsCastleMove())
            return CastleExtensions.GetCastleString(to, from);

        Span<char> re = stackalloc char[6];
        var i = 0;

        var pt = Pos.GetPieceType(from);

        if (pt != PieceTypes.Pawn)
            re[i++] = pt.GetPieceChar();

        re[i++] = from.FileChar;
        re[i++] = from.RankChar;

        if (move.IsEnPassantMove())
        {
            re[i++] = 'e';
            re[i++] = 'p';
            re[i++] = from.FileChar;
        }
        else
        {
            var capturedPiece = Pos.GetPiece(to);
            if (capturedPiece != Piece.EmptyPiece)
            {
                if (pt == PieceTypes.Pawn)
                    re[i++] = from.FileChar;

                re[i++] = 'x';
                re[i++] = capturedPiece.Type().GetPieceChar();
            }
            else
                re[i++] = '-';
        }

        re[i++] = to.FileChar;
        re[i++] = to.RankChar;

        if (move.IsPromotionMove())
        {
            re[i++] = '=';
            re[i++] = move.PromotedPieceType().MakePiece(Pos.SideToMove).GetUnicodeChar();
        }

        if (Pos.InCheck)
            re[i++] = GetCheckChar();

        return new string(re[..i]);
    }
}