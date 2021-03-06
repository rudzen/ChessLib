using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Chess.Test")]

namespace Rudz.Chess
{
    using Enums;
    using System;
    using Types;

    /// <summary>
    /// Computes pawn blockage (fences) See https://pdfs.semanticscholar.org/31c2/d37c80ea1aef0676ba30393bc46c0ccc70e9.pdf
    /// </summary>
    public sealed class Blockage : IBlockage
    {
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

        public Blockage(IPosition pos)
        {
            _pos = pos;
            _fenceRank = new Rank[(int)Files.FileNb];
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
            var theirKsq = _pos.GetKingSquare(_them);

            if (ourKsq.Rank.RelativeRank(_us) > _fenceRank[ourKsq.File.AsInt()].RelativeRank(_us))
                return false;

            _dynamicPawns |= ComputeDynamicFencedPawns(_them);

            while (_dynamicPawns)
            {
                var sq = BitBoards.PopLsb(ref _dynamicPawns);
                var (r, f) = sq.RankFile;
                var rr = sq.RelativeRank(_us);

                if (r > _fenceRank[f.AsInt()])
                {
                    if ((_theirPawns & sq.PassedPawnFrontAttackSpan(_us)).IsEmpty && (theirKsq.File != f || theirKsq.Rank.RelativeRank(_us) < rr))
                        return false;
                }
                else if (_fence & sq)
                {
                    if (rr >= Ranks.Rank6)
                        return false;

                    if (_pos.GetPiece(sq + _us.PawnDoublePushDistance()) != _theirPawn)
                    {
                        if (theirKsq.File != f || theirKsq.Rank.RelativeRank(_us) < rr)
                            return false;

                        if (f != Files.FileA
                            && _pos.GetPiece(sq + Directions.West) != _ourPawn
                            || BitBoards.PopCount(_ourPawns & PreviousFile(f)) > 1
                            || (_fixedPawn & (sq + Directions.West)).IsEmpty
                            || (_fence & (sq + Directions.West)).IsEmpty)
                            return false;
                        if (f != Files.FileH
                            && _pos.GetPiece(sq + Directions.East) != _ourPawn
                            || BitBoards.PopCount(_ourPawns & NextFile(f)) > 1
                            || (_fixedPawn & (sq + Directions.East)).IsEmpty
                            || (_fence & (sq + Directions.East)).IsEmpty)
                            return false;
                    }

                    if ((sq + _us.PawnDoublePushDistance()).PawnAttack(_us) & _theirPawns)
                        return false;

                    if (BitBoards.PopCount(_ourPawns & f) > 1)
                        return false;
                }
                else if (r < _fenceRank[f.AsInt()])
                {
                    sq += up;
                    r = sq.Rank;
                    rr = sq.RelativeRank(_us);
                    while ((_fence & Square.Make(r, f)).IsEmpty)
                    {
                        var pawnAttacks = sq.PawnAttack(_us);
                        if (_theirPawns & pawnAttacks)
                            return false;

                        if (_pos.GetPiece(sq) == _ourPawn)
                            break;

                        sq += up;
                        r = sq.Rank;
                    }

                    if ((_fence & Square.Make(r, f)).IsEmpty || _pos.IsOccupied(sq))
                        continue;

                    if (rr >= Ranks.Rank6)
                        return false;

                    if ((_theirPawns & (sq + _us.PawnDoublePushDistance())).IsEmpty)
                    {
                        if (theirKsq.File != f || theirKsq.Rank.RelativeRank(_us) < rr)
                            return false;

                        if (f != Files.FileA
                            && _pos.GetPiece(sq + Directions.West) != _ourPawn
                            || BitBoards.PopCount(_ourPawns & (f - 1)) > 1
                            || (_fixedPawn & Square.Make(r, PreviousFile(f))).IsEmpty
                            || (_fence & Square.Make(r, PreviousFile(f))).IsEmpty)
                            return false;
                        if (f != Files.FileH
                            && _pos.GetPiece(sq + Directions.East) != _ourPawn
                            || BitBoards.PopCount(_ourPawns & (f + 1)) > 1
                            || (_fixedPawn & Square.Make(r, NextFile(f))).IsEmpty
                            || (_fence & Square.Make(r, NextFile(f))).IsEmpty)
                            return false;
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
                var (r, f) = sq.RankFile;
                _fenceRank[f.AsInt()] = r;
            }
        }

        /// <summary>
        /// Marks the current players pawns as either fixed and marked or dynamic
        /// </summary>
        /// <param name="up">The up direction for the current player</param>
        private void MarkOurPawns(Direction up)
        {
            var ourPawns = _pos.Board.Squares(PieceTypes.Pawn, _us);

            foreach (var psq in ourPawns)
            {
                var rr = psq.RelativeRank(_us);
                if (rr < Ranks.Rank7
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
                ? (Directions.SouthEast, Directions.SouthWest)
                : (Directions.NorthEast, Directions.NorthWest);

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
            if (sq.File == Files.FileH)
            {
                _fence |= sq;
                return true;
            }

            Span<Direction> directions = stackalloc Direction[] { _us.PawnPushDistance(), Directions.East, _them.PawnPushDistance() };

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
            for (Rank rank = Ranks.Rank2; rank < Ranks.Rank8; ++rank)
            {
                var startSquare = Square.Make(rank, Files.FileA);
                if ((_marked & startSquare).IsEmpty || !FormsFence(startSquare))
                    continue;
                _fence |= startSquare;
                return true;
            }

            return false;
        }

        private BitBoard ComputeDynamicFencedPawns(Player them)
        {
            // reverse order of Down
            var down = them.IsBlack
                ? Directions.South
                : Directions.North;

            var result = BitBoard.Empty;

            for (File f = Files.FileA; f < Files.FileNb; ++f)
            {
                var sq = NextFenceRankSquare(f, them);
                var b = sq.ForwardFile(them) & _theirPawns;
                while (!b.IsEmpty)
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
}