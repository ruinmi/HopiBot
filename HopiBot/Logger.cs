using System;
using System.IO;

namespace HopiBot
{
    public static class Logger
    {
        // log to desktop
        private static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "HopiBot.log");

        public static void Log(string message)
        {
            string logMessage = $"{DateTime.Now} {message}";
            WriteToFile(logMessage);
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

        public static void Log(string message, Exception ex)
        {
            string fullMessage = $"{message}\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
            Log(fullMessage);
        }
    }
}
