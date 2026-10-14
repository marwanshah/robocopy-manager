using System;
using System.Windows;

namespace RoboCopyManager
{
    public partial class CopySummaryWindow : Window
    {
        public CopySummaryWindow(CopyResult result)
        {
            InitializeComponent();

            if (result.IsSuccessful)
            {
                TitleText.Text = "Copy Completed";

                StatusText.Text =
                    "The copy operation completed successfully.";
            }
            else if (result.WasCancelled)
            {
                TitleText.Text = "Copy Cancelled";

                StatusText.Text =
                    "The copy operation was cancelled.";
            }
            else
            {
                TitleText.Text = "Copy Failed";

                StatusText.Text =
                    "The copy operation completed with errors.";
            }

            NewFilesText.Text =
                result.NewFilesCopied.ToString();

            OverwrittenText.Text =
                result.FilesOverwritten.ToString();

            SkippedText.Text =
                result.FilesSkipped.ToString();

            FailedText.Text =
                result.FailedFiles.ToString();

            DataCopiedText.Text =
                FormatBytes(result.BytesCopied);

            DurationText.Text =
                result.Duration.ToString(@"hh\:mm\:ss");

            AverageSpeedText.Text =
                $"{result.AverageSpeedMBPerSecond:F1} MB/s";
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";

            if (bytes < 1024L * 1024L * 1024L)
                return $"{bytes / (1024.0 * 1024.0):F2} MB";

            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }

        private void Ok_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}