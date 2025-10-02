using System;
using System.IO;
using System.Text;

namespace QuickLauncher
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuickLauncher",
            "logs",
            $"quicklauncher_{DateTime.Now:yyyyMMdd}.log"
        );

        private static readonly object _lock = new object();
        private static bool _isEnabled = true;
        private static LogLevel _minLevel = LogLevel.Info;

        public static bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public static LogLevel MinimumLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        static Logger()
        {
            try
            {
                string? directory = Path.GetDirectoryName(LogPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Clean up old log files (keep last 7 days)
                CleanupOldLogs(directory!, 7);
            }
            catch
            {
                // If we can't set up logging, just disable it
                _isEnabled = false;
            }
        }

        public static void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public static void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public static void Error(string message, Exception? ex = null)
        {
            if (ex != null)
            {
                message = $"{message}\nException: {ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            Log(LogLevel.Error, message);
        }

        private static void Log(LogLevel level, string message)
        {
            if (!_isEnabled || level < _minLevel)
                return;

            try
            {
                lock (_lock)
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string logEntry = $"[{timestamp}] [{level}] {message}";
                    
                    File.AppendAllText(LogPath, logEntry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Silently fail if logging doesn't work
            }
        }

        private static void CleanupOldLogs(string logDirectory, int daysToKeep)
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                    return;

                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                
                foreach (string file in Directory.GetFiles(logDirectory, "quicklauncher_*.log"))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        fileInfo.Delete();
                    }
                }
            }
            catch
            {
                // Silently fail cleanup
            }
        }

        public static string GetLogPath()
        {
            return LogPath;
        }
    }
}
