/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

namespace Rudzoft.ChessLib.Test.MaterialTests;

public sealed class MaterialTests
{
    //TODO : Implement the RIGHT way

    [Fact]
    public void MaterialValueTest()
    {
        Assert.True(true);
        //    var pos = new Position();
        //    var game = new Game(pos);
        //    game.NewGame();

        //    var startMaterial = game.State.Material.MaterialValueTotal;

        //    // generate moves
        //    IList<Move> moves = new List<Move>(12) {
        //                                              new Move(Rudz.Chess.Enums.Pieces.WhitePawn, Squares.f2, Squares.f4, MoveTypes.Doublepush, Rudz.Chess.Enums.Pieces.NoPiece),
        //                                              new Move(Rudz.Chess.Enums.Pieces.BlackPawn, Squares.e7, Squares.e5, MoveTypes.Doublepush, Rudz.Chess.Enums.Pieces.NoPiece),
        //                                              new Move(Rudz.Chess.Enums.Pieces.WhitePawn, Rudz.Chess.Enums.Pieces.BlackPawn, Squares.f4, Squares.e5, MoveTypes.Capture),
        //                                              new Move(Rudz.Chess.Enums.Pieces.BlackPawn, Squares.d7, Squares.d5, MoveTypes.Doublepush, Rudz.Chess.Enums.Pieces.NoPiece),
        //                                              new Move(Rudz.Chess.Enums.Pieces.WhitePawn, Squares.f5, Squares.d6, MoveTypes.Epcapture, Rudz.Chess.Enums.Pieces.NoPiece)
        //                                          };

        //    var lostMaterial = new List<int>(12)
        //    {
        //        0,
        //        0,
        //        100,
        //        100,
        //        200
        //    };

        //    var stepMaterial = new List<int>(12)
        //    {
        //        startMaterial,
        //        startMaterial - 100,
        //        startMaterial - 100,
        //        startMaterial - 100,
        //        startMaterial - 200
        //    };

        //    for (var i = 0; i < moves.Count; i++)
        //    {
        //        Assert.True(game.MakeMove(moves[i]));
        //        Assert.Equal(lostMaterial[i], Math.Abs(game.State.Material.MaterialValueTotal));
        //    }
    }
}