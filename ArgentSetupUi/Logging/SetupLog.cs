using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Argent.SetupUi.Logging
{
    /// <summary>
    /// Append-only setup log matching legacy SETUP_LOG.TXT / SAMPLE.TXT format.
    /// </summary>
    public sealed class SetupLog
    {
        private static readonly SetupLog Instance = new SetupLog();
        private readonly object _lock = new object();
        private int _sequence;
        private bool _headerWritten;
        private readonly string _machine;
        private readonly string _user;
        private readonly int _processId;

        public static SetupLog Current => Instance;

        public string LogFilePath { get; }

        private SetupLog()
        {
            _machine = Environment.MachineName;
            _user = (Environment.UserName ?? string.Empty).ToUpperInvariant();
            _processId = Process.GetCurrentProcess().Id;

            var logsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Argent", "Setup", "LOGS");
            Directory.CreateDirectory(logsDir);
            LogFilePath = Path.Combine(logsDir, "SETUP_LOG.TXT");
        }

        public void EnsureHeader()
        {
            lock (_lock)
            {
                if (_headerWritten || File.Exists(LogFilePath))
                {
                    _headerWritten = true;
                    return;
                }

                var header =
                    Environment.NewLine + Environment.NewLine +
                    "Argent Job Scheduler 10.0-2401-64W-A Setup Log File" + Environment.NewLine +
                    "Copyright (c) 2024 Argent Software" + Environment.NewLine +
                    Environment.NewLine +
                    "This product is protected by one or more of the following U.S. Patents:" + Environment.NewLine +
                    Environment.NewLine +
                    "6483813; 511167; 511346; 530335; 543551; 553142; 553143; 553144;" + Environment.NewLine +
                    " 553626; 553627; 553628; 553629; 553630; 553631; 553635; 553636;" + Environment.NewLine +
                    " 553637; 562835; 562836; 562837." + Environment.NewLine +
                    Environment.NewLine +
                    "Please contact Support at Support@Argent.com or help.Argent.com." + Environment.NewLine;

                File.AppendAllText(LogFilePath, header);
                _headerWritten = true;
            }
        }

        public void LogSessionStart()
        {
            EnsureHeader();
            Log(string.Empty);
            LogLabel("Machine", _machine);
            Log(string.Empty);
            Log(string.Empty);
            LogLabel("User", Environment.UserName ?? string.Empty);
            Log(string.Empty);
            LogLabel("Product", "Argent Job Scheduler");
            Log(string.Empty);
            LogLabel("Version", "10.0-2401-A");
            Log(string.Empty);
            LogLabel("Program", Environment.Is64BitProcess ? "64bit" : "32bit");
            Log(string.Empty);
            Log("Setup UI session started");
        }

        public void Log(string message)
        {
            lock (_lock)
            {
                EnsureHeader();
                File.AppendAllText(LogFilePath, FormatLine(message) + Environment.NewLine);
            }
        }

        public void LogLabel(string label, string value)
        {
            Log(string.Format(CultureInfo.InvariantCulture, "{0,-20}{1}", label + ":", value ?? string.Empty));
        }

        public void LogQuoted(string label, string value)
        {
            Log(string.Format(CultureInfo.InvariantCulture, "{0} '{1}'", label, value ?? string.Empty));
        }

        private string FormatLine(string message)
        {
            var now = DateTime.Now;
            var timestamp = now.ToString("dd MMM yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            _sequence++;
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1} {2} ({3:D5}-{4:D5}) {5:D5} {6}",
                timestamp,
                _machine,
                _user,
                _processId,
                threadId,
                _sequence,
                message ?? string.Empty);
        }
    }
}
