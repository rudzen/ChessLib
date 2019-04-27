namespace BenchmarkCore
{
    using BenchmarkDotNet.Attributes;
    using Rudz.Chess.Perft;

    //[ClrJob(true), CoreJob]
    //[RPlotExporter, RankColumn]
    public class PerftBench
    {
        private Perft _perft;

        [Params(4, 5, 6)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            _perft = new Perft(N);
            _perft.AddStartPosition();
        }

        [Benchmark]
        public ulong Result() => _perft.DoPerft();
    }
}