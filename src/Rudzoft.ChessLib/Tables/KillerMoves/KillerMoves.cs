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
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Tables.KillerMoves;

public sealed class KillerMoves : IKillerMoves
{
    private static readonly PieceSquare EmptyPieceSquare = new(Piece.EmptyPiece, Square.None);
    
    private readonly PieceSquare[][] _killerMoves;
    private readonly int _maxDepth;

    public static IKillerMoves Create(int maxDepth)
        => new KillerMoves(maxDepth).Initialize();

    private KillerMoves(int maxDepth)
    {
        _maxDepth = maxDepth + 1;
        _killerMoves = new PieceSquare[_maxDepth][];
        for (var i = 0; i < _killerMoves.Length; ++i)
            _killerMoves[i] = new[] { EmptyPieceSquare, EmptyPieceSquare };
    }

    private IKillerMoves Initialize()
    {
        for (var depth = 0; depth < _maxDepth; depth++)
        {
            _killerMoves[depth][0] = EmptyPieceSquare;
            _killerMoves[depth][1] = EmptyPieceSquare;
        }

        Reset();
        return this;
    }

    public int GetValue(int depth, Move m, Piece fromPc)
        => Equals(in _killerMoves[depth][0], m, fromPc)
            ? 2
            : Equals(in _killerMoves[depth][1], m, fromPc).AsByte();

    public void UpdateValue(int depth, Move m, Piece fromPc)
    {
        if (Equals(in _killerMoves[depth][0], m, fromPc))
            return;

        // Shift killer move.
        _killerMoves[depth][1] = _killerMoves[depth][0];
        // Update killer move.
        _killerMoves[depth][0] = new PieceSquare(fromPc, m.ToSquare());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Equals(in PieceSquare killerMove, Move m, Piece fromPc)
        => killerMove.Piece == fromPc && killerMove.Square == m.ToSquare();

    public ref PieceSquare Get(int depth, int index)
        => ref _killerMoves[depth][index];

    public void Shift(int depth)
    {
        // Shift killer moves closer to root position.
        var lastDepth = _killerMoves.Length - depth - 1;
        for (var i = 0; i <= lastDepth; i++)
        {
            _killerMoves[i][0] = _killerMoves[i + depth][0];
            _killerMoves[i][1] = _killerMoves[i + depth][1];
        }

        // Reset killer moves far from root position.
        for (var i = lastDepth + 1; i < _killerMoves.Length; i++)
        {
            _killerMoves[i][0] = EmptyPieceSquare;
            _killerMoves[i][1] = EmptyPieceSquare;
        }
    }

    public void Reset()
    {
        for (var i = 0; i < _maxDepth; i++)
        {
            _killerMoves[i][0] = EmptyPieceSquare;
            _killerMoves[i][1] = EmptyPieceSquare;
        }
    }

    public bool Equals(IKillerMoves other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        for (var i = 0; i < _maxDepth; ++i)
        {
            ref var otherKm = ref other.Get(i, 0);
            if (_killerMoves[i][0] != otherKm)
                return false;

            otherKm = ref other.Get(i, 1);
            if (_killerMoves[i][1] != otherKm)
                return false;
        }

        return false;
    }

    public override bool Equals(object obj)
        => ReferenceEquals(this, obj) || obj is KillerMoves other && Equals(other);

    public override int GetHashCode()
        => _killerMoves.GetHashCode();
}