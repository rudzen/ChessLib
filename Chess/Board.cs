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

namespace Rudz.Chess;

using Enums;
using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Types;

public sealed class Board : IBoard
{
    #region base piece information

    private readonly Piece[] _pieces;

    private readonly BitBoard[] _bySide;

    private readonly BitBoard[] _byType;

    private readonly int[] _pieceCount;

    private readonly Square[][] _pieceList;

    private readonly int[] _index;

    #endregion base piece information

    public Board()
    {
        _pieces = new Piece[64];
        _bySide = new BitBoard[2];
        _byType = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
        _pieceCount = new int[16];
        _pieceList = new Square[64][];
        for (var i = 0; i < _pieceList.Length; i++)
        {
            var arr = new Square[16];
            arr.Fill(Types.Square.None);
            _pieceList[i] = arr;
        }

        _index = new int[64];
    }

    public void Clear()
    {
        _pieces.Clear();
        _bySide.Clear();
        _byType.Clear();
        _pieceCount.Clear();
        foreach (var s in _pieceList)
            s.Fill(Types.Square.None);
        _index.Clear();
    }

    public Piece PieceAt(Square sq)
        => _pieces[sq.AsInt()];

    public bool IsEmpty(Square sq)
        => _pieces[sq.AsInt()] == Piece.EmptyPiece;

    public void AddPiece(in Piece pc, in Square sq)
    {
        _pieces[sq.AsInt()] = pc;
        _byType[PieceTypes.AllPieces.AsInt()] |= sq;
        _byType[pc.Type().AsInt()] |= sq;
        _bySide[pc.ColorOf().Side] |= sq;
        _index[sq.AsInt()] = _pieceCount[pc.AsInt()]++;
        _pieceList[pc.AsInt()][_index[sq.AsInt()]] = sq;
        _pieceCount[PieceTypes.AllPieces.MakePiece(pc.ColorOf()).AsInt()]++;
    }

    public void RemovePiece(Square sq)
    {
        // WARNING: This is not a reversible operation. If we remove a piece in MakeMove() and
        // then replace it in TakeMove() we will put it at the end of the list and not in its
        // original place, it means index[] and pieceList[] are not invariant to a MakeMove() +
        // TakeMove() sequence.
        var pc = _pieces[sq.AsInt()];
        _byType[PieceTypes.AllPieces.AsInt()] ^= sq;
        _byType[pc.Type().AsInt()] ^= sq;
        _bySide[pc.ColorOf().Side] ^= sq;
        /* board[s] = NO_PIECE;  Not needed, overwritten by the capturing one */
        var lastSquare = _pieceList[pc.AsInt()][--_pieceCount[pc.AsInt()]];
        _index[lastSquare.AsInt()] = _index[sq.AsInt()];
        _pieceList[pc.AsInt()][_index[lastSquare.AsInt()]] = lastSquare;
        _pieceList[pc.AsInt()][_pieceCount[pc.AsInt()]] = Types.Square.None;
        _pieceCount[PieceTypes.AllPieces.MakePiece(pc.ColorOf()).AsInt()]--;
    }

    public void ClearPiece(Square sq)
        => _pieces[sq.AsInt()] = Piece.EmptyPiece;

    public void MovePiece(Square from, Square to)
    {
        // _index[from] is not updated and becomes stale. This works as long as _index[] is
        // accessed just by known occupied squares.
        var pc = _pieces[from.AsInt()];
        var fromTo = from | to;
        _byType[PieceTypes.AllPieces.AsInt()] ^= fromTo;
        _byType[pc.Type().AsInt()] ^= fromTo;
        _bySide[pc.ColorOf().Side] ^= fromTo;
        _pieces[from.AsInt()] = Piece.EmptyPiece;
        _pieces[to.AsInt()] = pc;
        _index[to.AsInt()] = _index[from.AsInt()];
        _pieceList[pc.AsInt()][_index[to.AsInt()]] = to;
    }

    public Piece MovedPiece(Move move)
        => PieceAt(move.FromSquare());

    public BitBoard Pieces()
        => _byType[PieceTypes.AllPieces.AsInt()];

    public BitBoard Pieces(PieceTypes pt)
        => _byType[pt.AsInt()];

    public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2)
        => _byType[pt1.AsInt()] | _byType[pt2.AsInt()];

    public BitBoard Pieces(Player c)
        => _bySide[c.Side];

    public BitBoard Pieces(Player c, PieceTypes pt)
        => _bySide[c.Side] & _byType[pt.AsInt()];

    public BitBoard Pieces(Player c, PieceTypes pt1, PieceTypes pt2)
        => _bySide[c.Side] & (_byType[pt1.AsInt()] | _byType[pt2.AsInt()]);

    public Square Square(PieceTypes pt, Player c)
    {
        Debug.Assert(_pieceCount[pt.MakePiece(c).AsInt()] == 1);
        return _pieceList[pt.MakePiece(c).AsInt()][0];
    }

    public ReadOnlySpan<Square> Squares(PieceTypes pt, Player c)
    {
        var squares = _pieceList[pt.MakePiece(c).AsInt()];
        var idx = Array.IndexOf(squares, Types.Square.None);
        return idx == 0
            ? ReadOnlySpan<Square>.Empty
            : squares.AsSpan()[..idx];
    }

    public int PieceCount(PieceTypes pt, Player c)
        => _pieceCount[pt.MakePiece(c).AsInt()];

    public int PieceCount(PieceTypes pt)
        => _pieceCount[pt.MakePiece(Player.White).AsInt()] + _pieceCount[pt.MakePiece(Player.Black).AsInt()];

    public int PieceCount()
        => PieceCount(PieceTypes.AllPieces);

    public IEnumerator<Piece> GetEnumerator()
        => _pieces.Where(piece => piece != Piece.EmptyPiece).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
