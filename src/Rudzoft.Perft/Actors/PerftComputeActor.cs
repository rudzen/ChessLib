// using System.Diagnostics;
// using Akka.Actor;
// using Microsoft.Extensions.DependencyInjection;
// using Rudzoft.ChessLib.Perft.Interfaces;
// using Rudzoft.Perft2.Models;
//
// namespace Rudzoft.Perft2.Actors;
//
// public sealed class PerftComputeActor : ReceiveActor
// {
//     public sealed record PerftPositions(List<PerftPosition> Positions);
//     
//     private readonly IPerft _perft;
//
//     public PerftComputeActor(IServiceProvider sp)
//     {
//         _perft = sp.GetRequiredService<IPerft>();
//         ReceiveAsync<PerftPositions>(async pp =>
//         {
//             var result = await ComputePerft(pp.Positions).ConfigureAwait(false);
//             Sender.Tell(result);
//         });
//     }
//     
//     private async Task<PerftResult> ComputePerft(CancellationToken cancellationToken)
//     {
//         var result = _resultPool.Get();
//         result.Clear();
//
//         var pp = _perft.Positions[^1];
//         var baseFileName = SaveResults
//             ? Path.Combine(CurrentDirectory.Value, $"{FixFileName(pp.Fen)}[")
//             : string.Empty;
//
//         var errors = 0;
//
//         result.Fen = pp.Fen;
//         _perft.SetGamePosition(pp);
//         _perft.BoardPrintCallback(_perft.GetBoard());
//         _log.Information("Fen         : {Fen}", pp.Fen);
//         _log.Information(Line);
//
//         foreach (var (depth, expected) in pp.Value)
//         {
//             cancellationToken.ThrowIfCancellationRequested();
//
//             _log.Information("Depth       : {Depth}", depth);
//
//             var start = Stopwatch.GetTimestamp();
//
//             var perftResult = await _perft.DoPerftAsync(depth).ConfigureAwait(false);
//             var elapsedMs = Stopwatch.GetElapsedTime(start);
//
//             ComputeResults(in perftResult, depth, in expected, in elapsedMs, result);
//
//             errors += LogResults(result);
//
//             if (baseFileName.IsNullOrEmpty())
//                 continue;
//
//             await WriteOutput(result, baseFileName, cancellationToken);
//         }
//
//         _log.Information("{Info} parsing complete. Encountered {Errors} errors", _usingEpd ? "EPD" : "FEN", errors);
//
//         result.Errors = errors;
//
//         return result;
//     }
// }