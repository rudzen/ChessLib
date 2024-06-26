/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2024 Rudy Alex Kohn

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

using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Rudzoft.ChessLib.PGN;

public sealed class PgnGenerator : IResettable
{
    private readonly StringBuilder _pgnBuilder = new();
    private readonly List<string> _moves = [];

    public PgnGenerator AddMetadata(
        string eventStr,
        string site,
        string date,
        string round,
        string whitePlayer,
        string blackPlayer,
        string result)
    {
        _pgnBuilder.AppendLine($"[Event \"{eventStr}\"]");
        _pgnBuilder.AppendLine($"[Site \"{site}\"]");
        _pgnBuilder.AppendLine($"[Date \"{date}\"]");
        _pgnBuilder.AppendLine($"[Round \"{round}\"]");
        _pgnBuilder.AppendLine($"[White \"{whitePlayer}\"]");
        _pgnBuilder.AppendLine($"[Black \"{blackPlayer}\"]");
        _pgnBuilder.AppendLine($"[Result \"{result}\"]");
        return this;
    }

    public PgnGenerator AddMove(string move)
    {
        _moves.Add(move);
        return this;
    }

    public string Build()
    {
        var s = CollectionsMarshal.AsSpan(_moves);
        for (var i = 0; i < s.Length; i += 2)
        {
            var moveNumber = (i / 2) + 1;
            _pgnBuilder.Append($"{moveNumber}. {s[i]} ");
            if (i + 1 < s.Length)
                _pgnBuilder.Append($"{s[i + 1]} ");
        }

        _pgnBuilder.AppendLine();
        return _pgnBuilder.ToString();
    }

    public bool TryReset()
    {
        _pgnBuilder.Clear();
        return true;
    }
}