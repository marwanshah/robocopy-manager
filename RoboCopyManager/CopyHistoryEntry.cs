using System;

namespace RoboCopyManager
{
    public class CopyHistoryEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime StartTime { get; set; } = DateTime.Now;

        public DateTime? EndTime { get; set; }

        public string Source { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public string Status { get; set; } = "Analyzing";

        public string Remarks { get; set; } = string.Empty;

        public double DurationSeconds { get; set; }

        public int TotalFiles { get; set; }

        public int CopiedFiles { get; set; }

        public int FailedFiles { get; set; }

        public long BytesTransferred { get; set; }

        public string LogFilePath { get; set; } = string.Empty;
    }
}