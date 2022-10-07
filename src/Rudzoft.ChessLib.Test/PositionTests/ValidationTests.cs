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

using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Test.PositionTests;

public sealed class ValidationTests
{
    [Fact]
    public void ValidationKingsNegative()
    {
        const PositionValidationTypes type = PositionValidationTypes.Kings;
        var expectedErrorMsg = $"king count for player {Player.White} was 2";

        var game = GameFactory.Create();
        game.NewGame();

        var pc = PieceTypes.King.MakePiece(Player.White);

        game.Pos.AddPiece(pc, Square.E4);

        var validator = game.Pos.Validate(type);

        Assert.NotEmpty(validator.ErrorMsg);

        var actualErrorMessage = validator.ErrorMsg;

        Assert.Equal(expectedErrorMsg, actualErrorMessage);
        Assert.False(validator.IsOk);
    }

    [Fact]
    public void ValidateCastleling()
    {
        // position only has pawns, rooks and kings
        const string fen = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1";
        const PositionValidationTypes validationType = PositionValidationTypes.Castle;
        var game = GameFactory.Create();
        game.NewGame(fen);

        var validator = game.Pos.Validate(validationType);

        Assert.True(validator.IsOk);
        Assert.True(validator.ErrorMsg.IsNullOrEmpty());
    }

    // TODO : Add tests for the rest of the validations
}