/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Rudz.Chess.UCI
{
    using Extensions;
    using System;
    using System.Diagnostics;

    public sealed class Option : IOption
    {
        private string _currentValue;

        public Option()
        {
            Name = DefaultValue = _currentValue = string.Empty;
            Min = Max = Idx = 0;
        }

        public Option(string name, int indices, Action<IOption> func = null)
        {
            Name = name;
            Type = UciOptionType.Button;
            Min = Max = 0;
            Idx = indices;
            OnChange = func;
        }

        public Option(string name, int indices, bool v, Action<IOption> func = null)
        {
            Name = name;
            Type = UciOptionType.Check;
            Min = Max = 0;
            Idx = indices;
            OnChange = func;
            DefaultValue = _currentValue = v.ToString();
        }

        public Option(string name, int indices, string v, Action<IOption> func = null)
        {
            Name = name;
            Type = UciOptionType.Text;
            Min = Max = 0;
            Idx = indices;
            OnChange = func;
            DefaultValue = _currentValue = v;
        }

        public Option(string name, int indices, int v, int minValue, int maxValue, Action<IOption> func = null)
        {
            Name = name;
            Type = UciOptionType.Spin;
            Min = minValue;
            Max = maxValue;
            Idx = indices;
            OnChange = func;
            DefaultValue = _currentValue = v.ToString();
        }

        public string Name { get; set; }

        public UciOptionType Type { get; set; }

        public string DefaultValue { get; set; }

        public int Min { get; set; }

        public int Max { get; set; }

        public int Idx { get; set; }

        public Action<IOption> OnChange { get; set; }

        public int GetInt()
        {
            Debug.Assert(Type == UciOptionType.Check || Type == UciOptionType.Spin);
            return Type == UciOptionType.Spin ? Convert.ToInt32(_currentValue) : bool.Parse(_currentValue).ToInt();
        }

        public string GetText()
        {
            Debug.Assert(Type != UciOptionType.Check || Type != UciOptionType.Spin);
            return _currentValue;
        }

        /// <summary>
        /// Updates _currentValue and triggers OnChange() action.
        /// It's up to the GUI to check for option's limits, but we could receive the new value from
        /// the user by console window, so let's check the bounds anyway.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public IOption SetCurrentValue(string v)
        {
            if (Type != UciOptionType.Button && (v == null || string.IsNullOrEmpty(v))
                || Type == UciOptionType.Check && !bool.TryParse(v, out var r)
                || Type == UciOptionType.Spin)
            {
                v.ToIntegral(out int val);
                if (val < Min || val > Max)
                    return this;
            }

            if (Type != UciOptionType.Button)
                _currentValue = v;

            OnChange?.Invoke(this);

            return this;
        }
    }
}