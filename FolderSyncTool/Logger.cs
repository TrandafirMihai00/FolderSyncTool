using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSyncTool
{
    internal class Logger
    {
        private readonly string logFilePath;

        public Logger(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }

        public void LogMessage(string message)
        {
            string logEntry = $"{DateTime.Now}: {message}";
            Console.WriteLine(logEntry);
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}
