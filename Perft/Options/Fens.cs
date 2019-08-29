using CommandLine;
using System.Collections.Generic;

namespace Perft.Options
{
    [Verb("fens", HelpText = "Add fens to run")]
    public class FenOptions : IOptions
    {
        [Option('f', "fen", Required = true, HelpText = "Set the FEN string to perform perft test on.")]
        public IEnumerable<string> Fens { get; set; }

        [Option('d', "depth", Required = true, HelpText = "Set the depth for corresponding fens in same order")]
        public IEnumerable<int> Depths { get; set; }
    }
}