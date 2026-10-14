using System;

namespace RoboCopyManager
{
    public class CopyCancellationSummary
    {
        public int TotalFiles { get; set; }

        public int FullyCopied { get; set; }

        public int PartiallyCopied { get; set; }

        public int NotCopied { get; set; }

        public long DataTransferredBytes { get; set; }

        public TimeSpan Duration { get; set; }
    }
}