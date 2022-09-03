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

using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib;

public interface IPieceValue
{
    Value MaxValueWithoutPawns { get; }
    Value MaxValue { get; }

    Value PawnValueMg { get; set; }

    Value PawnValueEg { get; set; }

    Value KnightValueMg { get; set; }

    Value KnightValueEg { get; set; }

    Value BishopValueMg { get; set; }

    Value BishopValueEg { get; set; }

    Value RookValueMg { get; set; }

    Value RookValueEg { get; set; }

    Value QueenValueMg { get; set; }

    Value QueenValueEg { get; set; }

    public Value ValueZero { get; set; }

    public Value ValueDraw { get; set; }

    public Value ValueKnownWin { get; set; }

    public Value ValueMate { get; set; }

    public Value ValueInfinite { get; set; }

    public Value ValueNone { get; set; }

    public Value ValueMateInMaxPly { get; }

    public Value ValueMatedInMaxPly { get; }

    void SetDefaults();

    void SetPieceValues(PieceValues[] values, Phases phase);

    PieceValues GetPieceValue(Piece pc, Phases phase);
}
