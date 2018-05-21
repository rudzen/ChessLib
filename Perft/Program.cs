using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rudz.Chess.Extensions;

namespace Perft
{
    public class Program
    {

        private static readonly IList<ulong> vals;

        static Program() => vals = new List<ulong>(6)
        {
            20,
            400,
            8902,
            197281,
            4865609,
            119060324
        };

        public static void Main(string[] args)
        {
            Console.WriteLine("ChessLib Perft test program v0.1.");
            Console.WriteLine("Use Perft.exe <depth> to set depth (1-6), default is 5.");
            int depth = 0;
            if (args.Length > 0)
                depth = args[0].ToIntegral();

            if (!depth.InBetween(1, 6))
                depth = 5;
            
            Console.WriteLine($"Running perft test with depth {depth}.");
            if (depth == 6)
                Console.WriteLine("Brace yourself, this could take a little while.");
            
            Perft p = new Perft(depth);
            
            Stopwatch watch = Stopwatch.StartNew();
            ulong result = p.DoPerft();
            long elapsedMs = watch.ElapsedMilliseconds;
            
            Console.WriteLine("Perft test complete..");
            Console.WriteLine($"Total moves processed : {result}");
            Console.WriteLine($"Time to process (ms) : {elapsedMs}");
            Console.WriteLine(vals[depth - 1] == result
                                               ? "Move count matches!"
                                               : "Move count failed!");
        }
        
        
        
        
    }
}