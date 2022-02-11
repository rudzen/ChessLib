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

namespace Rudz.Chess.Tables;

using Extensions;
using System;
using System.Runtime.CompilerServices;
using Types;

public sealed class KillerMoves : IKillerMoves
{
    private IPieceSquare[,] _killerMoves;
    private int _maxDepth;

    public void Initialize(int maxDepth)
    {
        _maxDepth = maxDepth;
        _killerMoves = new IPieceSquare[maxDepth + 1, 2];
        for (var depth = 0; depth <= maxDepth; depth++)
        {
            _killerMoves[depth, 0] = new PieceSquare(Piece.EmptyPiece, Square.None);
            _killerMoves[depth, 1] = new PieceSquare(Piece.EmptyPiece, Square.None);
        }
        Reset();
    }

    public int GetValue(int depth, Move move, Piece fromPiece)
    {
        if (Equals(_killerMoves[depth, 0], move, fromPiece))
            return 2;

        var result = Equals(_killerMoves[depth, 1], move, fromPiece).AsByte();
        return result;
    }

    public void UpdateValue(int depth, Move move, Piece fromPiece)
    {
        if (Equals(_killerMoves[depth, 0], move, fromPiece))
            return;

        // Shift killer move.
        _killerMoves[depth, 1].Piece = _killerMoves[depth, 0].Piece;
        _killerMoves[depth, 1].Square = _killerMoves[depth, 0].Square;
        // Update killer move.
        _killerMoves[depth, 0].Piece = fromPiece;
        _killerMoves[depth, 0].Square = move.ToSquare();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Equals(IPieceSquare killerMove, Move move, Piece fromPiece)
        => killerMove.Piece == fromPiece && killerMove.Square == move.ToSquare();

    public IPieceSquare Get(int depth, int index)
    {
        return _killerMoves[depth, index];
    }

    public void Shift(int depth)
    {
        // Shift killer moves closer to root position.
        var lastDepth = _killerMoves.Length - depth - 1;
        for (var i = 0; i <= lastDepth; i++)
        {
            _killerMoves[i, 0].Piece = _killerMoves[i + depth, 0].Piece;
            _killerMoves[i, 0].Square = _killerMoves[i + depth, 0].Square;
            _killerMoves[i, 1].Piece = _killerMoves[i + depth, 1].Piece;
            _killerMoves[i, 1].Square = _killerMoves[i + depth, 1].Square;
        }

        // Reset killer moves far from root position.
        for (var i = lastDepth + 1; i < _killerMoves.Length; i++)
        {
            _killerMoves[i, 0].Piece = Piece.EmptyPiece;
            _killerMoves[i, 0].Square = Square.None;
            _killerMoves[i, 1].Piece = Piece.EmptyPiece;
            _killerMoves[i, 1].Square = Square.None;
        }
    }

    public void Reset()
    {
        Array.Clear(_killerMoves, 0, _killerMoves.Length);
        for (var i = 0; i < _killerMoves.Length; i++)
        {
            _killerMoves[i, 0].Piece = Piece.EmptyPiece;
            _killerMoves[i, 0].Square = Square.None;
            _killerMoves[i, 1].Piece = Piece.EmptyPiece;
            _killerMoves[i, 1].Square = Square.None;
        }
    }

    public bool Equals(IKillerMoves other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        for (var j = 0; j < _maxDepth; ++j)
        {
            var otherKm = other.Get(j, 0);
            if (otherKm.Piece != _killerMoves[j, 0].Piece || otherKm.Square != _killerMoves[j, 0].Square)
                return false;
            otherKm = other.Get(j, 1);
            if (otherKm.Piece != _killerMoves[j, 1].Piece || otherKm.Square != _killerMoves[j, 1].Square)
                return false;
        }

        return false;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is KillerMoves other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (_killerMoves != null ? _killerMoves.GetHashCode() : 0);
    }
}
