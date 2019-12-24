using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCoveCapital.Core
{
    public class LogEntry
    {
        public LogEntry() { }

        public LogEntry(LogEntryImportance importance, DateTime dateTime, string colleague, string message)
        {
            DateTime = dateTime;
            Colleague = colleague;
            Message = message;
            Importance = importance.ToString();
            ThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public DateTime DateTime { get; set; }
        public string Colleague { get; set; }
        public string Message { get; set; }
        public string Importance { get; set; }
        public int ThreadID { get; set; }
    }
    public enum LogEntryImportance
    {
        Info = 1,
        Debug = 2,
        Error = 3,
    }
}
