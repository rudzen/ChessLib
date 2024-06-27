// using System.Linq;
// using Rudzoft.ChessLib.Factories;
// using Rudzoft.ChessLib.MoveGeneration;
// using Rudzoft.ChessLib.Protocol.UCI;
// using Rudzoft.ChessLib.Types;
//
// namespace Rudzoft.ChessLib.Test.MateTests;
//
// public sealed class MateTests
// {
//
//     [Fact]
//     public void IsMateTest()
//     {
//         const string fen = "8/6pk/pb5p/8/1P2qP2/P7/2r2pNP/1QR4K b - - 1 2";
//         var game = GameFactory.Create(fen);
//         
//         /*
//          * e3f2 g1h1 f2f1q
//          */
//     
//         var uciMoves = new string[]
//         {
//             "e3f2", "g1h1", "f2f1q"
//         };
//     
//         var pos = game.Pos;
//         var uci = new Uci();
//         var s = new State();
//     
//         foreach (var uciMove in uciMoves)
//         {
//             var move = uci.MoveFromUci(pos, uciMove);
//             pos.MakeMove(move, s);
//         }
//     
//         var moves = pos.GenerateMoves();
//     
//         var empty = moves.Get().IsEmpty;
//         
//         Assert.True(empty);
//     }
//     
// }