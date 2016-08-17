using System;
using System.Configuration.Install;
using System.IO;

namespace Eyedrivomatic.Setup.Actions
{
    internal class Log : IDisposable
    {
        //private TextWriter _logWritter;
        private InstallContext _context;
        private static Log _instance;

        internal static void Initialize(string path, InstallContext context)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(nameof(path));

            _instance = new Log(path, context);
        }

        private Log(string path, InstallContext context)
        {
            _context = context;

            var logDir = Path.GetDirectoryName(path);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            //_logWritter = File.CreateText(path);
        }

        public static void Debug(string message)
        {
            _instance?.WriteMessage($"DEBUG: {message}");
        }

        public static void Info(string message)
        {
            _instance?.WriteMessage($"DEBUG: {message}");
        }

        public static void Warning(string message)
        {
            _instance?.WriteMessage($"WARNING: {message}");
        }

        public static void Error(string message)
        {
            _instance?.WriteMessage($"ERROR: {message}");
        }

        private void WriteMessage(string message)
        {
            _context?.LogMessage(message);
            //_logWritter.WriteLine(message);
        }

        public void Dispose()
        {
            //_logWritter.Dispose();
        }

        public static void Close()
        {
            var tmp = _instance;
            _instance = null;
            tmp.Dispose();
        }
    }
}
