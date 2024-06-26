/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

using Microsoft.Extensions.DependencyInjection;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Notation;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Test.NotationTests;

public sealed class SanTests
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
                                                         .AddChessLib()
                                                         .BuildServiceProvider();

    [Theory]
    [InlineData("8/6k1/8/8/8/8/1K1N1N2/8 w - - 0 1", MoveNotations.San, PieceTypes.Knight, Squares.d2, Squares.f2,
        Squares.e4)]
    public void SanRankAmbiguities(
        string fen,
        MoveNotations moveNotations,
        PieceTypes movingPt,
        Squares fromSqOne,
        Squares fromSqTwo,
        Squares toSq)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state   = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var pt = new PieceType(movingPt);
        var pc = pt.MakePiece(pos.SideToMove);

        var fromOne = new Square(fromSqOne);
        var fromTwo = new Square(fromSqTwo);
        var to      = new Square(toSq);

        Assert.True(fromOne.IsOk);
        Assert.True(fromTwo.IsOk);
        Assert.True(to.IsOk);

        var pieceChar = pc.GetPieceChar();
        var toString  = to.ToString();

        var moveOne = Move.Create(fromOne, to);
        var moveTwo = Move.Create(fromTwo, to);

        var moveNotation = _serviceProvider.GetRequiredService<IMoveNotation>();
        var notation     = moveNotation.ToNotation(moveNotations);

        var expectedOne = $"{pieceChar}{fromOne.FileChar}{toString}";
        var expectedTwo = $"{pieceChar}{fromTwo.FileChar}{toString}";

        var actualOne = notation.Convert(pos, moveOne);
        var actualTwo = notation.Convert(pos, moveTwo);

        Assert.Equal(expectedOne, actualOne);
        Assert.Equal(expectedTwo, actualTwo);
    }

    [Theory]
    [InlineData("8/6k1/8/8/3N4/8/1K1N4/8 w - - 0 1", MoveNotations.San, PieceTypes.Knight, Squares.d2, Squares.d4,
        Squares.f3)]
    public void SanFileAmbiguities(
        string fen, MoveNotations moveNotations, PieceTypes movingPt, Squares fromSqOne,
        Squares fromSqTwo, Squares toSq)
    {
        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state   = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var pt = new PieceType(movingPt);
        var pc = pt.MakePiece(pos.SideToMove);

        var fromOne = new Square(fromSqOne);
        var fromTwo = new Square(fromSqTwo);
        var to      = new Square(toSq);

        Assert.True(fromOne.IsOk);
        Assert.True(fromTwo.IsOk);
        Assert.True(to.IsOk);

        var pieceChar = pc.GetPieceChar();
        var toString  = to.ToString();

        var moveOne = Move.Create(fromOne, to);
        var moveTwo = Move.Create(fromTwo, to);

        var moveNotation = _serviceProvider.GetRequiredService<IMoveNotation>();
        var notation     = moveNotation.ToNotation(moveNotations);

        var expectedOne = $"{pieceChar}{fromOne.RankChar}{toString}";
        var expectedTwo = $"{pieceChar}{fromTwo.RankChar}{toString}";

        var actualOne = notation.Convert(pos, moveOne);
        var actualTwo = notation.Convert(pos, moveTwo);

        Assert.Equal(expectedOne, actualOne);
        Assert.Equal(expectedTwo, actualTwo);
    }

    [Fact]
    public void RookSanAmbiguity()
    {
        // Tests rook ambiguity notation for white rooks @ e1 and g2. Original author : johnathandavis

        const string        fen               = "5r1k/p6p/4r1n1/3NPp2/8/8/PP4RP/4R1K1 w - - 3 53";
        const MoveNotations moveNotations     = MoveNotations.San;
        var                 expectedNotations = new[] { "Ree2", "Rge2" };

        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state   = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var sideToMove  = pos.SideToMove;
        var targetPiece = PieceType.Rook.MakePiece(sideToMove);

        var moveNotation = _serviceProvider.GetRequiredService<IMoveNotation>();
        var notation     = moveNotation.ToNotation(moveNotations);

        var ml = pos.GenerateMoves();
        ml.Generate(in pos);

        var sanMoves = ml
            .GetMoves(move => pos.GetPiece(move.FromSquare()) == targetPiece)
            .Select(m => notation.Convert(pos, m))
            .ToArray();

        foreach (var notationResult in expectedNotations)
            Assert.Contains(sanMoves, s => s == notationResult);
    }

    [Theory]
    [InlineData("2rr2k1/p3ppbp/b1n3p1/2p1P3/5P2/2N3P1/PP2N1BP/3R1RK1 w - - 2 18", "Rxd8+")]
    public void SanCaptureWithCheck(string fen, string expected)
    {
        // author: skotz

        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state   = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var moveNotation = _serviceProvider.GetRequiredService<IMoveNotation>();
        var notation     = moveNotation.ToNotation(MoveNotations.San);

        var move    = Move.Create(Square.D1, Square.D8, MoveTypes.Normal);
        var sanMove = notation.Convert(pos, move);

        // Capturing a piece with check
        Assert.Equal(sanMove, expected);
    }

    [Theory]
    [InlineData("2rR2k1/p3ppbp/b1n3p1/2p1P3/5P2/2N3P1/PP2N1BP/5RK1 b - - 0 36", "Nxd8", "Rxd8", "Bf8")]
    public void SanRecaptureNotCheckmate(string fen, params string[] expected)
    {
        // author: skotz

        var pos = _serviceProvider.GetRequiredService<IPosition>();

        var fenData = new FenData(fen);
        var state   = new State();

        pos.Set(in fenData, ChessMode.Normal, state);

        var moveNotation = _serviceProvider.GetRequiredService<IMoveNotation>();
        var notation     = moveNotation.ToNotation(MoveNotations.San);

        var allMoves = pos.GenerateMoves().Get();

        foreach (var move in allMoves)
        {
            var sanMove = notation.Convert(pos, move);

            // Recapturing a piece to remove the check
            Assert.Contains(sanMove, expected);
        }
    }
}