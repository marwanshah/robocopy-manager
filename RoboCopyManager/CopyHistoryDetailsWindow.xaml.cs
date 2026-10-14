using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace RoboCopyManager
{
    public partial class CopyHistoryDetailsWindow : Window
    {
        private readonly CopyHistoryEntry historyEntry;

        public CopyHistoryDetailsWindow(CopyHistoryEntry entry)
        {
            InitializeComponent();

            historyEntry = entry;

            LoadHistoryDetails();
        }

        private void LoadHistoryDetails()
        {
            SourceText.Text = historyEntry.Source;
            DestinationText.Text = historyEntry.Destination;

            TotalFilesText.Text =
                historyEntry.TotalFiles.ToString("N0");

            CopiedFilesText.Text =
                historyEntry.CopiedFiles.ToString("N0");

            FailedFilesText.Text =
                historyEntry.FailedFiles.ToString("N0");

            int notProcessedFiles = Math.Max(
                0,
                historyEntry.TotalFiles
                - historyEntry.CopiedFiles
                - historyEntry.FailedFiles);

            NotProcessedFilesText.Text =
                historyEntry.Status.Equals(
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase)
                    ? notProcessedFiles.ToString("N0")
                    : "0";

            DataTransferredText.Text =
                FormatBytes(historyEntry.BytesTransferred);

            DurationText.Text =
                TimeSpan.FromSeconds(
                    Math.Max(0, historyEntry.DurationSeconds))
                .ToString(@"hh\:mm\:ss");

            StartTimeText.Text =
                historyEntry.StartTime.ToString("dd/MM/yyyy hh:mm:ss tt");

            EndTimeText.Text =
                historyEntry.EndTime?.ToString("dd/MM/yyyy hh:mm:ss tt")
                ?? "In progress";

            RemarksText.Text =
                string.IsNullOrWhiteSpace(historyEntry.Remarks)
                ? "No remarks"
                : historyEntry.Remarks;

            StatusText.Text = historyEntry.Status;

            if (historyEntry.Status.Equals(
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                StatusBadge.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(255, 237, 213));

                StatusText.Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(194, 65, 12));

                StatusDot.Fill =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(249, 115, 22));
            }
            else if (historyEntry.Status.Equals(
                "Failed",
                StringComparison.OrdinalIgnoreCase))
            {
                StatusBadge.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(254, 226, 226));

                StatusText.Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(185, 28, 28));

                StatusDot.Fill =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(220, 38, 38));
            }
            else if (historyEntry.Status.Equals(
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                StatusBadge.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(220, 252, 231));

                StatusText.Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(22, 101, 52));

                StatusDot.Fill =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(22, 163, 74));
            }
            else
            {
                StatusBadge.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(254, 243, 199));

                StatusText.Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(146, 64, 14));

                StatusDot.Fill =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(217, 119, 6));
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 0)
                bytes = 0;

            string[] units = { "bytes", "KB", "MB", "GB", "TB" };

            double size = bytes;
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:N2} {units[unitIndex]}";
        }

        private void OpenFullLog_Click(object sender, RoutedEventArgs e)
        {
            string logPath = historyEntry.LogFilePath;

            if (string.IsNullOrWhiteSpace(logPath) ||
                !File.Exists(logPath))
            {
                MessageBox.Show(
                    "The log file could not be found.",
                    "Log Unavailable",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open the log file.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}