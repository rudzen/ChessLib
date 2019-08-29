namespace Perft.Options
{
    using CommandLine;
    using System.Collections.Generic;

    [Verb("epd", HelpText = "Add parsing of an epd file containing perft information")]
    public sealed class EpdOptions : IOptions
    {
        [Option('f', "files", Required = true, HelpText = "List of epd files to parse.")]
        public IEnumerable<string> Epds { get; set; }

        [Option('h', "help", Required = false, HelpText = "Show more detailed help for epd file format")]
        public bool Help { get; set; }
    }
}