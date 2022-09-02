/*
Perft, a chess perft test library

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
using System.Collections.Generic;
using System.Linq;

namespace Perft.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Concats parameters onto an object.
    /// The resulting array is a copy of the objects, which makes this fast for small array sizes.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static object[] ConcatParametersCopy(this object @this, params object[] args)
    {
        var result = new object[args.Length + 1];
        result[0] = @this;
        // forced to use Array copy as there is no way to get a byte size of an managed object in c#
        // as per : http://blogs.msdn.com/cbrumme/archive/2003/04/15/51326.aspx
        Array.Copy(args, 0, result, 1, args.Length);
        return result;
    }

    /// <summary>
    /// Concats parameters onto an object.
    /// The resulting array is deferred object which means this will potentially be faster for larger object arrays
    /// </summary>
    /// <param name="this"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IEnumerable<object> ConcatParameters(this object @this, params object[] args)
    {
        var result = new[] { @this };
        return result.Concat(args);
    }
}