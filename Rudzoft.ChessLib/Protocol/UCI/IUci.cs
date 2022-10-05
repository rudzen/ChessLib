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
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Protocol.UCI;

public interface IUci
{
    public int MaxThreads { get; set; }
    public IDictionary<string, IOption> O { get; set; }
    Action<IOption> OnLogger { get; set; }
    Action<IOption> OnEval { get; set; }
    Action<IOption> OnThreads { get; set; }
    Action<IOption> OnHashSize { get; set; }
    Action<IOption> OnClearHash { get; set; }
    bool IsDebugModeEnabled { get; set; }

    void Initialize(int maxThreads = 128);

    void AddOption(string name, IOption option);

    ulong Nps(in ulong nodes, in TimeSpan time);

    Move MoveFromUci(IPosition pos, ReadOnlySpan<char> uciMove);

    IEnumerable<Move> MovesFromUci(IPosition pos, Stack<State> states, IEnumerable<string> moves);

    string UciOk();

    string ReadyOk();

    string CopyProtection(CopyProtections copyProtections);

    string BestMove(Move move, Move ponderMove);

    string CurrentMoveNum(int moveNumber, Move move, in ulong visitedNodes, in TimeSpan time);

    string Score(int value, int mateInMaxPly, int valueMate);

    string ScoreCp(int value);

    string Depth(int depth);

    string Pv(int count, int score, int depth, int selectiveDepth, int alpha, int beta, in TimeSpan time, IEnumerable<Move> pvLine, in ulong nodes);

    string Fullness(in ulong tbHits, in ulong nodes, in TimeSpan time);

    string ToString();
}
