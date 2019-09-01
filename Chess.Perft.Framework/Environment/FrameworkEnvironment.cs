namespace Perft.Environment
{
    using Chess.Perft.Environment;
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security.Principal;

    public sealed class FrameworkEnvironment : IFrameworkEnvironment
    {
        [DllImport("libc")]
        public static extern uint getuid();

        #region Public Properties

        private static readonly string FrameWork = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

        public bool IsDevelopment { get; set; } = true;

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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var identity = WindowsIdentity.GetCurrent())
                    {
                        var principal = new WindowsPrincipal(identity);
                        return principal.IsInRole(WindowsBuiltInRole.Administrator);
                    }
                }

                return getuid() == 0;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to determine administrator or root status", ex);
            }
        }
    }
}