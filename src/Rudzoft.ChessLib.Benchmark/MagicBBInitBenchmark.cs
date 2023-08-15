// using Rudzoft.ChessLib.Types;
//
// namespace Rudzoft.ChessLib.Benchmark;
//
// [MemoryDiagnoser]
// public class MagicBBInitBenchmark
// {
//     [Benchmark]
//     public BitBoard V1()
//     {
//         return MagicBB.BishopAttacks(Square.A1, BitBoard.Empty);
//     }
//     
//         [Benchmark]
//     public BitBoard V2()
//     {
//         return MagicBB2.BishopAttacks(Square.A1, BitBoard.Empty);
//     }
// }