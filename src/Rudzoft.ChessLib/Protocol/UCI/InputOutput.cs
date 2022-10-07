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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rudzoft.ChessLib.Protocol.UCI;

public sealed class InputOutput : IInputOutput
{
    private static readonly char[] SplitChar = { ' ' };

    private readonly Mutex _mutex;

    private string[] _words;
    private int _index = -1;

    public InputOutput()
    {
        _mutex = new Mutex();
        LastLineRead = string.Empty;
        _words = Array.Empty<string>();
    }

    public TextReader Input { get; set; }

    public TextWriter Output { get; set; }

    public string LastLineRead { get; set; }

    public string ReadLine(InputOutputMutex action = InputOutputMutex.None)
    {
        if (action.IsWaitable())
            _mutex.WaitOne();

        var line = Input.ReadLine();

        if (action.IsReleasable())
            _mutex.ReleaseMutex();

        return line;
    }

    public async Task<string> ReadLineAsync(InputOutputMutex action = InputOutputMutex.None)
    {
        if (action.IsWaitable())
            _mutex.WaitOne();

        var line = await Input.ReadLineAsync();

        if (action.IsReleasable())
            _mutex.ReleaseMutex();

        return line;
    }

    public string ReadWord(InputOutputMutex action = InputOutputMutex.None)
    {
        if (_index < 0 || _index == _words.Length)
        {
            _index = -1;
            var line = ReadLine(action);
            _words = line.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries);
        }

        _index++;
        return _index < _words.Length
            ? _words[_index]
            : string.Empty;
    }

    public async Task<string> ReadWordAsync(InputOutputMutex action = InputOutputMutex.None)
    {
        if (_index < 0 || _index == _words.Length)
        {
            _index = -1;
            LastLineRead = await ReadLineAsync(action).ConfigureAwait(false);
            _words = LastLineRead.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries);
        }

        _index++;
        return _index < _words.Length
            ? _words[_index]
            : string.Empty;
    }

    public void InitSync() => _mutex.WaitOne();

    public void EndSync() => _mutex.ReleaseMutex();

    public void Write(string cad, InputOutputMutex action = InputOutputMutex.None)
    {
        if (action.IsWaitable())
            _mutex.WaitOne();

        Output.Write(cad);

        if (action.IsReleasable())
            _mutex.ReleaseMutex();
    }

    public async Task WriteAsync(string cad, InputOutputMutex action = InputOutputMutex.None)
    {
        if (action.IsWaitable())
            _mutex.WaitOne();

        await Output.WriteAsync(cad).ConfigureAwait(false);

        if (action.IsReleasable())
            _mutex.ReleaseMutex();
    }

    public void WriteLine(string cad, InputOutputMutex action = InputOutputMutex.None)
        => Write($"{cad}{Environment.NewLine}", action);

    public Task WriteLineAsync(string cad, InputOutputMutex action = InputOutputMutex.None)
        => WriteAsync($"{cad}{Environment.NewLine}", action);
}