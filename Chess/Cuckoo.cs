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

using Hash;
using System;
using System.Diagnostics;
using Types;

/// <summary>
/// Marcel van Kervinck's cuckoo algorithm for fast detection of "upcoming repetition"
/// situations. https://marcelk.net/2013-04-06/paper/upcoming-rep-v2.pdf
/// TODO : Unit tests
/// </summary>
public static class Cuckoo
{
    private static readonly HashKey[] CuckooKeys = new HashKey[8192];
    private static readonly Move[] CuckooMoves = new Move[8192];

    static Cuckoo()
    {
        Span<Piece> pieces = stackalloc Piece[]
        {
            Enums.Pieces.WhitePawn,
            Enums.Pieces.WhiteKnight,
            Enums.Pieces.WhiteBishop,
            Enums.Pieces.WhiteRook,
            Enums.Pieces.WhiteQueen,
            Enums.Pieces.WhiteKing,
            Enums.Pieces.BlackPawn,
            Enums.Pieces.BlackKnight,
            Enums.Pieces.BlackBishop,
            Enums.Pieces.BlackRook,
            Enums.Pieces.BlackQueen,
            Enums.Pieces.BlackKing
        };

        var count = 0;
        foreach (var pc in pieces)
        {
            foreach (var sq1 in BitBoards.AllSquares)
            {
                for (var sq2 = sq1 + 1; sq2 <= Enums.Squares.h8; ++sq2)
                {
                    if ((pc.Type().PseudoAttacks(sq1) & sq2).IsEmpty)
                        continue;

                    var move = Move.Create(sq1, sq2);
                    HashKey key = pc.GetZobristPst(sq1) ^ pc.GetZobristPst(sq2) ^ Zobrist.GetZobristSide();
                    var i = CuckooHashOne(key);
                    while (true)
                    {
                        (CuckooKeys[i], key) = (key, CuckooKeys[i].Key);
                        (CuckooMoves[i], move) = (move, CuckooMoves[i]);

                        // check for empty slot
                        if (move.IsNullMove())
                            break;

                        // Push victim to alternative slot
                        i = i == CuckooHashOne(key)
                            ? CuckooHashTwo(key)
                            : CuckooHashOne(key);
                    }

                    count++;
                }
            }
        }

        Debug.Assert(count == 3668);
    }

    public static bool HashCuckooCycle(IPosition pos, int end, int ply)
    {
        var state = pos.State;
        var originalKey = state.Key;
        var statePrevious = state.Previous;

        for (var i = 3; i <= end; i += 2)
        {
            statePrevious = statePrevious.Previous.Previous;
            var moveKey = originalKey ^ statePrevious.Key;

            var j = CuckooHashOne(moveKey);
            var found = CuckooKeys[j] == moveKey;

            if (!found)
            {
                j = CuckooHashTwo(moveKey);
                found = CuckooKeys[j] == moveKey;
            }

            if (!found)
                continue;

            var move = CuckooMoves[j];
            var s1 = move.FromSquare();
            var s2 = move.ToSquare();

            if ((s1.BitboardBetween(s2) & pos.Board.Pieces()).IsEmpty)
                continue;

            if (ply > i)
                return true;

            // For nodes before or at the root, check that the move is a repetition rather than
            // a move to the current position. In the cuckoo table, both moves Rc1c5 and Rc5c1
            // are stored in the same location, so we have to select which square to check.
            if (pos.GetPiece(!pos.IsOccupied(s1) ? s2 : s1).ColorOf() != pos.SideToMove)
                continue;

            // For repetitions before or at the root, require one more
            if (statePrevious.Repetition > 0)
                return true;
        }

        return false;
    }

    private static int CuckooHashOne(in HashKey key)
        => (int)(key.Key & 0x1FFF);

    private static int CuckooHashTwo(in HashKey key)
        => (int)((key.Key >> 16) & 0x1FFF);
}
