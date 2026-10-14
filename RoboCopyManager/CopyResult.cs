using System;

namespace RoboCopyManager
{
    public class CopyResult
    {
        public int FilesAnalyzed { get; set; }

        public int NewFilesCopied { get; set; }

        public int FilesOverwritten { get; set; }

        public int FilesSkipped { get; set; }

        public int FailedFiles { get; set; }

        public long BytesCopied { get; set; }

        public TimeSpan Duration { get; set; }

        public double AverageSpeedMBPerSecond { get; set; }

        public bool WasCancelled { get; set; }

        public bool IsSuccessful { get; set; }
    }
}