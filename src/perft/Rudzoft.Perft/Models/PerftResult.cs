﻿/*
Perft, a chess perft testing application

MIT License

Copyright (c) 2019-2023 Rudy Alex Kohn

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

using Microsoft.Extensions.ObjectPool;

namespace Rudzoft.Perft.Models;

public sealed class PerftResult : IResettable
{
    public string Id { get; set; }
    public string Fen { get; set; }
    public int Depth { get; set; }
    public UInt128 Result { get; set; }
    public UInt128 CorrectResult { get; set; }
    public TimeSpan Elapsed { get; set; }
    public ulong Nps { get; set; }
    public ulong TableHits { get; set; }
    public bool Passed { get; set; }
    public int Errors { get; set; }

    public bool TryReset()
    {
        Fen = string.Empty;
        Depth = Errors = 0;
        Result = CorrectResult = Nps = TableHits = ulong.MinValue;
        Elapsed = TimeSpan.Zero;
        Passed = false;
        return true;
    }
}