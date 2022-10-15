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

using Cysharp.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rudzoft.ChessLib.Types;

public sealed class StateStack : IEnumerable<State>
{
    private readonly State[] _stack;

    public StateStack(int size)
    {
        if (size < 1)
            throw new NotSupportedException("A Stack must have at least one element");

        Size = size;
        Count = 0;
        _stack = new State[size];
        for (var i = 0; i < _stack.Length; i++)
            Push(new State());
    }

    /// <summary>
    /// Max size of stack
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Current number of elements in the stack
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Get the current stack element without removing it
    /// </summary>
    /// <returns></returns>
    public State Peek()
    {
        var index = Count - 1;
        return index < 0 ? null : _stack[index];
    }

    /// <summary>
    /// Get state by negative offset; this is useful to look back in the current state stack
    /// </summary>
    /// <param name="offset">The negative offset from the current top of the stack</param>
    /// <returns>The stack with the offset based from the top of the stack</returns>
    /// <exception cref="InvalidOperationException">
    /// If the offset is either positive or will go below zero
    /// </exception>
    public State Peek(int offset)
    {
        if (offset >= 0)
            throw new InvalidOperationException("Offset can only be negative");

        var index = Count - 1 + offset;
        if (index < 0)
            throw new InvalidOperationException("Offset value reached before start of stack");

        return _stack[index];
    }

    /// <summary>
    /// Get the top stack element and remove it
    /// </summary>
    /// <returns>The top element of the stack</returns>
    /// <exception cref="InvalidOperationException">If the stack is empty</exception>
    public State Pop()
    {
        var index = Count - 1;
        if (index < 0)
            throw new InvalidOperationException("There are no elements on the stack.");

        Count--;
        return _stack[index];
    }

    /// <summary>
    /// Add a new element to the stack
    /// </summary>
    /// <param name="state">The new state to add</param>
    /// <exception cref="StackOverflowException">Stack size limit has been reached</exception>
    public void Push(State state)
    {
        if (Count == Size)
            throw new StackOverflowException();

        _stack[Count] = state;
        Count++;
    }

    public IEnumerator GetEnumerator()
        => _stack.GetEnumerator();

    public void CopyTo(State[] target, bool trim = false)
    {
        var max = trim ? Count : Size;
        target = new State[max];
        Array.Copy(_stack, target, max);
    }

    /// <summary>
    /// Copy the current stack as an array
    /// </summary>
    /// <returns></returns>
    public State[] ToArray()
    {
        var copy = new State[Count];
        Array.Copy(_stack, copy, Count);
        return copy;
    }

    public ReadOnlySpan<State> AsReadOnlySpan()
        => _stack.AsSpan()[..Count];

    IEnumerator<State> IEnumerable<State>.GetEnumerator()
        => _stack.TakeWhile(static state => state != null).GetEnumerator();

    public override string ToString()
    {
        using var sb = ZString.CreateStringBuilder();
        for (var i = Size; i > 0; i--)
        {
            var o = _stack[i];
            sb.Append(' ');
            if (o != null)
                sb.Append(o.ToString());
            else
                sb.Append(' ');
            sb.Append(' ');
            sb.Append('|');
        }

        return sb.ToString();
    }
}