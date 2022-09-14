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
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib;

public sealed class Game : IGame
{
    private readonly ObjectPool<IMoveList> _moveLists;
    private readonly IPosition _pos;

    public Game(IPosition pos)
    {
        _pos = pos;
        _moveLists = new DefaultObjectPool<IMoveList>(new MoveListPolicy());
        SearchParameters = new SearchParameters();
    }

    static Game()
    {
        Table = new TranspositionTable(256);
        Uci = new Uci();
    }

    public Action<IPieceSquare> PieceUpdated => _pos.PieceUpdated;

    public int MoveNumber => 0; //(PositionIndex - 1) / 2 + 1;

    public BitBoard Occupied => Pos.Pieces();

    public IPosition Pos => _pos;

    public GameEndTypes GameEndType { get; set; }

    public static TranspositionTable Table { get; }

    public SearchParameters SearchParameters { get; }

    public static IUci Uci { get; }

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

        var moveList = _moveLists.Get();
        moveList.Generate(in _pos);
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var em in moveList.Get())
            if (!Pos.IsLegal(em.Move))
            {
                gameEndType |= GameEndTypes.Pat;
                break;
            }

        GameEndType = gameEndType;
    }

    public override string ToString()
        => _pos.ToString();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public IEnumerator<Piece> GetEnumerator()
        => _pos.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard OccupiedBySide(Player p)
        => _pos.Pieces(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Player CurrentPlayer() => _pos.SideToMove;

    public ulong Perft(int depth, bool root = true)
    {
        var tot = ulong.MinValue;

        var ml = _moveLists.Get();
        ml.Generate(in _pos);
        var state = new State();

        foreach (var em in ml.Get())
            if (root && depth <= 1)
                tot++;
            else
            {
                var m = em.Move;
                _pos.MakeMove(m, in state);

                if (depth <= 2)
                {
                    var ml2 = _moveLists.Get();
                    ml2.Generate(in _pos);
                    tot += (ulong)ml2.Length;
                    _moveLists.Return(ml2);
                }
                else
                    tot += Perft(depth - 1, false);

                _pos.TakeMove(m);
            }

        _moveLists.Return(ml);

        return tot;
    }
}