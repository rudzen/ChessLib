﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Minor Code Smell", "S3963:\"static\" fields should be initialized inline", Justification = "Needed for some functionality", Scope = "member", Target = "~M:Rudzoft.ChessLib.Evaluation.KpkBitBase.#cctor")]
[assembly: SuppressMessage("Critical Bug", "S2222:Locks should be released", Justification = "Should be alright", Scope = "member", Target = "~M:Rudzoft.ChessLib.Protocol.UCI.InputOutput.Write(System.String,Rudzoft.ChessLib.Protocol.UCI.InputOutputMutex)")]
