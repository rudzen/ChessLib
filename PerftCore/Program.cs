namespace Perft
{
    using System;
    using System.Diagnostics;
    using Rudz.Chess.Extensions;
    using P = Rudz.Chess.Perft.Perft;

    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ChessLib Perft test program v0.1.1");
            Console.WriteLine("Use Perft.exe <depth> to set depth (1-6), default is 5.");
            var depth = 0;
            if (args.Length > 0)
                depth = args[0].ToIntegral();

            if (!depth.InBetween(1, 6))
                depth = 5;

            Console.WriteLine($"Running perft test with depth {depth}.");
            if (depth == 6)
                Console.WriteLine("Brace yourself, this could take a little while.");

            void Callback(string s, ulong v)
            {
                Console.WriteLine($"Position   : {s}\nMove Count : {v}");
            }

            var p = new P(depth, Callback);
            p.AddStartPosition();
            var watch = Stopwatch.StartNew();
            var result = p.DoPerft();
            var elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"Time (ms)  : {elapsedMs}");
            Console.WriteLine($"Nps        : {1000 * result / (ulong) elapsedMs}");
            Console.WriteLine(p.GetPositionCount(0, depth) == result
                ? "Move count matches!"
                : "Move count failed!");

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

        }
    }
}