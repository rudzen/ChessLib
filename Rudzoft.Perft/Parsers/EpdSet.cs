/*
Perft, a chess perft testing application

MIT License

Copyright (c) 2019-2022 Rudy Alex Kohn

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
using System.Collections.Generic;
using Rudzoft.ChessLib.Perft.Interfaces;

namespace Rudzoft.Perft.Parsers;

public sealed class EpdSet : IEpdSet
{
    public string Id { get; set; }

    public string Epd { get; set; }

    public List<PerftPositionValue> Perft { get; set; }

    public bool Equals(IEpdSet x, IEpdSet y)
    {
        var idComparison = string.Equals(x.Id, y.Id, StringComparison.Ordinal);

        if (!idComparison)
            return false;

        var epdComparison = string.Equals(x.Epd, y.Epd, StringComparison.Ordinal);

        return epdComparison;
    }

    public int GetHashCode(IEpdSet obj)
        => Id.GetHashCode(StringComparison.Ordinal) ^ Epd.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(IEpdSet other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        var idComparison = string.Compare(Id, other.Id, StringComparison.Ordinal);

        return idComparison != 0
            ? idComparison
            : string.Compare(Epd, other.Epd, StringComparison.Ordinal);
    }
}