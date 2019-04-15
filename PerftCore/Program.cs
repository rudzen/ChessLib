/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

using System.Threading.Tasks;

namespace Perft
{
    using Rudz.Chess.Extensions;
    using System;
    using System.Diagnostics;
    using P = Rudz.Chess.Perft.Perft;

    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("ChessLib Perft test program v0.1.1");
            Console.WriteLine("Use Perft.exe <depth> to set depth (1-6), default is 5.");

            if (Stopwatch.IsHighResolution)
                Console.WriteLine($"Timer resolution : {(Stopwatch.IsHighResolution ? "high" : "normal")}.");

            var depth = 0;
            if (args.Length > 0)
                depth = args[0].ToIntegral();

            if (!depth.InBetween(1, 6))
                depth = 5;

            Console.WriteLine($"Running perft test with depth {depth}.");
            if (depth > 5)
                Console.WriteLine("Brace yourself.");

            void Callback(string s, ulong v) => Console.WriteLine($"Position   : {s}\nMove Count : {v}");

            var p = new P(depth, Callback);
            p.AddStartPosition();
            var watch = Stopwatch.StartNew();
            var result = await p.DoPerft();
            // add 1 to avoid potential dbz
            var elapsedMs = watch.ElapsedMilliseconds + 1;
            var nps = 1000 * result / (ulong)elapsedMs;

            Console.WriteLine($"Time (ms)  : {elapsedMs}");
            Console.WriteLine($"Nps        : {nps}");

            var matches = p.GetPositionCount(0, depth) == result;

            Console.WriteLine(matches
                ? "Move count matches!"
                : "Move count failed!");

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}