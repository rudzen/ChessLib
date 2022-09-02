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
using System.Diagnostics;

namespace Rudzoft.ChessLib.Protocol.UCI;

public sealed class CPU
{
    private const int Interval = 500;

    private const double DefaultOverflow = double.MinValue;

    private readonly Process _currentProcessName;

    private readonly int _numProcessors;

    private DateTime _lastCpu;

    private TimeSpan _lastSysCpu;

    private TimeSpan _lastUserCpu;

    public CPU()
    {
        _currentProcessName = Process.GetCurrentProcess();
        _numProcessors = Environment.ProcessorCount;
    }

    public double CpuUse { get; set; }

    public double Usage()
    {
        var now = DateTime.UtcNow;
        var total = _currentProcessName.TotalProcessorTime;
        var user = _currentProcessName.UserProcessorTime;

        double percentage;

        if (now <= _lastCpu || total.Milliseconds < _lastSysCpu.Milliseconds ||
            user.Milliseconds < _lastUserCpu.Milliseconds)
        {
            //Overflow detection. Just skip this value.
            percentage = DefaultOverflow;
        }
        else
        {
            percentage = (total - _lastSysCpu + user - _lastUserCpu).Milliseconds;
            percentage /= (now - _lastCpu).Milliseconds;
            percentage /= _numProcessors;
        }

        _lastCpu = now;
        _lastSysCpu = total;
        _lastUserCpu = user;
        CpuUse = percentage;

        return percentage switch
        {
            <= 0 => 0,
            _ => Math.Round(percentage * 1000, MidpointRounding.ToEven)
        };
    }
}