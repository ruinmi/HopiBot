using System;
using System.IO;

namespace HopiBot
{
    public class Logger
    {
        private string logFilePath;
        private bool logToFile;

        public Logger(string filePath = null)
        {
            logFilePath = filePath;
            logToFile = !string.IsNullOrEmpty(filePath);
        }

        public void Log(string message)
        {
            string logMessage = $"{DateTime.Now} {message}";
            if (logToFile)
            {
                WriteToFile(logMessage);
            }
            else
            {
                Console.WriteLine(logMessage);
            }
        }

        private void WriteToFile(string message)
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

        public void Log(string message, Exception ex)
        {
            string fullMessage = $"{message}\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
            Log(fullMessage);
        }
    }
}
