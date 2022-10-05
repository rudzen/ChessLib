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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using System.Linq;

namespace Rudzoft.ChessLib.Protocol.UCI;

public class Uci : IUci
{
    private static readonly OptionComparer OptionComparer = new();

    private static readonly string[] OptionTypeStrings = Enum.GetNames(typeof(UciOptionType));

    private readonly ObjectPool<StringBuilder> _pvPool;

    public Uci()
    {
        var policy = new StringBuilderPooledObjectPolicy();
        _pvPool = new DefaultObjectPool<StringBuilder>(policy, 128);
        O = new Dictionary<string, IOption>();
    }

    public int MaxThreads { get; set; }

    public IDictionary<string, IOption> O { get; set; }

    public Action<IOption> OnLogger { get; set; }

    public Action<IOption> OnEval { get; set; }

    public Action<IOption> OnThreads { get; set; }

    public Action<IOption> OnHashSize { get; set; }

    public Action<IOption> OnClearHash { get; set; }

    public bool IsDebugModeEnabled { get; set; }

    public void Initialize(int maxThreads = 128)
    {
        O["Write Debug Log"] = new Option("Write Debug Log", O.Count, false, OnLogger);
        O["Write Search Log"] = new Option("Write Search Log", O.Count, false);
        O["Search Log Filename"] = new Option("Search Log Filename", O.Count);
        O["Book File"] = new Option("Book File", O.Count);
        O["Best Book Move"] = new Option("Best Book Move", O.Count, false);
        O["Threads"] = new Option("Threads", O.Count, 1, 1, maxThreads, OnThreads);
        O["Hash"] = new Option("Hash", O.Count, 32, 1, 16384, OnHashSize);
        O["Clear Hash"] = new Option("Clear Hash", O.Count, OnClearHash);
        O["Ponder"] = new Option("Ponder", O.Count, true);
        O["OwnBook"] = new Option("OwnBook", O.Count, false);
        O["MultiPV"] = new Option("MultiPV", O.Count, 1, 1, 500);
        O["UCI_Chess960"] = new Option("UCI_Chess960", O.Count, false);
    }

    public void AddOption(string name, IOption option) => O[name] = option;

    public ulong Nps(in ulong nodes, in TimeSpan time)
        => (ulong)(nodes * 1000.0 / time.Milliseconds);

    public Move MoveFromUci(IPosition pos, ReadOnlySpan<char> uciMove)
    {
        var moveList = pos.GenerateMoves();
        foreach (var move in moveList.Get())
            if (uciMove.Equals(move.Move.ToString(), StringComparison.InvariantCultureIgnoreCase))
                return move.Move;

        return Move.EmptyMove;
    }

    public IEnumerable<Move> MovesFromUci(IPosition pos, Stack<State> states, IEnumerable<string> moves)
    {
        var uciMoves = moves
            .Select(x => MoveFromUci(pos, x))
            .Where(x => !x.IsNullMove());

        foreach (var move in uciMoves)
        {
            var state = new State();
            states.Push(state);
            pos.MakeMove(move, in state);
            yield return move;
        }
    }

    public string UciOk() => "uciok";

    public string ReadyOk() => "readyok";

    public string CopyProtection(CopyProtections copyProtections)
        => $"copyprotection {copyProtections}";

    public string BestMove(Move move, Move ponderMove)
        => !ponderMove.IsNullMove()
            ? $"bestmove {move} ponder {ponderMove}"
            : $"bestmove {move}";

    public string CurrentMoveNum(int moveNumber, Move move, in ulong visitedNodes, in TimeSpan time)
        => $"info currmovenumber {moveNumber} currmove {move} nodes {visitedNodes} time {time.Milliseconds}";

    public string Score(int value, int mateInMaxPly, int valueMate)
        => Math.Abs(value) >= mateInMaxPly
            ? $"mate {(value > 0 ? valueMate - value + 1 : -valueMate - value) / 2}"
            : $"cp {ToCenti(value)}";

    public string ScoreCp(int value)
        => $"info score cp {ToCenti(value)}";

    public string Depth(int depth)
        => $"info depth {depth}";

    public string Pv(
        int count,
        int score,
        int depth,
        int selectiveDepth,
        int alpha,
        int beta,
        in TimeSpan time,
        IEnumerable<Move> pvLine,
        in ulong nodes)
    {
        var sb = _pvPool.Get();

        sb.Append($"info multipv {count + 1} depth {depth} seldepth {selectiveDepth} score {score} ");

        if (score >= beta)
            sb.Append("lowerbound ");
        else if (score <= alpha)
            sb.Append("upperbound ");

        sb.Append($"nodes {nodes} nps {Nps(in nodes, in time)} tbhits {Game.Table.Hits} time {time.Milliseconds} ");
        sb.AppendJoin(' ', pvLine);

        var result = sb.ToString();
        _pvPool.Return(sb);
        return result;
    }

    public string Fullness(in ulong tbHits, in ulong nodes, in TimeSpan time)
        =>
            $"info hashfull {Game.Table.Fullness()} tbhits {tbHits} nodes {nodes} time {time.Milliseconds} nps {Nps(in nodes, in time)}";

    public string MoveToString(Move m, ChessMode chessMode = ChessMode.Normal)
    {
        if (m.IsNullMove())
            return "(none)";

        var (from, to, type) = m;

        if (type == MoveTypes.Castling && chessMode != ChessMode.Chess960)
            to = Square.Create(from.Rank, to > from ? File.FileG : File.FileC);

        Span<char> s = stackalloc char[5];
        var index = 0;

        s[index++] = from.FileChar;
        s[index++] = from.RankChar;
        s[index++] = to.FileChar;
        s[index++] = to.RankChar;

        if (type == MoveTypes.Promotion)
            s[index++] = m.PromotedPieceType().GetPieceChar();

        return new string(s[..index]);
    }

    /// <summary>
    /// Print all the options default values in chronological insertion order (the idx field)
    /// and in the format defined by the UCI protocol.
    /// </summary>
    /// <returns>the current UCI options as string</returns>
    public new string ToString()
    {
        var list = new List<IOption>(O.Values);
        list.Sort(OptionComparer);
        var sb = _pvPool.Get();

        foreach (var opt in CollectionsMarshal.AsSpan(list))
        {
            sb.AppendLine();
            sb.Append("option name ").Append(opt.Name).Append(" type ").Append(OptionTypeStrings[(int)opt.Type]);
            if (opt.Type != UciOptionType.Button)
                sb.Append(" default ").Append(opt.DefaultValue);

            if (opt.Type == UciOptionType.Spin)
                sb.Append(" min ").Append(opt.Min).Append(" max ").Append(opt.Max);
        }

        var result = sb.ToString();
        _pvPool.Return(sb);
        return result;
    }

    private static int ToCenti(int v) => v / 100;
}