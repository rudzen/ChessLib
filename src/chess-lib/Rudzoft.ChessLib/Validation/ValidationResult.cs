//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2022 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2025 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

namespace Rudzoft.ChessLib.Validation;

using System;
using System.Threading.Tasks;

#nullable enable

/// <summary>
/// A result type frequently used inside Akka.Streams and elsewhere.
/// </summary>
public readonly record struct ValidationResult
{
    /// <summary>
    /// <c>true</c> if the result is successful, <c>false</c> otherwise.
    /// </summary>
    public readonly bool IsSuccess;

    /// <summary>
    /// <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.
    /// </summary>
    public readonly string? Error;

    public ValidationResult()
    {
        IsSuccess = true;
        Error = null;
    }

    public ValidationResult(string error) : this()
    {
        IsSuccess = false;
        Error = error;
    }

    public override string ToString() => IsSuccess ? "Success" : $"Failure ({Error})";
}

/// <summary>
/// Helper methods for creating <see cref="ValidationResult{T}"/> instances.
/// </summary>
public static class ValidationResults
{
    public static ValidationResult Success(bool value)
    {
        return new();
    }

    public static ValidationResult Failure(string error)
    {
        return new(error);
    }

    public static ValidationResult From(Func<bool> func)
    {
        try
        {
            var value = func();
            return new();
        }
        catch (Exception e)
        {
            return new(e.Message);
        }
    }
}