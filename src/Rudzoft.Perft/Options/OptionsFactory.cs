/*
Perft, a chess perft testing application

MIT License

Copyright (c) 2019-2023 Rudy Alex Kohn

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

using CommandLine;
using Rudzoft.Perft.Models;

namespace Rudzoft.Perft.Options;

public sealed class OptionsFactory : IOptionsFactory
{
    private readonly string[] _args;

    public OptionsFactory(CommandLineArgs args)
    {
        _args = args.Args;
    }
    
    public IEnumerable<PerftOption> Parse()
    {
        var optionsUsed = OptionType.None;
        IPerftOptions options = null;
        IPerftOptions ttOptions = null;

        var setEdp = new Func<EpdOptions, int>(o =>
        {
            optionsUsed |= OptionType.EdpOptions;
            options = o;
            return 0;
        });

        var setFen = new Func<FenOptions, int>(o =>
        {
            optionsUsed |= OptionType.FenOptions;
            options = o;
            return 0;
        });

        var setTT = new Func<TTOptions, int>(o =>
        {
            optionsUsed |= OptionType.TTOptions;
            ttOptions = o;
            return 0;
        });

        // fens -f "rnkq1bnr/p3ppp1/1ppp3p/3B4/6b1/2PQ3P/PP1PPP2/RNB1K1NR w KQ -" -d 6
        // fens -f "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1" -d 6
        // epd -f D:\perft-random.epd
        var returnValue = Parser.Default.ParseArguments<EpdOptions, FenOptions, TTOptions>(_args)
            .MapResult(
                (EpdOptions opts) => setEdp(opts),
                (FenOptions opts) => setFen(opts),
                (TTOptions opts) => setTT(opts),
                static _ => 1);

        if (returnValue != 0)
            yield break;

        if (optionsUsed.HasFlagFast(OptionType.EdpOptions))
            yield return new(OptionType.EdpOptions, options!);
        else
            yield return new(OptionType.FenOptions, options!);

        if (optionsUsed.HasFlagFast(OptionType.TTOptions))
            yield return new(OptionType.TTOptions, ttOptions!);
    }
}