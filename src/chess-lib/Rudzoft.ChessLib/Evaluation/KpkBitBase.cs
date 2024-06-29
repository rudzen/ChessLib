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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;
using File = Rudzoft.ChessLib.Types.File;

namespace Rudzoft.ChessLib.Evaluation;

/// <summary>
/// Based on bit base in Stockfish.
/// However, some modifications are done to be slightly less confusing.
/// </summary>
public sealed class KpkBitBase : IKpkBitBase
{
    // There are 24 possible pawn squares: files A to D and ranks from 2 to 7.
    // Positions with the pawn on files E to H will be mirrored before probing.
    private const int MaxIndex = 2 * 24 * 64 * 64; // stm * psq * wksq * bksq = 196608

    private readonly BitArray _kpKbb = new(MaxIndex);

    public KpkBitBase()
    {
        // Initialize db with known win / draw positions
        var db = Enumerable
            .Range(0, MaxIndex)
            .Select(KpkPosition.Create)
            .ToArray()
            .AsSpan();

        // Iterate through the positions until none of the unknown positions can be
        // changed to either wins or draws (15 cycles needed).
        int repeat;

        ref var dbSpace = ref MemoryMarshal.GetReference(db);

        do
        {
            repeat = 1;
            for (var i = 0; i < db.Length; ++i)
            {
                ref var kpkPosition = ref Unsafe.Add(ref dbSpace, i);
                repeat = (kpkPosition.Result == Results.Unknown
                          && kpkPosition.Classify(db) != Results.Unknown).AsByte();
            }
        } while (repeat != 0);

        // Fill the bit base with the decisive results
        for (var i = 0; i < db.Length; ++i)
        {
            ref var kpkPosition = ref Unsafe.Add(ref dbSpace, i);
            if (kpkPosition.Result == Results.Win)
                _kpKbb.Set(i, true);
        }
    }

    /// <summary>
    /// A KPK bitbase index is an integer in [0,IndexMax] range
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
    /// <param name="weakKingSq"></param>
    /// <param name="strongKsq"></param>
    /// <param name="strongPawnSq"></param>
    /// <returns></returns>
    private static int Index(Color stm, Square weakKingSq, Square strongKsq, Square strongPawnSq)
    {
        return strongKsq
               | (weakKingSq << 6)
               | (stm << 12)
               | (strongPawnSq.File << 13)
               | ((Rank.Rank7 - strongPawnSq.Rank) << 15);
    }

    [Flags]
    private enum Results
    {
        None = 0,
        Unknown = 1,
        Draw = 2,
        Win = 4
    }

    private readonly record struct KpkPosition(Results Result, Color Stm, Square[] KingSquares, Square PawnSquare)
    {
        public static KpkPosition Create(int idx)
        {
            Results results;
            var stm = new Color((idx >> 12) & 0x01);
            var ksq = new Square[] { (idx >> 0) & 0x3F, (idx >> 6) & 0x3F };
            var psq = Square.Create(new(Ranks.Rank7.AsInt() - ((idx >> 15) & 0x7)), new((idx >> 13) & 0x3));

            // Invalid if two pieces are on the same square or if a king can be captured
            if (ksq[Color.White].Distance(ksq[Color.Black]) <= 1
                || ksq[Color.White] == psq
                || ksq[Color.Black] == psq
                || (stm.IsWhite && (psq.PawnAttack(Color.White) & ksq[Color.Black]).IsNotEmpty))
                results = Results.None;

            // Win if the pawn can be promoted without getting captured
            else if (stm.IsWhite
                     && psq.Rank == Ranks.Rank7
                     && ksq[Color.White] != psq + Directions.North
                     && (ksq[Color.Black].Distance(psq + Directions.North) > 1
                         || ksq[Color.White].Distance(psq + Directions.North) == 1))
                results = Results.Win;

            // Draw if it is stalemate or the black king can capture the pawn
            else if (stm.IsBlack
                     && ((PieceType.King.PseudoAttacks(ksq[Color.Black]) &
                          ~(PieceType.King.PseudoAttacks(ksq[Color.White]) | psq.PawnAttack(Color.White)))
                         .IsEmpty
                         || !(PieceType.King.PseudoAttacks(ksq[Color.Black]) &
                              ~PieceType.King.PseudoAttacks(ksq[Color.White]) & psq).IsEmpty))
                results = Results.Draw;

            // Position will be classified later in initialization
            else
                results = Results.Unknown;

            return new(
                Result: results,
                Stm: stm,
                KingSquares: ksq,
                PawnSquare: psq);
        }

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
        public Results Classify(ReadOnlySpan<KpkPosition> db)
        {
            var (good, bad) = InitResults();
            var r = GetInitialKingResults(db);

            if (Stm.IsWhite)
            {
                // Single push
                if (PawnSquare.Rank < Rank.Rank7)
                    r |= db[
                        Index(Color.Black, KingSquares[Color.Black], KingSquares[Color.White],
                            PawnSquare + Directions.North)].Result;

                // Double push
                if (PawnSquare.Rank == Rank.Rank2
                    && PawnSquare + Directions.North != KingSquares[Color.White]
                    && PawnSquare + Directions.North != KingSquares[Color.Black])
                    r |= db[
                        Index(Color.Black, KingSquares[Color.Black], KingSquares[Color.White],
                            PawnSquare + Directions.NorthDouble)].Result;
            }

            if ((r & good) != 0)
                return good;

            return (r & Results.Unknown) != 0
                ? Results.Unknown
                : bad;
        }

        private (Results, Results) InitResults()
        {
            return Stm.IsWhite
                ? (Results.Win, Results.Draw)
                : (Results.Draw, Results.Win);
        }

        private Results GetInitialKingResults(ReadOnlySpan<KpkPosition> db)
        {
            var r = Results.None;
            var b = PieceType.King.PseudoAttacks(KingSquares[Stm]);

            while (b)
            {
                var (bkSq, wkSq) = Stm.IsWhite
                    ? (KingSquares[Color.Black], BitBoards.PopLsb(ref b))
                    : (BitBoards.PopLsb(ref b), KingSquares[Color.White]);
                r |= db[Index(~Stm, bkSq, wkSq, PawnSquare)].Result;
            }

            return r;
        }
    }

    /// <inheritdoc />
    public Square Normalize(IPosition pos, Color strongSide, Square sq)
    {
        Debug.Assert(pos.Board.PieceCount(PieceType.Pawn, strongSide) == 1);

        if (pos.GetPieceSquare(PieceType.Pawn, strongSide).File >= File.FileE)
            sq = sq.FlipFile();

        return strongSide.IsWhite ? sq : sq.FlipRank();
    }

    /// <inheritdoc />
    public bool Probe(Square strongKsq, Square strongPawnSq, Square weakKsq, Color stm)
    {
        Debug.Assert(strongPawnSq.File <= File.FileD);
        return _kpKbb[Index(stm, weakKsq, strongKsq, strongPawnSq)];
    }

    /// <inheritdoc />
    public bool Probe(bool strongActive, Square skSq, Square wkSq, Square spSq)
    {
        return _kpKbb[Index(strongActive, skSq, wkSq, spSq)];
    }

    public bool IsDraw(IPosition pos)
    {
        return !Probe(
            strongKsq: pos.GetPieceSquare(PieceType.King, Color.White),
            strongPawnSq: pos.Pieces(PieceType.Pawn).Lsb(),
            weakKsq: pos.GetPieceSquare(PieceType.King, Color.Black),
            stm: pos.SideToMove);
    }
}