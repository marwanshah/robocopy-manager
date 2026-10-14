using System.Windows;
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Text;

namespace RoboCopyManager
{
    public partial class HistoryWindow : Window
    {
        private readonly CopyHistoryService copyHistoryService =
        new CopyHistoryService();
        private CopyHistoryEntry? selectedHistoryEntry;
        public HistoryWindow()
        {
            InitializeComponent();
            RefreshHistory();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshHistory();
        }

        private void RefreshHistory()
        {
            try
            {
                var history = copyHistoryService.LoadHistory();

                HistoryDataGrid.ItemsSource = history;

                TotalAttemptsText.Text = history.Count.ToString();

                SelectedLogText.Text = history.Count == 0
                    ? "No history records found."
                    : "Select a history record to view its log.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load copy history.\n\n{ex.Message}",
                    "Copy History",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void HistoryDataGrid_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedHistoryEntry =
                HistoryDataGrid.SelectedItem as CopyHistoryEntry;

            if (selectedHistoryEntry == null)
            {
                SelectedLogText.Text =
                    "Select a history record to view its log.";

                return;
            }

            SelectedLogText.Text =
                File.Exists(selectedHistoryEntry.LogFilePath)
                    ? selectedHistoryEntry.LogFilePath
                    : "Log file not found.";
        }

        private void OpenLog_Click(object sender, RoutedEventArgs e)
        {
            if (selectedHistoryEntry == null)
            {
                MessageBox.Show(
                    "Please select a history record first.",
                    "Copy History",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            string logFilePath = selectedHistoryEntry.LogFilePath;

            if (string.IsNullOrWhiteSpace(logFilePath) ||
                !File.Exists(logFilePath))
            {
                MessageBox.Show(
                    "The log file could not be found.",
                    "Copy History",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open the log file.\n\n{ex.Message}",
                    "Copy History",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HistoryDataGrid_MouseDoubleClick(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (HistoryDataGrid.SelectedItem is not CopyHistoryEntry selectedEntry)
                return;

            var detailsWindow = new CopyHistoryDetailsWindow(selectedEntry)
            {
                Owner = this
            };

            detailsWindow.ShowDialog();
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var history = copyHistoryService.LoadHistory();

            if (history == null || history.Count == 0)
            {
                MessageBox.Show(
                    "There are no history records to export.",
                    "Export History",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Export Copy History",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"RoboCopy_History_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv",
                AddExtension = true
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var csv = new StringBuilder();

                csv.AppendLine(
                    "Start Time,End Time,Source,Destination,Status," +
                    "Total Files,Copied Files,Failed Files," +
                    "Bytes Transferred,Duration (Seconds),Remarks");

                foreach (var entry in history)
                {
                    string[] fields =
                    {
                entry.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                entry.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                entry.Source,
                entry.Destination,
                entry.Status,
                entry.TotalFiles.ToString(),
                entry.CopiedFiles.ToString(),
                entry.FailedFiles.ToString(),
                entry.BytesTransferred.ToString(),
                entry.DurationSeconds.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                entry.Remarks
            };

                    csv.AppendLine(
                        string.Join(",", Array.ConvertAll(fields, EscapeCsv)));
                }

                File.WriteAllText(
                    dialog.FileName,
                    csv.ToString(),
                    new UTF8Encoding(true));

                MessageBox.Show(
                    $"History exported successfully!\n\n{dialog.FileName}",
                    "Export Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to export history.\n\n{ex.Message}",
                    "Export Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static string EscapeCsv(string value)
        {
            value ??= "";

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

    }
}