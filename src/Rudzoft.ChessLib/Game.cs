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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib;

public sealed class Game : IGame
{
    private readonly ObjectPool<IMoveList> _moveListPool;
    private readonly IPosition _pos;

    public Game(
        ITranspositionTable transpositionTable,
        IUci uci,
        ISearchParameters searchParameters,
        IPosition pos,
        ObjectPool<IMoveList> moveListPoolPool)
    {
        _moveListPool = moveListPoolPool;
        _pos = pos;
        
        Table = transpositionTable;
        SearchParameters = searchParameters;
        Uci = uci;
    }

    public Action<IPieceSquare> PieceUpdated => _pos.PieceUpdated;

    public int MoveNumber => 0; //(PositionIndex - 1) / 2 + 1;

    public BitBoard Occupied => Pos.Pieces();

    public IPosition Pos => _pos;

    public GameEndTypes GameEndType { get; set; }

    public ITranspositionTable Table { get; }

    public ISearchParameters SearchParameters { get; }

    public IUci Uci { get; }

    public bool IsRepetition => _pos.IsRepetition;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NewGame(string fen = Fen.Fen.StartPositionFen)
    {
        var fenData = new FenData(fen);
        var state = new State();
        _pos.Set(in fenData, ChessMode.Normal, state, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FenData GetFen() => _pos.GenerateFen();

    public void UpdateDrawTypes()
    {
        var gameEndType = GameEndTypes.None;
        if (IsRepetition)
            gameEndType |= GameEndTypes.Repetition;
        if (Pos.Rule50 >= 100)
            gameEndType |= GameEndTypes.FiftyMove;

        var moveList = _moveListPool.Get();
        moveList.Generate(in _pos);

        var moves = moveList.Get();
        ref var movesSpace = ref MemoryMarshal.GetReference(moves);
        for (var i = 0; i < moves.Length; ++i)
        {
            var em = Unsafe.Add(ref movesSpace, i);
            if (Pos.IsLegal(em.Move))
                continue;
            gameEndType |= GameEndTypes.Pat;
            break;
        }

        GameEndType = gameEndType;
    }

    public override string ToString()
    {
        return _pos.ToString() ?? string.Empty;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Piece> GetEnumerator() => _pos.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard OccupiedBySide(Player p) => _pos.Pieces(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Player CurrentPlayer() => _pos.SideToMove;

    public ulong Perft(int depth, bool root = true)
    {
        var tot = ulong.MinValue;

        var ml = _moveListPool.Get();
        ml.Generate(in _pos);
        var state = new State();

        var moves = ml.Get();
        ref var movesSpace = ref MemoryMarshal.GetReference(moves);
        for (var i = 0; i < moves.Length; ++i)
        {
            var em = Unsafe.Add(ref movesSpace, i);
            if (root && depth <= 1)
                tot++;
            else
            {
                var m = em.Move;
                _pos.MakeMove(m, in state);

                if (depth <= 2)
                {
                    var ml2 = _moveListPool.Get();
                    ml2.Generate(in _pos);
                    tot += (ulong)ml2.Length;
                    _moveListPool.Return(ml2);
                }
                else
                    tot += Perft(depth - 1, false);

                _pos.TakeMove(m);
            }
        }

        _moveListPool.Return(ml);

        return tot;
    }
}