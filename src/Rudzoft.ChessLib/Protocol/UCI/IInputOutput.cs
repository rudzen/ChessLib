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

using System.IO;
using System.Threading.Tasks;

namespace Rudzoft.ChessLib.Protocol.UCI;

public interface IInputOutput
{
    TextReader Input { get; set; }
    TextWriter Output { get; set; }
    string LastLineRead { get; set; }
    string ReadLine(InputOutputMutex action = InputOutputMutex.None);
    Task<string> ReadLineAsync(InputOutputMutex action = InputOutputMutex.None);
    string ReadWord(InputOutputMutex action = InputOutputMutex.None);
    Task<string> ReadWordAsync(InputOutputMutex action = InputOutputMutex.None);
    void InitSync();
    void EndSync();
    void Write(string cad, InputOutputMutex action = InputOutputMutex.None);
    Task WriteAsync(string cad, InputOutputMutex action = InputOutputMutex.None);
    void WriteLine(string cad, InputOutputMutex action = InputOutputMutex.None);
    Task WriteLineAsync(string cad, InputOutputMutex action = InputOutputMutex.None);
}
