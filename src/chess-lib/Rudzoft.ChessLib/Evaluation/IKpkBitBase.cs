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

using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Evaluation;

public interface IKpkBitBase
{
    /// <summary>
    /// Normalizes a square in accordance with the data.
    /// Kpk bit-base is only used for king pawn king positions!
    /// </summary>
    /// <param name="pos">Current position</param>
    /// <param name="strongSide">The strong side (the one with the pawn)</param>
    /// <param name="sq">The square that needs to be normalized</param>
    /// <returns>Normalized square to be used with probing</returns>
    Square Normalize(IPosition pos, Color strongSide, Square sq);

    /// <summary>
    /// Probe with normalized squares and strong player
    /// </summary>
    /// <param name="strongKsq">"Strong" side king square</param>
    /// <param name="strongPawnSq">"Strong" side pawn square</param>
    /// <param name="weakKsq">"Weak" side king square</param>
    /// <param name="stm">strong side. fx strongSide == pos.SideToMove ? Color.White : Color.Black</param>
    /// <returns>true if strong side "won"</returns>
    bool Probe(Square strongKsq, Square strongPawnSq, Square weakKsq, Color stm);

    /// <summary>
    /// </summary>
    /// <param name="strongActive"></param>
    /// <param name="skSq">White King</param>
    /// <param name="wkSq">Black King</param>
    /// <param name="spSq">White Pawn</param>
    /// <returns></returns>
    bool Probe(bool strongActive, Square skSq, Square wkSq, Square spSq);

    bool IsDraw(IPosition pos);
}