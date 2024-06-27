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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib;

public sealed class Game(
    ITranspositionTable transpositionTable,
    IUci uci,
    ICpu cpu,
    ISearchParameters searchParameters,
    IPosition pos,
    ObjectPool<MoveList> moveListPool)
    : IGame
{
    private readonly PertT _perftTable = new(8);

    public Action<IPieceSquare> PieceUpdated => pos.PieceUpdated;

    public int MoveNumber => 1 + (Pos.Ply - Pos.SideToMove.IsBlack.AsByte() / 2);

    public BitBoard Occupied => Pos.Pieces();

    public IPosition Pos => pos;

    public GameEndTypes GameEndType { get; set; }

    public ITranspositionTable Table { get; } = transpositionTable;

    public ISearchParameters SearchParameters { get; } = searchParameters;

    public IUci Uci { get; } = uci;

    public ICpu Cpu { get; } = cpu;

    public bool IsRepetition => pos.IsRepetition;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NewGame(string fen = Fen.Fen.StartPositionFen)
    {
        var fenData = new FenData(fen);
        var state = new State();
        pos.Set(in fenData, ChessMode.Normal, state, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FenData GetFen() => pos.GenerateFen();

    public void UpdateDrawTypes()
    {
        var gameEndType = GameEndTypes.None;
        if (IsRepetition)
            gameEndType |= GameEndTypes.Repetition;
        if (Pos.Rule50 >= 100)
            gameEndType |= GameEndTypes.FiftyMove;

        var moveList = moveListPool.Get();
        moveList.Generate(in pos);

        var moves = moveList.Get();

        if (moves.IsEmpty)
            gameEndType |= GameEndTypes.Pat;

        moveListPool.Return(moveList);

        GameEndType = gameEndType;
    }

    public override string ToString()
    {
        return pos.ToString() ?? string.Empty;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Piece> GetEnumerator() => pos.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard OccupiedBySide(Color c) => pos.Pieces(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color CurrentPlayer() => pos.SideToMove;

    public UInt128 Perft(in HashKey baseKey, int depth, bool root = true)
    {
        var tot = UInt128.MinValue;
        var ml = moveListPool.Get();
        ml.Generate(in pos);
        var moves = ml.Get();

        if (root && depth <= 1)
        {
            tot = (ulong)moves.Length;
            moveListPool.Return(ml);
            return tot;
        }

        ref var movesSpace = ref MemoryMarshal.GetReference(moves);

        for (var i = 0; i < moves.Length; ++i)
        {
            var m = Unsafe.Add(ref movesSpace, i).Move;
            var state = new State();

            pos.MakeMove(m, in state);

            if (depth <= 2)
            {
                var ml2 = moveListPool.Get();
                ml2.Generate(in pos);
                tot += (ulong)ml2.Length;
                moveListPool.Return(ml2);
            }
            else
            {
                var next = Perft(in baseKey, depth - 1, false);
                tot += next;
            }

            pos.TakeMove(m);
        }

        moveListPool.Return(ml);

        return tot;
    }
}