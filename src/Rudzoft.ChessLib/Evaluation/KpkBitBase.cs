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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Evaluation;

/// <summary>
/// Based on bit base in Stockfish.
/// However, some modifications are done to be slightly less confusing.
/// </summary>
public static class KpkBitBase
{
    // There are 24 possible pawn squares: files A to D and ranks from 2 to 7.
    // Positions with the pawn on files E to H will be mirrored before probing.
    private const int MaxIndex = 2 * 24 * 64 * 64; // stm * psq * wksq * bksq = 196608

    private static readonly BitArray KpKbb = new(MaxIndex);

    static KpkBitBase()
    {
        List<KpkPosition> db = new(MaxIndex);
        int idx;

        // Initialize db with known win / draw positions
        for (idx = 0; idx < MaxIndex; ++idx)
            db.Add(KpkPosition.Create(idx));

        // Iterate through the positions until none of the unknown positions can be
        // changed to either wins or draws (15 cycles needed).
        var repeat = 1;

        var dbs = CollectionsMarshal.AsSpan(db);
        while (repeat != 0)
        {
            repeat = 1;
            foreach (var kpkPosition in dbs)
                repeat = (kpkPosition.Result == Result.Unknown && kpkPosition.Classify(db) != Result.Unknown).AsByte();
        }

        // Fill the bitbase with the decisive results
        idx = 0;
        foreach (var kpkPosition in dbs)
        {
            if (kpkPosition.Result == Result.Win)
                KpKbb.Set(idx, true);
            idx++;
        }
    }

    /// <summary>
    /// A KPK bitbase index is an integer in [0, IndexMax] range
    ///
    /// Information is mapped in a way that minimizes the number of iterations:
    ///
    /// bit  0- 5: white king square (from SQ_A1 to SQ_H8)
    /// bit  6-11: black king square (from SQ_A1 to SQ_H8)
    /// bit    12: side to move (WHITE or BLACK)
    /// bit 13-14: white pawn file (from FILE_A to FILE_D)
    /// bit 15-17: white pawn RANK_7 - rank (from RANK_7 - RANK_7 to RANK_7 - RANK_2)
    /// </summary>
    /// <param name="stm"></param>
    /// <param name="blackKsq"></param>
    /// <param name="whiteKsq"></param>
    /// <param name="pawnSq"></param>
    /// <returns></returns>
    private static int Index(Player stm, Square blackKsq, Square whiteKsq, Square pawnSq)
    {
        return whiteKsq.AsInt()
               | (blackKsq.AsInt() << 6)
               | (stm << 12)
               | (pawnSq.File.AsInt() << 13)
               | ((Ranks.Rank7.AsInt() - pawnSq.Rank.AsInt()) << 15);
    }

    [Flags]
    private enum Result
    {
        None = 0,
        Unknown = 1 << 0,
        Draw = 1 << 1,
        Win = 1 << 2
    }

    private struct KpkPosition
    {
        private KpkPosition(
            Result result,
            Player stm,
            Square[] kingSquares,
            Square pawnSquare)
        {
            Result = result;
            Stm = stm;
            KingSquares = kingSquares;
            PawnSquare = pawnSquare;
        }

        public static KpkPosition Create(int idx)
        {
            Result result;
            var stm = new Player((idx >> 12) & 0x01);
            var ksq = new Square[] { (idx >> 0) & 0x3F, (idx >> 6) & 0x3F };
            var psq = Square.Create(new Rank(Ranks.Rank7.AsInt() - ((idx >> 15) & 0x7)), new File((idx >> 13) & 0x3));

            // Invalid if two pieces are on the same square or if a king can be captured
            if (ksq[Player.White.Side].Distance(ksq[Player.Black.Side]) <= 1
                || ksq[Player.White.Side] == psq
                || ksq[Player.Black.Side] == psq
                || (stm.IsWhite && !(psq.PawnAttack(Player.White) & ksq[Player.Black.Side]).IsEmpty))
                result = Result.None;

            // Win if the pawn can be promoted without getting captured
            else if (stm.IsWhite
                     && psq.Rank == Ranks.Rank7
                     && ksq[Player.White.Side] != psq + Directions.North
                     && (ksq[Player.Black.Side].Distance(psq + Directions.North) > 1
                         || ksq[Player.White.Side].Distance(psq + Directions.North) == 1))
                result = Result.Win;

            // Draw if it is stalemate or the black king can capture the pawn
            else if (stm.IsBlack
                     && ((PieceTypes.King.PseudoAttacks(ksq[Player.Black.Side]) &
                          ~(PieceTypes.King.PseudoAttacks(ksq[Player.White.Side]) | psq.PawnAttack(Player.White)))
                         .IsEmpty
                         || !(PieceTypes.King.PseudoAttacks(ksq[Player.Black.Side]) &
                              ~PieceTypes.King.PseudoAttacks(ksq[Player.White.Side]) & psq).IsEmpty))
                result = Result.Draw;

            // Position will be classified later in initialization
            else
                result = Result.Unknown;

            return new KpkPosition(
                result,
                stm,
                ksq,
                psq);
        }

