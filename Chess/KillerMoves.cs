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

namespace Rudz.Chess
{
    using Enums;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class KillerMoves : IKillerMoves
    {
        private readonly IPosition _position;
        private readonly IPieceSquare[][] _killerMoves;

        public KillerMoves(IPosition position, int maxDepth)
        {
            _position = position;
            _killerMoves = new IPieceSquare[maxDepth + 1][];
            for (var depth = 0; depth <= maxDepth; depth++)
            {
                _killerMoves[depth] = new IPieceSquare[]
                    {new PieceSquare(EPieces.NoPiece, ESquare.none), new PieceSquare(EPieces.NoPiece, ESquare.none)};
            }
            Reset();
        }

        public int GetValue(int depth, Move move)
        {
            if (Equals(_killerMoves[depth][0], move))
                return 2;

            return Equals(_killerMoves[depth][1], move) ? 1 : 0;
        }

        public void UpdateValue(int depth, Move move)
        {
            if (Equals(_killerMoves[depth][0], move))
                return;

            // Shift killer move.
            _killerMoves[depth][1].Piece = _killerMoves[depth][0].Piece;
            _killerMoves[depth][1].Square = _killerMoves[depth][0].Square;
            // Update killer move.
            _killerMoves[depth][0].Piece = _position.GetPiece(move.GetFromSquare());
            _killerMoves[depth][0].Square = move.GetToSquare();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Equals(IPieceSquare killerMove, Move move)
            => killerMove.Piece == _position.GetPiece(move.GetFromSquare()) && killerMove.Square == move.GetToSquare();

        public void Shift(int depth)
        {
            // Shift killer moves closer to root position.
            var lastDepth = _killerMoves.Length - depth - 1;
            for (var i = 0; i <= lastDepth; i++)
            {
                _killerMoves[i][0].Piece = _killerMoves[i + depth][0].Piece;
                _killerMoves[i][0].Square = _killerMoves[i + depth][0].Square;
                _killerMoves[i][1].Piece = _killerMoves[i + depth][1].Piece;
                _killerMoves[i][1].Square = _killerMoves[i + depth][1].Square;
            }

            // Reset killer moves far from root position.
            for (var i = lastDepth + 1; i < _killerMoves.Length; i++)
            {
                _killerMoves[i][0].Piece = EPieces.NoPiece;
                _killerMoves[i][0].Square = ESquare.none;
                _killerMoves[i][1].Piece = EPieces.NoPiece;
                _killerMoves[i][1].Square = ESquare.none;
            }
        }

        public void Reset()
        {
            foreach (var killerMoves in _killerMoves)
            {
                killerMoves[0].Piece = EPieces.NoPiece;
                killerMoves[0].Square = ESquare.none;
                killerMoves[1].Piece = EPieces.NoPiece;
                killerMoves[1].Square = ESquare.none;
            }
        }
    }
}