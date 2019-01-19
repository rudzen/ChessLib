/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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
    using Data;
    using Enums;

    /// <summary>
    /// Type for 64 bit key.
    /// Used for Zobrist hashing, but can be used for other things as well, such as Transpositional Table etc.
    /// </summary>
    public struct Key
    {
        public ulong Value;

        public uint GetFirst32Bits() => (uint)Value;

        public uint GetLast32Bits() => (uint)(Value >> 32);

        public void Hash(ECastleling castleling, ERank rank) => Value ^= Zobrist.GetZobristCastleling((ECastlelingRights) rank);

        public void Hash(Piece piece, Square square) => Value ^= Zobrist.GetZobristPst(piece, square);

        public void HashSide() => Value ^= Zobrist.GetZobristSide();

        public void Hash(EFile enPassantFile) => Value ^= Zobrist.GetZobristEnPessant(enPassantFile);
    }
}