        public Result Result { get; private set; }
        private Player Stm { get; }
        private Square[] KingSquares { get; }
        private Square PawnSquare { get; }

        /// <summary>
        /// White to move: If one move leads to a position classified as WIN, the result
        /// of the current position is WIN. If all moves lead to positions classified
        /// as DRAW, the current position is classified as DRAW, otherwise the current
        /// position is classified as UNKNOWN.
        ///
        /// Black to move: If one move leads to a position classified as DRAW, the result
        /// of the current position is DRAW. If all moves lead to positions classified
        /// as WIN, the position is classified as WIN, otherwise the current position is
        /// classified as UNKNOWN.
        /// </summary>
        /// <param name="db">Current KpkPositions as list</param>
        /// <returns>Result after classification</returns>
        public Result Classify(IList<KpkPosition> db)
        {
            var (good, bad) = Stm.IsWhite
                ? (Result.Win, Result.Draw)
                : (Result.Draw, Result.Win);

            var r = Result.None;
            var b = PieceTypes.King.PseudoAttacks(KingSquares[Stm.Side]);

            while (b)
            {
                var (bksq, wksq) = Stm.IsWhite
                    ? (KingSquares[Player.Black.Side], BitBoards.PopLsb(ref b))
                    : (BitBoards.PopLsb(ref b), KingSquares[Player.White.Side]);
                r |= db[Index(~Stm, bksq, wksq, PawnSquare)].Result;
            }

            if (Stm.IsWhite)
            {
                // Single push
                if (PawnSquare.Rank < Ranks.Rank7)
                    r |= db[
                        Index(Player.Black, KingSquares[Player.Black.Side], KingSquares[Player.White.Side],
                            PawnSquare + Directions.North)].Result;

                // Double push
                if (PawnSquare.Rank == Ranks.Rank2
                    && PawnSquare + Directions.North != KingSquares[Player.White.Side]
                    && PawnSquare + Directions.North != KingSquares[Player.Black.Side])
                    r |= db[
                        Index(Player.Black, KingSquares[Player.Black.Side], KingSquares[Player.White.Side],
                            PawnSquare + Directions.NorthDouble)].Result;
            }

            if ((r & good) != 0)
                return good;

            return (r & Result.Unknown) != 0
                    ? Result.Unknown
                    : bad;
        }
    }

    /// <summary>
    /// Normalizes a square in accordance with the data.
    /// Kpk bit-base is only used for king pawn king positions!
    /// </summary>
    /// <param name="pos">Current position</param>
    /// <param name="strongSide">The strong side (the one with the pawn)</param>
    /// <param name="sq">The square that needs to be normalized</param>
    /// <returns>Normalized square to be used with probing</returns>
    public static Square Normalize(IPosition pos, Player strongSide, Square sq)
    {
        Debug.Assert(pos.Board.PieceCount(PieceTypes.Pawn, strongSide) == 1);

        if (pos.GetPieceSquare(PieceTypes.Pawn, strongSide).File >= Files.FileE)
            sq = sq.FlipFile();

        return strongSide.IsWhite ? sq : sq.FlipRank();
    }

    /// <summary>
    /// Probe with normalized squares and strong player
    /// </summary>
    /// <param name="whiteKsq">"Strong" side king square</param>
    /// <param name="pawnSq">"Strong" side pawn square</param>
    /// <param name="blackKsq">"Weak" side king square</param>
    /// <param name="stm">strong side. fx strongSide == pos.SideToMove ? Player.White : Player.Black</param>
    /// <returns>true if strong side "won"</returns>
    public static bool Probe(Square whiteKsq, Square pawnSq, Square blackKsq, Player stm)
    {
        Debug.Assert(pawnSq.File <= File.FileD);

        return KpKbb[Index(stm, blackKsq, whiteKsq, pawnSq)];
    }

    public static bool IsDraw(IPosition pos)
    {
        return !Probe(
            pos.GetPieceSquare(PieceTypes.King, Player.White),
            pos.Pieces(PieceTypes.Pawn).Lsb(),
            pos.GetPieceSquare(PieceTypes.King, Player.Black),
            pos.SideToMove);
    }
}