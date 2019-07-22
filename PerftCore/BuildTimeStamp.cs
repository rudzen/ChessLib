namespace Perft
{
    using System;
    using System.IO;
    using System.Reflection;

    public static class BuildTimeStamp
    {
        private static readonly Lazy<string> _buildTimeStamp = new Lazy<string>(GetTimestamp);

        public static string TimeStamp => _buildTimeStamp.Value;

        private static string GetTimestamp()
        {
            var assembly = Assembly.GetEntryAssembly();

            var stream = assembly.GetManifestResourceStream("Perft.BuildTimeStamp.txt");

            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd().TrimEnd('\r', '\n').TrimEnd();
        }
    }
}