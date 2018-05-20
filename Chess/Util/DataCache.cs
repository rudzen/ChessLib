/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2018 Rudy Alex Kohn

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

namespace Rudz.Chess.Util
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// <para>Generic singular method result data cache.</para>
    /// <para>Construct idea from Bill Hilke</para>
    /// </summary>
    public static class DataCache
    {
        public static Func<TArg, TRet> Cache<TArg, TRet>(this Func<TArg, TRet> functor)
        {
            ConcurrentDictionary<TArg, TRet> table = new ConcurrentDictionary<TArg, TRet>();

            return arg0 =>
                {
                    if (table.TryGetValue(arg0, out TRet returnValue))
                        return returnValue;

                    returnValue = functor(arg0);
                    table.TryAdd(arg0, returnValue);

                    return returnValue;
                };
        }
    }
}