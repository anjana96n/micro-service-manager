using System;

namespace MicroserviceManager.Models
{
    /// <summary>
    /// A single line of captured output. Entries are held in memory only; the console
    /// TextBox is just a rendered, filtered view of the current entry list.
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Service { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsError { get; set; }

        public string Format()
        {
            var prefix = IsError ? "ERROR: " : string.Empty;
            return $"[{Timestamp:HH:mm:ss}] [{Service}] {prefix}{Text}";
        }
    }
}
