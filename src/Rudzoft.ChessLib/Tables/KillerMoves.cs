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
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Tables;

public sealed class KillerMoves : IKillerMoves
{
    private IPieceSquare[,] _killerMoves;
    private readonly int _maxDepth;

    public static IKillerMoves Create(int maxDepth)
        => new KillerMoves(maxDepth).Initialize();

    public KillerMoves(int maxDepth)
        => _maxDepth = maxDepth + 1;

    private IKillerMoves Initialize()
    {
        _killerMoves = new IPieceSquare[_maxDepth, 2];
        for (var depth = 0; depth < _maxDepth; depth++)
        {
            _killerMoves[depth, 0] = new PieceSquareEventArgs(Piece.EmptyPiece, Square.None);
            _killerMoves[depth, 1] = new PieceSquareEventArgs(Piece.EmptyPiece, Square.None);
        }

        Reset();
        return this;
    }

    public int GetValue(int depth, Move m, Piece fromPc)
        => Equals(_killerMoves[depth, 0], m, fromPc)
            ? 2
            : Equals(_killerMoves[depth, 1], m, fromPc).AsByte();

    public void UpdateValue(int depth, Move m, Piece fromPc)
    {
        if (Equals(_killerMoves[depth, 0], m, fromPc))
            return;

        // Shift killer move.
        _killerMoves[depth, 1].Piece = _killerMoves[depth, 0].Piece;
        _killerMoves[depth, 1].Square = _killerMoves[depth, 0].Square;
        // Update killer move.
        _killerMoves[depth, 0].Piece = fromPc;
        _killerMoves[depth, 0].Square = m.ToSquare();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Equals(IPieceSquare killerMove, Move m, Piece fromPc)
        => killerMove.Piece == fromPc && killerMove.Square == m.ToSquare();

    public IPieceSquare Get(int depth, int index)
        => _killerMoves[depth, index];

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
        //Array.Clear(_killerMoves, 0, _killerMoves.Length);
        for (var i = 0; i < _maxDepth; i++)
        {
            _killerMoves[i, 0].Piece = Piece.EmptyPiece;
            _killerMoves[i, 0].Square = Square.None;
            _killerMoves[i, 1].Piece = Piece.EmptyPiece;
            _killerMoves[i, 1].Square = Square.None;
        }
    }

    public bool Equals(IKillerMoves other)
    {
        if (other is null) return false;
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
        => ReferenceEquals(this, obj) || obj is KillerMoves other && Equals(other);

    public override int GetHashCode()
        => _killerMoves != null ? _killerMoves.GetHashCode() : 0;
}