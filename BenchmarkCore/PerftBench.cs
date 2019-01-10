namespace BenchmarkCore
{
    using BenchmarkDotNet.Attributes;

    [ClrJob(baseline: true), CoreJob, CoreRtJob]
    [RPlotExporter, RankColumn]
    public class PerftBench
    {
        private Perft _perft;

        [Params(1, 2, 3, 4, 5, 6)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            _perft = new Perft(N);
        }

        [Benchmark]
        public ulong Result() => _perft.DoPerft();
    }
}