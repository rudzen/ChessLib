/*
Perft, a chess perft testing application

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

namespace Perft.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Fast epd file parser
    /// </summary>
    public class EpdParser : IEpdParser
    {
        public EpdParser(IEpdParserSettings settings)
        {
            Settings = settings;
        }

        public List<IEpdSet> Sets { get; set; }

        public IEpdParserSettings Settings { get; set; }

        public async Task<ulong> ParseAsync()
        {
            const char space = ' ';
            Sets = new List<IEpdSet>();
            using (var fs = File.Open(Settings.Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var bs = new BufferedStream(fs))
                {
                    using (var sr = new StreamReader(bs))
                    {
                        string s;
                        string id = string.Empty, epd = string.Empty;
                        bool idSet = false, epdSet = false;
                        var perftData = new List<string>(16);
                        while ((s = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                        {
                            // skip comments
                            if (s.Length < 4 || s[0] == '#')
                            {
                                if (idSet && epdSet)
                                {
                                    var p = new List<(int, ulong)>(perftData.Count);
                                    foreach (var pd in perftData)
                                    {
                                        var pp = ParsePerftLines(pd);
                                        p.Add(pp);
                                    }

                                    Sets.Add(new EpdSet {Epd = epd, Id = id, Perft = p});

                                    id = epd = string.Empty;
                                    idSet = epdSet = false;
                                    perftData.Clear();
                                }

                                continue;
                            }

                            if (!idSet & s[0] == 'i' && s[1] == 'd')
                            {
                                id = s.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                                idSet = true;
                                continue;
                            }

                            if (s[0] == 'e' & s[1] == 'p' & s[2] == 'd')
                            {
                                var firstSpace = s.IndexOf(space);
                                epd = s.Substring(firstSpace).TrimStart();
                                epdSet = true;
                                continue;
                            }

                            if (s.StartsWith("perft"))
                            {
                                var firstSpace = s.IndexOf(space);
                                perftData.Add(s.Substring(firstSpace));
                            }
                        }
                    }
                }
            }

            return (ulong)Sets.Count;
        }

        public ulong Parse()
        {
            return 0;
            //var count = 0UL;
            //const char space = ' ';
            //Sets = new List<IEpdSet>(1024);
            //using (var fs = File.Open(Settings.Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //{
            //    using (var bs = new BufferedStream(fs))
            //    {
            //        using (var sr = new StreamReader(bs))
            //        {
            //            string s;
            //            string id = string.Empty, epd = string.Empty;
            //            bool idSet = false, epdSet = false;
            //            var perftData = new List<string>(16);
            //            while ((s = sr.ReadLine()) != null)
            //            {
            //                // skip comments
            //                if (s.Length < 4 || s[0] == '#')
            //                {
            //                    if (idSet && epdSet)
            //                    {
            //                        var p = new List<(int, ulong)>(perftData.Count);
            //                        foreach (var pd in perftData)
            //                        {
            //                            var pp = ParsePerftLines(pd);
            //                            p.Add(pp);
            //                        }

            //                        Sets.Add(new EpdSet { Epd = epd, Id = id, Perft = p });
            //                        count++;

            //                        id = epd = string.Empty;
            //                        idSet = epdSet = false;
            //                        perftData.Clear();
            //                    }
            //                    continue;
            //                }

            //                if (!idSet & s[0] == 'i' && s[1] == 'd')
            //                {
            //                    id = s.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
            //                    idSet = true;
            //                    continue;
            //                }

            //                if (s[0] == 'e' & s[1] == 'p' & s[2] == 'd')
            //                {
            //                    var firstSpace = s.IndexOf(space);
            //                    epd = s.Substring(firstSpace).TrimStart();
            //                    epdSet = true;
            //                    continue;
            //                }

            //                if (s.StartsWith("perft"))
            //                {
            //                    var firstSpace = s.IndexOf(space);
            //                    perftData.Add(s.Substring(firstSpace));
            //                }
            //            }
            //        }
            //    }
            //}

            //return count;
        }

        private static (int, ulong) ParsePerftLines(string perftData)
        {
            var s = perftData.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var result = (depth: int.Parse(s[0]), count: ulong.Parse(s[1]));
            return result;
        }
    }
}