using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rudz.Chess.Enums;
using Rudz.Chess.Extensions;
using Rudz.Chess.Types;

namespace Rudz.Chess
{
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

        private readonly CastlelingRights[] _castlingRightsMask;

        private readonly Square[] _castlingRookSquare;

        private readonly BitBoard[] _castlingPath;

        public Board()
        {
            _pieces = new Piece[64];
            _bySide = new BitBoard[2];
            _byType = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
            _pieceCount = new int[(int) Enums.Pieces.PieceNb];
            var mem = new Memory<Square>(new Square[(int) Enums.Pieces.PieceNb]);
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

        public void AddPiece(Piece pc, Square sq)
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
            // WARNING: This is not a reversible operation. If we remove a piece in
            // do_move() and then replace it in undo_move() we will put it at the end of
            // the list and not in its original place, it means index[] and pieceList[]
            // are not invariant to a do_move() + undo_move() sequence.
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
            // index[from] is not updated and becomes stale. This works as long as index[]
            // is accessed just by known occupied squares.
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
            => PieceAt(move.GetFromSquare());

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
            var pc = pt.MakePiece(c).AsInt();
            return _pieceList[pc].TakeWhile(s => s != Types.Square.None).ToArray().AsSpan();
            // var squares = _pieceList[pc];
            // var idx = Array.IndexOf(squares, Types.Square.None);
            // return _pieceList[pt.MakePiece(c).AsInt()].AsSpan();
        }

        public int PieceCount(PieceTypes pt, Player c)
            => _pieceCount[pt.MakePiece(c).AsInt()];

        public int PieceCount(PieceTypes pt)
            => _pieceCount[pt.MakePiece(Player.White).AsInt()] + _pieceCount[pt.MakePiece(Player.Black).AsInt()];

        public IEnumerator<Piece> GetEnumerator()
        {
            return _pieces.Where(piece => piece != Piece.EmptyPiece).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}