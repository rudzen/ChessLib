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

using System;
using System.Text.Json.Serialization;
using Rudzoft.ChessLib.Enums;

namespace Rudzoft.ChessLib.Types;

public enum PieceValues
{
    ValueZero = 0,
    ValueDraw = 0,
    ValueKnownWin = 10000,
    ValueMate = 32000,
    ValueInfinite = 32001,
    ValueMinusInfinite = -32001,
    ValueNone = 32002,

    ValueMateInMaxPly = ValueMate - 2 * PieceValue.MAX_PLY,
    ValueMatedInMaxPly = -ValueMate + 2 * PieceValue.MAX_PLY,

    PawnValueMg = 128,
    PawnValueEg = 213,
    KnightValueMg = 781,
    KnightValueEg = 854,
    BishopValueMg = 825,
    BishopValueEg = 915,
    RookValueMg = 1276,
    RookValueEg = 1380,
    QueenValueMg = 2538,
    QueenValueEg = 2682,

    MidgameLimit = 15258,
    EndgameLimit = 3915
}

public sealed class PieceValue : IPieceValue
{
    public const int MAX_PLY = 246;

    private readonly PieceValues[][] _defaults;

    private Value _valueMateInMaxPly;
    private Value _valueMatedInMaxPly;
    private Value _valueMate;

    public PieceValue()
    {
        _defaults = new PieceValues[2][];
        for (var index = 0; index < _defaults.Length; index++)
            _defaults[index] = new PieceValues[6];

        _defaults[0] = new[] { PieceValues.ValueZero, PieceValues.PawnValueMg, PieceValues.KnightValueMg, PieceValues.BishopValueMg, PieceValues.RookValueMg, PieceValues.QueenValueMg };
        _defaults[1] = new[] { PieceValues.ValueZero, PieceValues.PawnValueEg, PieceValues.KnightValueEg, PieceValues.BishopValueEg, PieceValues.RookValueEg, PieceValues.QueenValueEg };

        SetDefaults();
    }

    public PieceValues[][] _values { get; set; }

    public Value MaxValueWithoutPawns { get; private set; }

    public Value MaxValue { get; private set; }

    public Value PawnValueMg { get; set; }

    public Value PawnValueEg { get; set; }

    public Value KnightValueMg { get; set; }

    public Value KnightValueEg { get; set; }

    public Value BishopValueMg { get; set; }

    public Value BishopValueEg { get; set; }

    public Value RookValueMg { get; set; }

    public Value RookValueEg { get; set; }

    public Value QueenValueMg { get; set; }

    public Value QueenValueEg { get; set; }

    public Value ValueZero { get; set; }

    public Value ValueDraw { get; set; }

    public Value ValueKnownWin { get; set; }

    public Value ValueMate
    {
        get => _valueMate;
        set
        {
            if (_valueMate == value)
                return;

            _valueMate = value;
            _valueMateInMaxPly = value - 2 * MAX_PLY;
            _valueMatedInMaxPly = value + 2 * MAX_PLY;
        }
    }

    public Value ValueInfinite { get; set; }

    public Value ValueNone { get; set; }

    [JsonIgnore]
    public Value ValueMateInMaxPly => _valueMateInMaxPly;

    [JsonIgnore]
    public Value ValueMatedInMaxPly => _valueMatedInMaxPly;

    public void SetDefaults()
    {
        _values = new PieceValues[2][];
        for (var index = 0; index < _values.Length; index++)
            _values[index] = new PieceValues[6];

        Span<Phases> phases = stackalloc Phases[] { Phases.Mg, Phases.Eg };

        foreach (var phase in phases)
            SetPieceValues(_defaults[(int)phase], phase);

        var sum = Value.ValueZero;
        var sumNoPawns = Value.ValueZero;

        Span<PieceTypes> pieceTypes = stackalloc PieceTypes[] { PieceTypes.Pawn, PieceTypes.Knight, PieceTypes.Bishop, PieceTypes.Rook, PieceTypes.Queen };

        foreach (var pt in pieceTypes)
        {
            var value = new Value(_values[0][pt.AsInt()]);
            switch (pt)
            {
                case PieceTypes.Pawn:
                    value *= 8;
                    break;

                case PieceTypes.Knight:
                case PieceTypes.Bishop:
                case PieceTypes.Rook:
                    value *= 2;
                    break;
            }
            if (pt != PieceTypes.Pawn)
                sumNoPawns += value;
            sum += value;
        }

        MaxValueWithoutPawns = sumNoPawns * 2;
        MaxValue = sum * 2;

        PawnValueMg = GetPieceValue(PieceTypes.Pawn, Phases.Mg);
        PawnValueEg = GetPieceValue(PieceTypes.Pawn, Phases.Eg);
        KnightValueMg = GetPieceValue(PieceTypes.Knight, Phases.Mg);
        KnightValueEg = GetPieceValue(PieceTypes.Knight, Phases.Eg);
        BishopValueMg = GetPieceValue(PieceTypes.Bishop, Phases.Mg);
        BishopValueEg = GetPieceValue(PieceTypes.Bishop, Phases.Eg);
        RookValueMg = GetPieceValue(PieceTypes.Rook, Phases.Mg);
        RookValueEg = GetPieceValue(PieceTypes.Rook, Phases.Eg);
        QueenValueMg = GetPieceValue(PieceTypes.Queen, Phases.Mg);
        QueenValueEg = GetPieceValue(PieceTypes.Queen, Phases.Eg);
    }

    public void SetPieceValues(PieceValues[] values, Phases phase)
        => Array.Copy(values, _values[phase.AsInt()], values.Length);

    public PieceValues GetPieceValue(Piece pc, Phases phase)
        => _values[(int)phase][pc.Type().AsInt()];

    public PieceValues GetPieceValue(PieceTypes pt, Phases phase)
        => _values[phase.AsInt()][pt.AsInt()];
}
