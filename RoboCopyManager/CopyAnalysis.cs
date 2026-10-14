using System;
using System.Collections.Generic;

namespace RoboCopyManager
{
    public enum CopyAction
    {
        Missing,
        Same,
        Different
    }

    public enum CopyDecision
    {
        Skip,
        Overwrite
    }

    public class CopyFileInfo
    {
        public string SourcePath { get; set; } = string.Empty;

        public string DestinationPath { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;

        public long SourceSize { get; set; }

        public long DestinationSize { get; set; }

        public DateTime SourceModified { get; set; }

        public DateTime DestinationModified { get; set; }

        public CopyAction Action { get; set; }

        public CopyDecision Decision { get; set; }

        public bool IsOverwriteSelected
        {
            get => Decision == CopyDecision.Overwrite;
            set => Decision =
                value
                    ? CopyDecision.Overwrite
                    : CopyDecision.Skip;
        }
    }


    public class CopyAnalysisResult
    {
        public List<CopyFileInfo> MissingFiles { get; set; } = new();

        public List<CopyFileInfo> SameFiles { get; set; } = new();

        public List<CopyFileInfo> DifferentFiles { get; set; } = new();

        public long TotalBytesToCopy { get; set; }

        public int TotalFiles =>
            MissingFiles.Count +
            SameFiles.Count +
            DifferentFiles.Count;
    }

    public class CopyPlan
    {
        public List<CopyFileInfo> FilesToCopy { get; set; } = new();

        public long TotalBytes { get; set; }

        public int TotalFiles =>
            FilesToCopy.Count;
    }
}