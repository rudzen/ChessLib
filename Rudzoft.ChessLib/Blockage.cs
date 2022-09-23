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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Types;

[assembly: InternalsVisibleTo("Chess.Test")]

namespace Rudzoft.ChessLib;

/// <summary>
/// Computes pawn blockage (fences) See
/// https://elidavid.com/pubs/blockage2.pdf
/// https://pdfs.semanticscholar.org/31c2/d37c80ea1aef0676ba30393bc46c0ccc70e9.pdf
/// </summary>
public sealed class Blockage : IBlockage
{
    private static readonly BitBoard PawnFileASquares =
        File.FileA.FileBB() & ~(Rank.Rank1.RankBB() | Rank.Rank8.RankBB());

    private readonly IPosition _pos;

    private readonly BitBoard _ourPawns;

    private readonly BitBoard _theirPawns;

    private readonly Piece _ourPawn;

    private readonly Piece _theirPawn;

    private readonly Player _us;

    private readonly Player _them;

    /// <summary>
    /// Contains the rank for each file which has a fence
    /// </summary>
    private readonly Rank[] _fenceRank;

    private BitBoard _dynamicPawns;
    private BitBoard _fixedPawn;
    private BitBoard _marked;
    private BitBoard _fence;
    private BitBoard _processed;

    public Blockage(in IPosition pos)
    {
        _pos = pos;
        _fenceRank = new Rank[File.Count];
        _us = pos.SideToMove;
        _them = ~_us;
        _ourPawns = pos.Pieces(PieceTypes.Pawn, _us);
        _theirPawns = pos.Pieces(PieceTypes.Pawn, _them);
        _ourPawn = PieceTypes.Pawn.MakePiece(_us);
        _theirPawn = ~_ourPawn;
    }

