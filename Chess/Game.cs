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
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using Rudz.Chess.Enums;
using Rudz.Chess.Fen;
using Rudz.Chess.Hash.Tables.Transposition;
using Rudz.Chess.MoveGeneration;
using Rudz.Chess.ObjectPoolPolicies;
using Rudz.Chess.Protocol.UCI;
using Rudz.Chess.Types;

namespace Rudz.Chess;

public sealed class Game : IGame
{
    private readonly ObjectPool<IMoveList> _moveLists;

    public Game(IPosition pos)
    {
        Pos = pos;
        _moveLists = new DefaultObjectPool<IMoveList>(new MoveListPolicy());
        Uci = new Uci();
        SearchParameters = new SearchParameters();
    }

    static Game()
    {
        Table = new TranspositionTable(256);
    }

    public Action<IPieceSquare> PieceUpdated => Pos.PieceUpdated;

    public int MoveNumber => 0; //(PositionIndex - 1) / 2 + 1;

    public BitBoard Occupied => Pos.Pieces();

    public IPosition Pos { get; }

    public GameEndTypes GameEndType { get; set; }

    public static TranspositionTable Table { get; }

    public SearchParameters SearchParameters { get; }

    public IUci Uci { get; }

    public bool IsRepetition => Pos.IsRepetition;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NewGame(string fen = Fen.Fen.StartPositionFen)
    {
        var fenData = new FenData(fen);
        var state = new State();
        Pos.Set(in fenData, ChessMode.Normal, state, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FenData GetFen() => Pos.GenerateFen();

    public void UpdateDrawTypes()
    {
        var gameEndType = GameEndTypes.None;
        if (IsRepetition)
            gameEndType |= GameEndTypes.Repetition;
        // if (State.Material[Player.White.Side] <= 300 && State.Material[Player.Black.Side] <=
        // 300)//&& Pos.BoardPieces[0].Empty && Pos.BoardPieces[8].Empty) gameEndType |= GameEndTypes.MaterialDrawn;
        if (Pos.Rule50 >= 100)
            gameEndType |= GameEndTypes.FiftyMove;

        var moveList = _moveLists.Get();
        moveList.Generate(Pos);
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var em in moveList)
            if (!Pos.IsLegal(em.Move))
            {
                gameEndType |= GameEndTypes.Pat;
                break;
            }

        GameEndType = gameEndType;
    }

    public override string ToString()
        => Pos.ToString();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public IEnumerator<Piece> GetEnumerator()
        => Pos.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard OccupiedBySide(Player c)
        => Pos.Pieces(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Player CurrentPlayer() => Pos.SideToMove;

    public ulong Perft(int depth, bool root = false)
    {
        var ml = _moveLists.Get();
        ml.Generate(Pos);

        if (depth == 1)
        {
            var res = ml.Count;
            _moveLists.Return(ml);
            return (ulong)res;
        }

        var state = new State();
        ulong tot = 0;

        foreach (var move in ml.Select(static em => em.Move))
        {
            Pos.MakeMove(move, in state);
            tot += Perft(depth - 1);
            Pos.TakeMove(move);
        }

        _moveLists.Return(ml);

        return tot;
    }
}