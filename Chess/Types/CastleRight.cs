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
using System.Collections;
using System.Collections.Generic;
using Rudz.Chess.Hash;

namespace Rudz.Chess.Types;

[Flags]
public enum CastlelingRights
{
    None = 0,
    WhiteOo = 1,
    WhiteOoo = WhiteOo << 1,
    BlackOo = WhiteOo << 2,
    BlackOoo = WhiteOo << 3,

    KingSide = WhiteOo | BlackOo,
    QueenSide = WhiteOoo | BlackOoo,
    WhiteCastleling = WhiteOo | WhiteOoo,
    BlackCastleling = BlackOo | BlackOoo,

    Any = WhiteCastleling | BlackCastleling,
    CastleRightsNb = 16
}

public static class CastlelingSideExtensions
{
    public static CastlelingRights MakeCastlelingRights(this CastlelingRights cs, Player p)
        => p.IsWhite
            ? cs == CastlelingRights.QueenSide
                ? CastlelingRights.WhiteOoo
                : CastlelingRights.WhiteOo
            : cs == CastlelingRights.QueenSide
                ? CastlelingRights.BlackOoo
                : CastlelingRights.BlackOo;
}

public enum CastlelingPerform
{
    Do,
    Undo
}

public static class CastlelingExtensions
{
    public static string GetCastlelingString(Square toSquare, Square fromSquare) => toSquare < fromSquare ? "O-O-O" : "O-O";

    public static bool HasFlagFast(this CastlelingRights value, CastlelingRights flag) => (value & flag) != 0;

    public static int AsInt(this CastlelingRights value) => (int)value;

    public static CastlelingRights Without(this CastlelingRights @this, CastlelingRights remove)
        => @this & ~remove;
}

public readonly struct CastleRight : IEnumerable<CastlelingRights>, IEquatable<CastleRight>
{
    private CastleRight(CastlelingRights cr) => Rights = cr;

    private CastleRight(int cr) => Rights = (CastlelingRights)cr;

    public CastlelingRights Rights { get; }

    public bool None => Rights == CastlelingRights.None;

    public static CastleRight NONE = new(CastlelingRights.None);

    public static CastleRight WHITE_OO = new(CastlelingRights.WhiteOo);

    public static CastleRight BLACK_OO = new(CastlelingRights.BlackOo);

    public static CastleRight WHITE_OOO = new(CastlelingRights.WhiteOoo);

    public static CastleRight BLACK_OOO = new(CastlelingRights.BlackOoo);

    public static CastleRight KING_SIDE = new(CastlelingRights.KingSide);

    public static CastleRight QUEEN_SIDE = new(CastlelingRights.QueenSide);

    public static CastleRight WHITE_CASTLELING = new(CastlelingRights.WhiteCastleling);

    public static CastleRight BLACK_CASTLELING = new(CastlelingRights.BlackCastleling);

    public static CastleRight ANY = new(CastlelingRights.Any);

    public static implicit operator CastleRight(CastlelingRights cr) => new(cr);

    public bool Equals(CastleRight other)
        => Rights == other.Rights;

    public IEnumerator<CastlelingRights> GetEnumerator()
    {
        if (None)
            yield break;

        var cr = (int)Rights;
        while (cr != 0)
            yield return (CastlelingRights)BitBoards.PopLsb(ref cr);
    }

    public override bool Equals(object obj)
        => obj is CastleRight other && Equals(other);

    public override int GetHashCode()
        => (int)Rights;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(CastleRight cr1, CastleRight cr2) => cr1.Rights == cr2.Rights;

    public static bool operator !=(CastleRight cr1, CastleRight cr2) => cr1.Rights != cr2.Rights;

    public static bool operator true(CastleRight cr) => cr.Rights != CastlelingRights.None;

    public static bool operator false(CastleRight cr) => cr.Rights == CastlelingRights.None;

    public static CastleRight operator |(CastleRight cr1, CastleRight cr2) => cr1.Rights | cr2.Rights;

    public static CastleRight operator |(CastleRight cr1, CastlelingRights cr2) => cr1.Rights | cr2;

    public static CastleRight operator ^(CastleRight cr1, CastleRight cr2) => cr1.Rights ^ cr2.Rights;

    public static CastleRight operator ^(CastleRight cr1, CastlelingRights cr2) => cr1.Rights ^ cr2;

    public static CastleRight operator &(CastleRight cr1, CastleRight cr2) => cr1.Rights & cr2.Rights;

    public static CastleRight operator &(CastleRight cr1, CastlelingRights cr2) => cr1.Rights & cr2;

    public static CastleRight operator ~(CastleRight cr) => ~cr.Rights;

    public HashKey Key() => Rights.GetZobristCastleling();

    public bool Has(CastlelingRights cr) => Rights.HasFlagFast(cr);

    public CastleRight Not(CastlelingRights cr)
        => Rights & ~cr;
}