/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

namespace Perft
{
    using System;
    using System.IO;
    using System.Reflection;

    public static class BuildTimeStamp
    {
        private static readonly Lazy<string> _buildTimeStamp = new Lazy<string>(GetTimestamp);

        public static string TimeStamp => _buildTimeStamp.Value;

        private static string GetTimestamp()
        {
            var assembly = Assembly.GetEntryAssembly();

            var stream = assembly.GetManifestResourceStream("Perft.BuildTimeStamp.txt");

            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd().TrimEnd('\r', '\n').TrimEnd();
        }
    }
}