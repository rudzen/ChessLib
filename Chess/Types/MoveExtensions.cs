/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Rudz.Chess.Types
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Made in a hurry.
    /// TODO : to be organized in a more fluent manner
    /// </summary>
    public static class MoveExtensions
    {
        public static readonly Move EmptyMove = new Move();

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static (bool, Move) Locate(this Move move, IPosition pos)
        // {
        //     // force position to contain the latest moves for the position moves to be searched in
        //     var moveList = pos.GenerateMoves();
        //
        //     var element = moveList.GetMove(move.GetFromSquare(), move.GetToSquare());
        //     return element == null ? (false, EmptyMove) : (true, element);
        // }
    }
}