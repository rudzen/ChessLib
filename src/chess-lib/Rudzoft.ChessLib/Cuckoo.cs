﻿/*
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

using System.Diagnostics;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib;

/// <summary>
/// Marcel van Kervinck's cuckoo algorithm for fast detection of "upcoming repetition"
/// situations. https://marcelk.net/2013-04-06/paper/upcoming-rep-v2.pdf
/// TODO : Unit tests
/// </summary>
public sealed class Cuckoo : ICuckoo
{
    private readonly HashKey[] _cuckooKeys;
    private readonly Move[]    _cuckooMoves;

    public Cuckoo(IZobrist zobrist)
    {
        _cuckooKeys  = new HashKey[8192];
        _cuckooMoves = new Move[8192];
        var count = 0;

        foreach (var pc in Piece.All.AsSpan())
        {
            var bb = BitBoards.AllSquares;
            while (bb)
            {
                var sq1 = BitBoards.PopLsb(ref bb);
                for (var sq2 = sq1 + 1; sq2 <= Square.H8; ++sq2)
                {
                    if (!pc.Type().PseudoAttacks(sq1).Contains(sq2))
                        continue;

                    var move = Move.Create(sq1, sq2);
                    var key  = zobrist.Psq(sq1, pc) ^ zobrist.Psq(sq2, pc) ^ zobrist.Side();
                    var j    = CuckooHashOne(in key);
                    do
                    {
                        (_cuckooKeys[j], key)   = (key, _cuckooKeys[j]);
                        (_cuckooMoves[j], move) = (move, _cuckooMoves[j]);

                        // check for empty slot
                        if (move.IsNullMove())
                            break;

                        // Push victim to alternative slot
                        j = j == CuckooHashOne(in key)
                            ? CuckooHashTwo(in key)
                            : CuckooHashOne(in key);
                    } while (true);

                    count++;
                }
            }
        }

        Debug.Assert(count == 3668);
    }

    public bool HashCuckooCycle(in IPosition pos, int end, int ply)
    {
        if (end < 3)
            return false;

        var state         = pos.State;
        var originalKey   = state.PositionKey;
        var statePrevious = state.Previous;

        for (var i = 3; i <= end; i += 2)
        {
            statePrevious = statePrevious.Previous.Previous;
            var moveKey = originalKey ^ statePrevious.PositionKey;

            var j     = CuckooHashOne(in moveKey);
            var found = _cuckooKeys[j] == moveKey;

            if (!found)
            {
                j     = CuckooHashTwo(in moveKey);
                found = _cuckooKeys[j] == moveKey;
            }

            if (!found)
                continue;

            var (s1, s2) = _cuckooMoves[j];

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

    private static int CuckooHashOne(in HashKey key) => (int)(key.Key & 0x1FFF);

    private static int CuckooHashTwo(in HashKey key) => (int)((key.Key >> 16) & 0x1FFF);
}