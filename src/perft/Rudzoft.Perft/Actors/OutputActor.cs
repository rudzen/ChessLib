using System.Runtime.CompilerServices;
using System.Text.Json;
using Akka.Actor;
using Rudzoft.Perft.Models;
using Serilog;

namespace Rudzoft.Perft.Actors;

public sealed class OutputActor : ReceiveActor
{
    private static readonly ILogger Log = Serilog.Log.ForContext<OutputActor>();

    private readonly bool _saveResult;

    public OutputActor(IServiceProvider sp)
    {
        _saveResult = true;
        ReceiveAsync<PerftResult>(Result);
    }

    private async Task Result(PerftResult perftResult)
    {
        Log.Information("{PerftResult}", perftResult);
        var baseFileName = _saveResult
            ? Path.Combine(Environment.CurrentDirectory, $"{FixFileName(perftResult.Fen)}[")
            : string.Empty;

        await WriteOutput(perftResult, baseFileName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FixFileName(string input)
        => input.Replace('/', '_');

    private static async Task WriteOutput(
        PerftResult result,
        string baseFileName)
    {
        var outputFileName = Path.Combine(Environment.CurrentDirectory, $"{baseFileName}{result.Depth}].json");
        await using var outStream = File.OpenWrite(outputFileName);
        await JsonSerializer.SerializeAsync(outStream, result);
    }
}