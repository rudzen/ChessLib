// using Rudzoft.ChessLib.Types;
//
// [MemoryDiagnoser]
// public class ComputeKnightAttackBenchmark
// {
//     private BitBoard bitBoard;
//
//     [GlobalSetup]
//     public void Setup()
//     {
//         // Initialize the BitBoard with some data
//         bitBoard = new BitBoard(0x5555555555555555);
//     }
//
//     [Benchmark]
//     public void OldComputeKnightAttack()
//     {
//         BitBoards.ComputeKnightAttack(bitBoard);
//     }
//
//     [Benchmark]
//     public void NewComputeKnightAttackSimd()
//     {
//         BitBoards.ComputeKnightAttackSimd(bitBoard);
//     }
// }
