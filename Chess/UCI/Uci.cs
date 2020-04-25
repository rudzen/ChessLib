/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

using Rudz.Chess.Types;

namespace Rudz.Chess.UCI
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Uci : IUci
    {
        protected readonly IDictionary<string, IOption> O;

        private static readonly OptionComparer OptionComparer;

        private static readonly string[] OptionTypeStrings;

        public Uci(IDictionary<string, IOption> options, int maxThreads = 128)
        {
            O = options;
            Initialize(maxThreads);
        }

        static Uci()
        {
            OptionComparer = new OptionComparer();
            OptionTypeStrings = Enum.GetNames(typeof(UciOptionType));
        }

        public Action<IOption> OnLogger { get; set; }

        public Action<IOption> OnEval { get; set; }

        public Action<IOption> OnThreads { get; set; }

        public Action<IOption> OnHashSize { get; set; }

        public Action<IOption> OnClearHash { get; set; }

        protected void Initialize(int maxThreads = 128)
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

        public Move MoveFromUci(IPosition pos, string uciMove)
        {
            var moveList = pos.GenerateMoves();
            var moves = moveList.GetMoves();

            foreach (var move in moves)
            {
                if (uciMove.Equals(move.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    return move;
            }

            return MoveExtensions.EmptyMove;
        }
        
        /// <summary>
        /// Print all the options default values in chronological
        /// insertion order (the idx field) and in the format defined by the UCI protocol.
        /// </summary>
        /// <returns>the current UCI options as string</returns>
        public override string ToString()
        {
            var list = new List<IOption>(O.Values);
            list.Sort(OptionComparer);
            var sb = new StringBuilder(128);

            foreach (var opt in list)
            {
                sb.AppendLine();
                sb.Append("option name ").Append(opt.Name).Append(" type ").Append(OptionTypeStrings[(int) opt.Type]);
                if (opt.Type != UciOptionType.Button)
                    sb.Append(" default ").Append(opt.DefaultValue);

                if (opt.Type == UciOptionType.Spin)
                    sb.Append(" min ").Append(opt.Min).Append(" max ").Append(opt.Max);
            }
            return sb.ToString();
        }
    }
}