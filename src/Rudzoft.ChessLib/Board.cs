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

using System.Collections;
using System.Diagnostics;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib;

public sealed class Board : IBoard
{
    private readonly Piece[] _board = new Piece[Types.Square.Count];

    private readonly BitBoard[] _bySide = new BitBoard[Player.Count];

    private readonly BitBoard[] _byType = new BitBoard[PieceType.Count];

    private readonly int[] _pieceCount = new int[Piece.Count];

    public void Clear()
    {
        Array.Fill(_board, Piece.EmptyPiece);
        _bySide[0] = _bySide[1] = BitBoard.Empty;
        Array.Fill(_byType, BitBoard.Empty);
        Array.Fill(_pieceCount, 0);
    }

    public Piece PieceAt(Square sq) => _board[sq];

    public bool IsEmpty(Square sq) => _board[sq] == Piece.EmptyPiece;

    public void AddPiece(Piece pc, Square sq)
    {
        Debug.Assert(sq.IsOk);

        var color = pc.ColorOf();
        var type = pc.Type();

        _board[sq] = pc;
        _byType[PieceType.AllPieces] |= sq;
        _byType[type] |= sq;
        _bySide[color] |= sq;
        _pieceCount[pc]++;
        _pieceCount[PieceType.AllPieces.MakePiece(color)]++;
    }

    public void RemovePiece(Square sq)
    {
        Debug.Assert(sq.IsOk);

        var pc = _board[sq];
        _byType[PieceType.AllPieces] ^= sq;
        _byType[pc.Type()] ^= sq;
        _bySide[pc.ColorOf()] ^= sq;
        _board[sq] = Piece.EmptyPiece;
        _pieceCount[pc]--;
        _pieceCount[PieceType.AllPieces.MakePiece(pc.ColorOf())]--;
    }

    public void ClearPiece(Square sq)
        => _board[sq] = Piece.EmptyPiece;

    public void MovePiece(Square from, Square to)
    {
        Debug.Assert(from.IsOk && to.IsOk);

        var pc = _board[from];
        var fromTo = from | to;
        _byType[PieceType.AllPieces] ^= fromTo;
        _byType[pc.Type()] ^= fromTo;
        _bySide[pc.ColorOf()] ^= fromTo;
        _board[from] = Piece.EmptyPiece;
        _board[to] = pc;
    }

    public Piece MovedPiece(Move m) => PieceAt(m.FromSquare());

    public BitBoard Pieces() => _byType[PieceType.AllPieces];

    public BitBoard Pieces(PieceType pt) => _byType[pt];

    public BitBoard Pieces(PieceType pt1, PieceType pt2) => _byType[pt1] | _byType[pt2];

    public BitBoard Pieces(Player p) => _bySide[p];

    public BitBoard Pieces(Player p, PieceType pt) => _bySide[p] & _byType[pt];

    public BitBoard Pieces(Player p, PieceType pt1, PieceType pt2)
        => _bySide[p] & (_byType[pt1] | _byType[pt2]);

    public Square Square(PieceType pt, Player p)
    {
        Debug.Assert(_pieceCount[pt.MakePiece(p)] == 1);
        return Pieces(p, pt).Lsb();
    }

    public int PieceCount(Piece pc) => _pieceCount[pc];

    public int PieceCount(PieceType pt, Player p) => PieceCount(pt.MakePiece(p));

    public int PieceCount(PieceType pt)
        => _pieceCount[pt.MakePiece(Player.White)] + _pieceCount[pt.MakePiece(Player.Black)];

    public int PieceCount() => PieceCount(PieceTypes.AllPieces);

    public IEnumerator<Piece> GetEnumerator()
        => _board.Where(static pc => pc != Piece.EmptyPiece).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}