    /// <summary>
    /// Computes whether the current position contains a pawn fence which makes the game a draw.
    /// </summary>
    /// <returns>true if the game is a draw position - otherwise false</returns>
    public bool IsBlocked()
    {
        // Quick check if there is only pawns and kings on the board It might be possible to
        // have a minor piece and exchange it into a passing pawn
        if (_pos.Board.PieceCount(PieceTypes.AllPieces) > _pos.Board.PieceCount(PieceTypes.Pawn) + 2)
            return false;

        var up = _us.PawnPushDistance();

        MarkOurPawns(up);
        MarkTheirPawns();

        var isFenceFormed = IsFenceFormed();

        if (!isFenceFormed)
            return false;

        ComputeFenceRanks();

        var ourKsq = _pos.GetKingSquare(_us);

        if (ourKsq.RelativeRank(_us) > _fenceRank[ourKsq.File.AsInt()].Relative(_us))
            return false;

        var theirKsq = _pos.GetKingSquare(_them);
        _dynamicPawns |= ComputeDynamicFencedPawns();

        while (_dynamicPawns)
        {
            var sq = BitBoards.PopLsb(ref _dynamicPawns);
            var (r, f) = sq;
            var rr = r.Relative(_us);

            if (r > _fenceRank[f.AsInt()])
            {
                if ((_theirPawns & sq.PassedPawnFrontAttackSpan(_us)).IsEmpty &&
                    (theirKsq.File != f || theirKsq.Rank.Relative(_us) < rr))
                    return false;
            }
            else if (_fence & sq)
            {
                if (rr >= Ranks.Rank6)
                    return false;

                if (_pos.GetPiece(sq + _us.PawnDoublePushDistance()) != _theirPawn)
                {
                    if (theirKsq.File != f || theirKsq.RelativeRank(_us) < rr)
                        return false;

                    if (f != File.FileA)
                    {
                        if (_pos.GetPiece(sq + Direction.West) != _ourPawn)
                            return false;

                        if (BitBoards.PopCount(_ourPawns & PreviousFile(f)) > 1)
                            return false;

                        if ((_fixedPawn & (sq + Direction.West)).IsEmpty)
                            return false;

                        if ((_fence & (sq + Direction.West)).IsEmpty)
                            return false;
                    }

                    if (f != File.FileH)
                    {
                        if (_pos.GetPiece(sq + Direction.East) != _ourPawn)
                            return false;

                        if (BitBoards.PopCount(_ourPawns & NextFile(f)) > 1)
                            return false;

                        if ((_fixedPawn & (sq + Direction.East)).IsEmpty)
                            return false;

                        if ((_fence & (sq + Direction.East)).IsEmpty)
                            return false;
                    }
                }

                if ((sq + _us.PawnDoublePushDistance()).PawnAttack(_us) & _theirPawns)
                    return false;

                if (BitBoards.MoreThanOne(_ourPawns & f))
                    return false;
            }
            else if (r < _fenceRank[f.AsInt()])
            {
                sq += up;
                r = sq.Rank;
                rr = r.Relative(_us);

                while ((_fence & Square.Create(r, f)).IsEmpty)
                {
                    var pawnAttacks = sq.PawnAttack(_us);
                    if (_theirPawns & pawnAttacks)
                        return false;

                    if (_ourPawns & sq)
                        break;

                    sq += up;
                    r = sq.Rank;
                }

                if (_pos.IsOccupied(sq) || (_fence & Square.Create(r, f)).IsEmpty)
                    continue;

                if (rr >= Ranks.Rank6)
                    return false;

                if ((_theirPawns & (sq + _us.PawnDoublePushDistance())).IsEmpty)
                {
                    if (theirKsq.File != f || theirKsq.RelativeRank(_us) < rr)
                        return false;

                    if (f != File.FileA)
                    {
                        if (_pos.GetPiece(sq + Direction.West) != _ourPawn)
                            return false;

                        if (BitBoards.PopCount(_ourPawns & (f - 1)) > 1)
                            return false;

                        if ((_fixedPawn & Square.Create(r, PreviousFile(f))).IsEmpty)
                            return false;

                        if ((_fence & Square.Create(r, PreviousFile(f))).IsEmpty)
                            return false;
                    }

                    if (f != File.FileH)
                    {
                        if (_pos.GetPiece(sq + Direction.East) != _ourPawn)
                            return false;

                        if (BitBoards.PopCount(_ourPawns & (f + 1)) > 1)
                            return false;

                        if ((_fixedPawn & Square.Create(r, NextFile(f))).IsEmpty)
                            return false;

                        if ((_fence & Square.Create(r, NextFile(f))).IsEmpty)
                            return false;
                    }
                }

                if ((sq + up).PawnAttack(_us) & _theirPawns)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Computes the fence ranks
    /// </summary>
    private void ComputeFenceRanks()
    {
        var covered = _fence;

        while (covered)
        {
            var sq = BitBoards.PopLsb(ref covered);
            var (r, f) = sq;
            _fenceRank[f.AsInt()] = r;
        }
    }

    /// <summary>
    /// Marks the current players pawns as either fixed and marked or dynamic
    /// </summary>
    /// <param name="up">The up direction for the current player</param>
    private void MarkOurPawns(Direction up)
    {
        var ourPawns = _ourPawns;

        while (ourPawns)
        {
            var psq = BitBoards.PopLsb(ref ourPawns);
            var rr = psq.RelativeRank(_us);
            if (rr < Rank.Rank7
                && (_pos.GetPiece(psq + up) == _theirPawn || !(_fixedPawn & (psq + up)).IsEmpty)
                && (psq.PawnAttack(_us) & _theirPawns).IsEmpty)
            {
                _fixedPawn |= psq;
                _marked |= psq;
            }
            else
                _dynamicPawns |= psq;
        }
    }

    /// <summary>
    /// Marks the opponent pawn attacks
    /// </summary>
    private void MarkTheirPawns()
    {
        var (southEast, southWest) = _us.IsWhite
            ? (Direction.SouthEast, Direction.SouthWest)
            : (Direction.NorthEast, Direction.NorthWest);

        _marked |= _theirPawns.Shift(southEast) | _theirPawns.Shift(southWest);
    }

    /// <summary>
    /// Determines which squares forms a fence. First square is always on file A - and will
    /// perform a depth first verification of its surrounding squares.
    /// </summary>
    /// <param name="sq">The square which is currently being looked at</param>
    /// <returns>true if the square is in the fence</returns>
    private bool FormsFence(Square sq)
    {
        _processed |= sq;

        // File H is marked as fence if it is reached.
        if (sq.File == File.FileH)
        {
            _fence |= sq;
            return true;
        }

        Span<Direction> directions = stackalloc Direction[]
            { _us.PawnPushDistance(), Directions.East, _them.PawnPushDistance() };

        foreach (var direction in directions)
        {
            var s = sq + direction;
            if ((_marked & s).IsEmpty || !(_processed & s).IsEmpty || !FormsFence(s))
                continue;
            _fence |= s;
            return true;
        }

        return false;
    }

    private Square NextFenceRankSquare(File f, Player them)
        => new Square(_fenceRank[f.AsInt()].AsInt() * 8 + f.AsInt()) + them.PawnPushDistance();

    private bool IsFenceFormed()
    {
        var bb = PawnFileASquares;
        while (bb)
        {
            var startSquare = BitBoards.PopLsb(ref bb);
            if ((_marked & startSquare).IsEmpty || !FormsFence(startSquare))
                continue;
            _fence |= startSquare;
            return true;
        }

        return false;
    }

    private BitBoard ComputeDynamicFencedPawns()
    {
        // reverse order of Down
        var down = _us.PawnPushDistance();

        var result = BitBoard.Empty;

        foreach (var f in File.AllFiles)
        {
            var sq = NextFenceRankSquare(f, _them);
            var b = sq.ForwardFile(_them) & _theirPawns;
            while (b)
            {
                sq = BitBoards.PopLsb(ref b) + down;
                if (_pos.GetPiece(sq) == _ourPawn)
                    result |= sq;
            }
        }

        return result;
    }

    private static File NextFile(File f)
        => f + 1;

    private static File PreviousFile(File f)
        => f - 1;
}