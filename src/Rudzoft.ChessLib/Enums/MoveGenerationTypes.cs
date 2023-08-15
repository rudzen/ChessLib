/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

using System.Runtime.CompilerServices;

namespace Rudzoft.ChessLib.Enums;

/// <summary>
/// Move generation flag
/// </summary>
[Flags]
public enum MoveGenerationTypes
{
    None = 0,
    
    /// <summary>
    /// Generate all legal moves
    /// </summary>
    Legal = 1,

    /// <summary>
    /// Generate only captures
    /// </summary>
    Captures = 2,

    /// <summary>
    /// Generate only quiet moves (non-captures)
    /// </summary>
    Quiets = 4,

    /// <summary>
    /// Generate only moves which are not evasions
    /// </summary>
    NonEvasions = 8,

    /// <summary>
    /// Generate only evasion moves (if fx in check)
    /// </summary>
    Evasions = 16,

    /// <summary>
    /// Generate only moves which are not captures and gives check
    /// </summary>
    QuietChecks = 32,
    
    All = Legal | Captures | Quiets | NonEvasions | Evasions | QuietChecks
}

public static class MoveGenerationTypesExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this MoveGenerationTypes value, MoveGenerationTypes flag)
    {
        return (value & flag) != 0;
    }
}
