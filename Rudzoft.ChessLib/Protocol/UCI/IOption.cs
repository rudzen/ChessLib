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

namespace Rudzoft.ChessLib.Protocol.UCI;

public interface IOption
{
    string Name { get; set; }
    UciOptionType Type { get; set; }
    string DefaultValue { get; set; }
    int Min { get; set; }
    int Max { get; set; }
    int Idx { get; set; }
    Action<IOption> OnChange { get; set; }
    int GetInt();
    string GetText();

    bool GetBool();

    /// <summary>
    /// Updates currentValue and triggers OnChange() action.
    /// It's up to the GUI to check for option's limits, but we could receive the new value from
    /// the user by console window, so let's check the bounds anyway.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    IOption SetCurrentValue(string v);
}