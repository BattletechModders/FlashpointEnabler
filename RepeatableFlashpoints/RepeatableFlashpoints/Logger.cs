using System;
using System.IO;

namespace RepeatableFlashpoints {
    public class Logger {
        private static StreamWriter LogStream;
        private readonly bool isDebug = false;

        public Logger(string modDir, string logName, bool isDebug) {
            string logFile = Path.Combine(modDir, $"{logName}.log");
            if (File.Exists(logFile)) {
                File.Delete(logFile);
            }

            LogStream = File.AppendText(logFile);
            LogStream.AutoFlush = true;

            this.isDebug = isDebug;
        }

        public void LogIfDebug(string message) {
            if (this.isDebug) {
                LogLine(message);
            }
        }

        public void LogLine(string message) {
            string now = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            LogStream.WriteLine($"{now} - {message}");
        }

        public void LogError(Exception error) {
            string now = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            LogStream.WriteLine($"{now} - {error}");
        }

        public void Close() {
            LogStream.Flush();
            LogStream.Close();
        }
    }
}