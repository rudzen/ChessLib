using CommandLine;

namespace Perft.Options
{
    [Verb("tt", HelpText = "Configuration for transposition table")]
    public class TTOptions : IOptions
    {
        [Option('u', "use", Required = false, Default = true, HelpText = "Dis/En-able use of transposition table")]
        public bool Use { get; set; }

        [Option('s', "size", Required = false, Default = 32, HelpText = "Set the size of the transposition table in mb")]
        public int Size { get; set; }
    }
}