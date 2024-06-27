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

using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using System.Runtime.CompilerServices;
using File = Rudzoft.ChessLib.Types.File;

namespace Rudzoft.ChessLib.Protocol.UCI;

public class Uci : IUci
{
    private static readonly IComparer<IOption> OptionComparer = new OptionComparer();

    private static readonly string[] OptionTypeStrings = Enum.GetNames(typeof(UciOptionType));

    protected readonly ObjectPool<MoveList> MoveListPool;

    private readonly ObjectPool<StringBuilder> _pvPool;
    private readonly Dictionary<string, IOption> _options;

    public Uci(ObjectPool<MoveList> moveListPool)
    {
        var policy = new StringBuilderPooledObjectPolicy();
        _pvPool = new DefaultObjectPool<StringBuilder>(policy, 128);
        _options = new();
        MoveListPool = moveListPool;
    }

    public int MaxThreads { get; set; }
    public Action<IOption> OnLogger { get; set; }
    public Action<IOption> OnEval { get; set; }
    public Action<IOption> OnThreads { get; set; }
    public Action<IOption> OnHashSize { get; set; }
    public Action<IOption> OnClearHash { get; set; }
    public bool IsDebugModeEnabled { get; set; }

    public void Initialize(int maxThreads = 128)
    {
        _options.Clear();
        _options["Write Debug Log"] = new Option("Write Debug Log", _options.Count, false, OnLogger);
        _options["Write Search Log"] = new Option("Write Search Log", _options.Count, false);
        _options["Search Log Filename"] = new Option("Search Log Filename", _options.Count);
        _options["Book File"] = new Option("Book File", _options.Count);
        _options["Best Book Move"] = new Option("Best Book Move", _options.Count, false);
        _options["Threads"] = new Option("Threads", _options.Count, 1, 1, maxThreads, OnThreads);
        _options["Hash"] = new Option("Hash", _options.Count, 32, 1, 16384, OnHashSize);
        _options["Clear Hash"] = new Option("Clear Hash", _options.Count, OnClearHash);
        _options["Ponder"] = new Option("Ponder", _options.Count, true);
        _options["OwnBook"] = new Option("OwnBook", _options.Count, false);
        _options["MultiPV"] = new Option("MultiPV", _options.Count, 1, 1, 500);
        _options["UCI_Chess960"] = new Option("UCI_Chess960", _options.Count, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOption(string name, IOption option) => _options[name] = option;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetOption(string name, out IOption option)
    {
        ref var opt = ref CollectionsMarshal.GetValueRefOrNullRef(_options, name);
        var result = !Unsafe.IsNullRef(ref opt);
        option = result ? opt : default;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Nps(in ulong nodes, in TimeSpan time)
        => (ulong)(nodes * 1000.0 / time.Milliseconds);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Nps(in UInt128 nodes, in TimeSpan time)
    {
        var t    = nodes * 1000;
        var d = new UInt128(ulong.MinValue, (ulong)time.Milliseconds + 1);
        return (ulong)(t / d);
    }

    [SkipLocalsInit]
    public Move MoveFromUci(IPosition pos, ReadOnlySpan<char> uciMove)
    {
        var ml = MoveListPool.Get();

        ml.Generate(in pos);

        var moves = ml.Get();

        var m = Move.EmptyMove;

        Span<char> s = stackalloc char[5];
        foreach (var move in moves)
        {
            if (move.Move.TryFormat(s, out var written)
                && !uciMove.Equals(s[..written] ,StringComparison.Ordinal))
                continue;

            m = move.Move;
            break;
        }

        MoveListPool.Return(ml);

        return m;
    }

    public IEnumerable<Move> MovesFromUci(IPosition pos, Stack<State> states, IEnumerable<string> moves)
    {
        foreach (var move in moves.Select(x => MoveFromUci(pos, x)))
        {
            if (move.IsNullMove())
                continue;

            var state = new State();
            states.Push(state);
            pos.MakeMove(move, in state);
            yield return move;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string UciOk() => "uciok";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadyOk() => "readyok";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string CopyProtection(CopyProtections copyProtections)
        => $"copyprotection {copyProtections}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string BestMove(Move move, Move ponderMove)
        => !ponderMove.IsNullMove()
            ? $"bestmove {move} ponder {ponderMove}"
            : $"bestmove {move}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string CurrentMoveNum(int moveNumber, Move move, in ulong visitedNodes, in TimeSpan time)
        => $"info currmovenumber {moveNumber} currmove {move} nodes {visitedNodes} time {time.Milliseconds}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Score(int value, int mateInMaxPly, int valueMate)
    {
        if (Math.Abs(value) >= mateInMaxPly)
        {
            var s = (value > 0 ? valueMate - value + 1 : -valueMate - value) / 2;
            return $"mate {s}";
        }

        return $"cp {ToCenti(value)}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ScoreCp(int value)
        => $"info score cp {ToCenti(value)}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Depth(int depth)
        => $"info depth {depth}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Pv(
        int count,
        int score,
        int depth,
        int selectiveDepth,
        int alpha,
        int beta,
        in TimeSpan time,
        IEnumerable<Move> pvLine,
        in ulong nodes,
        in ulong tableHits
        )
    {
        var sb = _pvPool.Get();

        sb.Append($"info multipv {count + 1} depth {depth} seldepth {selectiveDepth} score {score} ");

        if (score >= beta)
            sb.Append("lowerbound ");
        else if (score <= alpha)
            sb.Append("upperbound ");

        sb.Append($"nodes {nodes} nps {Nps(in nodes, in time)} tbhits {tableHits} time {time.Milliseconds} ");
        sb.AppendJoin(' ', pvLine);

        var result = sb.ToString();
        _pvPool.Return(sb);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Fullness(int fullNess, in ulong tbHits, in ulong nodes, in TimeSpan time)
        =>
            $"info hashfull {fullNess} tbhits {tbHits} nodes {nodes} time {time.Milliseconds} nps {Nps(in nodes, in time)}";

    [SkipLocalsInit]
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

        return new(s[..index]);
    }

    /// <summary>
    /// Print all the options default values in chronological insertion order (the idx field)
    /// and in the format defined by the UCI protocol.
    /// </summary>
    /// <returns>the current UCI options as string</returns>
    public new string ToString()
    {
        var sb = _pvPool.Get();

        foreach (var opt in _options.Values.Order(OptionComparer))
        {
            sb.AppendLine();

            var optionType = OptionTypeStrings[(int)opt.Type];

            sb.Append("option name ")
              .Append(opt.Name)
              .Append(" type ")
              .Append(optionType);

            if (opt.Type != UciOptionType.Button)
                sb.Append(" default ").Append(opt.DefaultValue);

            if (opt.Type == UciOptionType.Spin)
                sb.Append(" min ")
                  .Append(opt.Min)
                  .Append(" max ")
                  .Append(opt.Max);
        }

        var result = sb.ToString();
        _pvPool.Return(sb);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToCenti(int v) => v / 100;
}