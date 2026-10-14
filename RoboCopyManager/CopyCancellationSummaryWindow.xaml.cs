using System;
using System.Windows;

namespace RoboCopyManager
{
    public partial class CopyCancellationSummaryWindow : Window
    {
        public CopyCancellationSummaryWindow(
            CopyCancellationSummary summary)
        {
            InitializeComponent();

            TotalFilesText.Text =
                summary.TotalFiles.ToString("N0");

            FullyCopiedText.Text =
                summary.FullyCopied.ToString("N0");

            PartiallyCopiedText.Text =
                summary.PartiallyCopied.ToString("N0");

            NotCopiedText.Text =
                summary.NotCopied.ToString("N0");

            double transferredMB =
                summary.DataTransferredBytes / (1024.0 * 1024.0);

            DataTransferredText.Text = transferredMB >= 1024
                ? $"{transferredMB / 1024.0:F2} GB"
                : $"{transferredMB:F2} MB";

            DurationText.Text =
                summary.Duration.ToString(@"hh\:mm\:ss");
        }

        private void Close_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}