/*
Perft, a chess perft test library

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

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Perft.Environment;

public sealed class FrameworkEnvironment : IFrameworkEnvironment
{
    [DllImport("libc")]
    // ReSharper disable once IdentifierTypo
    public static extern uint getuid();

    #region Public Properties

    private static readonly string FrameWork = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

    public bool IsDevelopment { get; set; }

    public string Configuration => IsDevelopment ? "Development" : "Production";

    public string DotNetFrameWork => FrameWork;

    public string Os => RuntimeInformation.OSDescription;

    public bool IsAdmin => CheckIsAdmin();

    public bool HighresTimer => Stopwatch.IsHighResolution;

    #endregion Public Properties

    #region Constructor

    public FrameworkEnvironment()
    {
#if RELEASE
        IsDevelopment = false;
#endif
    }

    #endregion Constructor

    private static bool CheckIsAdmin()
    {
        try
        {
            // Perform OS check
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return getuid() == 0;

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Unable to determine administrator or root status", ex);
        }
    }
}
