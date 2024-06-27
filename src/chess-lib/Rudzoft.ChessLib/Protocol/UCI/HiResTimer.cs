﻿/*
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

using System.Diagnostics;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Protocol.UCI;

/// <summary>
/// Idea From https://stackoverflow.com/a/41697139/548894
/// </summary>
public sealed class HiResTimer : IHiResTimer, IEquatable<HiResTimer>
{
    public static readonly double TickLength = 1000f / Stopwatch.Frequency; // ms

    public static readonly double Frequency = Stopwatch.Frequency;

    private static readonly TimeSpan RestartThreshold = TimeSpan.FromHours(1);

    private readonly object _intervalLock;

    private float _interval;

    private CancellationTokenSource _cancellationTokenSource;

    private Task _executer;

    public HiResTimer() => _intervalLock = new();

    public HiResTimer(int id)
        : this(1f, id, null) { }

    public HiResTimer(float interval, int id, Action<HiResTimerArgs> elapsed)
    {
        _intervalLock = new();
        Interval      = interval;
        Elapsed       = elapsed;
        Id            = id;
    }

    /// <summary>
    /// Invoked when the timer is elapsed
    /// </summary>
    public Action<HiResTimerArgs> Elapsed { get; set; }

    public static bool IsHighResolution => Stopwatch.IsHighResolution;

    public int Id { get; }

    public float Interval
    {
        get
        {
            lock (_intervalLock)
                return _interval;
        }

        set
        {
            if (value is < 0f or float.NaN)
                throw new ArgumentOutOfRangeException(nameof(value));
            lock (_intervalLock)
                _interval = value;
        }
    }

    public bool IsRunning => _cancellationTokenSource is { Token: { IsCancellationRequested: false, CanBeCanceled: true } };

    public bool UseHighPriorityThread { get; set; } = true;

    public static implicit operator HiResTimer(int id) => new(id);

    public static bool operator ==(HiResTimer left, HiResTimer right) => Equals(left, right);

    public static bool operator ==(HiResTimer left, int right) => left != null && left.Id == right;

    public static bool operator ==(HiResTimer left, Color right) => left != null && left.Id == right;

    public static bool operator !=(HiResTimer left, HiResTimer right) => !Equals(left, right);

    public static bool operator !=(HiResTimer left, int right) => left != null && left.Id != right;

    public static bool operator !=(HiResTimer left, Color right) => left != null && left.Id != right;

    public void Start()
    {
        if (IsRunning)
            Stop();

        Debug.Print($"Timer Start on thread {Environment.CurrentManagedThreadId}");

        _cancellationTokenSource = new();
        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
        _executer = Task.Run(() => ExecuteTimer(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();

        if (!IsRunning)
            return;

        Debug.WriteLine($"Timer Stop on thread {Environment.CurrentManagedThreadId}");
        if (_executer.IsCanceled || _executer.IsCompleted)
            return;

        try
        {
            _executer.Wait(500);
            Debug.WriteLine("Timer Stop called ");
        }
        catch (AggregateException ae)
        {
            Debug.Print(ae.Message);
        }
        catch (ObjectDisposedException ode)
        {
            Debug.Print(ode.Message);
        }
    }

    public override int GetHashCode() => Id;

    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return obj.GetType() == GetType() && Equals((HiResTimer)obj);
    }

    public bool Equals(HiResTimer other) => Id == other?.Id;

    private static double ElapsedHiRes(in long timeStamp) => Stopwatch.GetElapsedTime(timeStamp).Ticks * TickLength;

    private void ExecuteTimer(CancellationToken cancellationToken)
    {
        Debug.Print($"Timer ExecuteTimer on thread {Environment.CurrentManagedThreadId}");

        var nextTrigger = 0f;
        var timeStamp = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested)
        {
            nextTrigger += Interval;
            double elapsed;

            do
            {
                elapsed = ElapsedHiRes(in timeStamp);
                var diff = nextTrigger - elapsed;
                if (diff <= 0f)
                    break;

                switch (diff)
                {
                    case < 1f:
                        Thread.SpinWait(10);
                        break;
                    case < 5f:
                        Thread.SpinWait(100);
                        break;
                    case < 15f:
                        Thread.Sleep(1);
                        break;
                    default:
                        Thread.Sleep(10);
                        break;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;
            } while (true);

            if (Elapsed != null)
            {
                var delay = elapsed - nextTrigger;
                Elapsed(new(delay, Id));
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            // restarting the timer in every day to hour precision problems
            if (Stopwatch.GetElapsedTime(timeStamp) < RestartThreshold)
                continue;
            timeStamp = Stopwatch.GetTimestamp();
            nextTrigger = 0f;
        }
    }
}
