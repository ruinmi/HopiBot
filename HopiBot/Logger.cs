using System;
using System.Diagnostics;
using System.IO;

namespace HopiBot
{
    public static class Logger
    {
        // log to desktop
        private static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "HopiBot.log");

        public static void Log(string message)
        {
            string logMessage = $"{DateTime.Now} {GetCallerInfo()} {message}";
            WriteToFile(logMessage);
        }

        public static void Log(string message, Exception ex)
        {
            string fullMessage = $"{message}\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
            Log(fullMessage);
        }

        private static void WriteToFile(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        private static string GetCallerInfo()
        {
            var stackTrace = new StackTrace(true);
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                var declaringType = method.DeclaringType;

                if (declaringType != typeof(Logger))
                {
                    var namespaceName = declaringType.Namespace;
                    var className = declaringType.Name;
                    var methodName = method.Name;
                    var fileName = frame.GetFileName();
                    var lineNumber = frame.GetFileLineNumber();
                    return $"{namespaceName}.{className}.{methodName} (at {fileName}:{lineNumber})";
                }
            }
            return "Unknown Caller";
        }
    }
}
