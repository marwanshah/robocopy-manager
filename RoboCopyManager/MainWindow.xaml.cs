using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoboCopyManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int totalFiles = 0;
        private int completedFiles = 0;
        private string currentFileName = "";

        private long totalBytes = 0;
        private long completedBytes = 0;
        private long lastMeasuredBytes = 0;

        private Stopwatch transferStopwatch = new Stopwatch();
        private DateTime lastSpeedUpdate = DateTime.Now;

        private Grid? selectedRow;
        private readonly RobocopyService robocopyService = new RobocopyService();
        private CancellationTokenSource? copyCancellationTokenSource;
        private bool isCopyRunning = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddJob_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddJobWindow
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                string source = window.SourcePath;
                string destination = window.DestinationPath;
                bool isFileSource = window.IsFileSource;

                AddJobRow(source, destination, isFileSource);
                UpdateJobCount();
            }
        
        }

        private void UpdateJobCount()
        {
            int jobCount =
                JobRowsPanel.Children.Count;

            JobCountText.Text =
                $"{jobCount:N0} job(s)";
        }
        private void AddJobRow(string source, string destination, bool isFileSource)
        {
            EmptyStatePanel.Visibility = Visibility.Collapsed;

            var row = new Grid
            {
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 1)
            };

            row.Tag = isFileSource;

            row.MouseLeftButtonDown += JobRow_Click;

            // Left padding
            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(12)
                });

            // #
            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(45)
                });

            // Source
            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(2, GridUnitType.Star)
                });

            // Destination
            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(2, GridUnitType.Star)
                });

            // Status
            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(130)
                });

            // Last Run
            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(160)
                });

            // Remarks
            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(160)
                });

            // Right padding
            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(12)
                });

            var jobNumber =
                    JobRowsPanel.Children.Count + 1;

            var numberCell =
                CreateCell(
                    jobNumber.ToString(),
                    TextAlignment.Center);

            Grid.SetColumn(numberCell, 1);
            row.Children.Add(numberCell);


            var sourceCell =
                CreateCell(source);

            Grid.SetColumn(sourceCell, 2);
            row.Children.Add(sourceCell);


            var destinationCell =
                CreateCell(destination);

            Grid.SetColumn(destinationCell, 3);
            row.Children.Add(destinationCell);


            var statusCell =
                CreateCell(
                    "Ready",
                    TextAlignment.Center);

            Grid.SetColumn(statusCell, 4);
            row.Children.Add(statusCell);


            var lastRunCell =
                CreateCell(
                    "-",
                    TextAlignment.Center);

            Grid.SetColumn(lastRunCell, 5);
            row.Children.Add(lastRunCell);


            var remarksCell =
                CreateCell(
                    "-",
                    TextAlignment.Center);

            Grid.SetColumn(remarksCell, 6);
            row.Children.Add(remarksCell);


            JobRowsPanel.Children.Add(row);
        }

        private void JobRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid row)
            {
                if (selectedRow != null)
{
                    selectedRow.Background = Brushes.White;
                }

                selectedRow = row;

                selectedRow.Background =
                    new SolidColorBrush(
                        Color.FromRgb(207, 228, 255));
            }
        }

        private void RemoveJob_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRow == null)
            {
                MessageBox.Show(
                    "Please select a job to remove.",
                    "No Job Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            JobRowsPanel.Children.Remove(selectedRow);
            UpdateJobCount();
            for (int i = 0; i < JobRowsPanel.Children.Count; i++)
            {
                if (JobRowsPanel.Children[i] is Grid row)
                {
                    if (row.Children[0] is TextBlock numberCell)
                    {
                        numberCell.Text = (i + 1).ToString();
                    }
                }
            }
            selectedRow = null;

            if (JobRowsPanel.Children.Count == 0)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
            }
        }

        private void EditJob_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRow == null)
            {
                MessageBox.Show(
                    "Please select a job to edit.",
                    "No Job Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            string source = string.Empty;
            string destination = string.Empty;
            bool isFileSource = selectedRow.Tag is bool value && value;

            if (selectedRow.Children.Count > 2)
            {
                if (selectedRow.Children[1] is TextBlock sourceCell)
                {
                    source = sourceCell.Text;
                }

                if (selectedRow.Children[2] is TextBlock destinationCell)
                {
                    destination = destinationCell.Text;
                }
            }

            var window = new AddJobWindow(source, destination, isFileSource)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                if (selectedRow.Children[1] is TextBlock sourceCell)
                {
                    sourceCell.Text = window.SourcePath;
                    sourceCell.ToolTip = window.SourcePath;
                }

                if (selectedRow.Children[2] is TextBlock destinationCell)
                {
                    destinationCell.Text = window.DestinationPath;
                    destinationCell.ToolTip = window.DestinationPath;
                }
            }
        
        }

        private void SetStatusBadge(string status)
        {
            switch (status)
            {
                case "Analyzing":

                    StatusBadge.Background =
                        new SolidColorBrush(
                            Color.FromRgb(255, 247, 214));

                    StatusBadgeDot.Fill =
                        new SolidColorBrush(
                            Color.FromRgb(217, 119, 6));

                    StatusBadgeText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(146, 64, 14));

                    break;


                case "Copying":

                    StatusBadge.Background =
                        new SolidColorBrush(
                            Color.FromRgb(254, 226, 226));

                    StatusBadgeDot.Fill =
                        new SolidColorBrush(
                            Color.FromRgb(220, 38, 38));

                    StatusBadgeText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(185, 28, 28));

                    break;


                case "Completed":

                    StatusBadge.Background =
                        new SolidColorBrush(
                            Color.FromRgb(234, 244, 255));

                    StatusBadgeDot.Fill =
                        new SolidColorBrush(
                            Color.FromRgb(36, 161, 72));

                    StatusBadgeText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(23, 107, 53));

                    break;


                case "Cancelled":

                    StatusBadge.Background =
                        new SolidColorBrush(
                            Color.FromRgb(243, 244, 246));

                    StatusBadgeDot.Fill =
                        new SolidColorBrush(
                            Color.FromRgb(107, 114, 128));

                    StatusBadgeText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(75, 85, 99));

                    break;


                case "Failed":

                    StatusBadge.Background =
                        new SolidColorBrush(
                            Color.FromRgb(254, 226, 226));

                    StatusBadgeDot.Fill =
                        new SolidColorBrush(
                            Color.FromRgb(220, 38, 38));

                    StatusBadgeText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(185, 28, 28));

                    break;


                default:

                    StatusBadge.Background =
                        new SolidColorBrush(
                            Color.FromRgb(234, 244, 255));

                    StatusBadgeDot.Fill =
                        new SolidColorBrush(
                            Color.FromRgb(36, 161, 72));

                    StatusBadgeText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(23, 107, 53));

                    break;
            }

            StatusBadgeText.Text = status;
            BottomStatusText.Text = status;

            switch (status)
            {
                case "Analyzing":
                    BottomStatusText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(217, 119, 6));
                    break;

                case "Copying":
                    BottomStatusText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(220, 38, 38));
                    break;

                case "Completed":
                    BottomStatusText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(22, 128, 60));
                    break;

                case "Cancelled":
                    BottomStatusText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(107, 114, 128));
                    break;

                case "Failed":
                    BottomStatusText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(220, 38, 38));
                    break;

                default:
                    BottomStatusText.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(22, 128, 60));
                    break;
            }
        }

        private void StopJob_Click(object sender, RoutedEventArgs e)
        {
            if (!isCopyRunning || copyCancellationTokenSource == null)
            {
                MessageBox.Show(
                    "There is no active copy job to stop.",
                    "RoboCopy Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            copyCancellationTokenSource.Cancel();

            CurrentFileText.Text = "Current: Stopping...";
            BottomStatusText.Text = "Stopping...";
        }

        private async void StartJob_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (selectedRow == null)
            {
                MessageBox.Show(
                    "Please select a job to start.",
                    "No Job Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            string source = string.Empty;
            string destination = string.Empty;

            bool isFileSource =
                selectedRow.Tag is bool value && value;

            if (selectedRow.Children[1] is TextBlock sourceCell)
            {
                source = sourceCell.Text;
            }

            if (selectedRow.Children[2] is TextBlock destinationCell)
            {
                destination = destinationCell.Text;
            }

            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(destination))
            {
                MessageBox.Show(
                    "Source or destination is empty.",
                    "Invalid Job",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (selectedRow.Children[3] is TextBlock statusCell)
            {
                statusCell.Text = "Analyzing";
            }

            SetStatusBadge("Analyzing");

            OverallProgressBar.Value = 0;
            OverallProgressText.Text =
                "Overall Progress: 0%";

            CurrentFileText.Text =
                "Current: Analyzing files...";

            SpeedText.Text =
                "Speed: 0 MB/s";

            EtaText.Text =
                "ETA: --:--:--";

            CopyPlan? plan = null;

            try
            {
                // =====================================================
                // STEP 1
                // Analyze source and destination
                // =====================================================

                SetStatusBadge("Analyzing");

                CopyAnalysisResult analysis =
                    await robocopyService.AnalyzeAsync(
                        source,
                        destination,
                        isFileSource);

                CopyResult copyResult = new CopyResult
                {
                    FilesAnalyzed = analysis.TotalFiles,

                    NewFilesCopied =
                        analysis.MissingFiles.Count,

                    FilesOverwritten = 0,

                    FilesSkipped =
                        analysis.SameFiles.Count,

                    FailedFiles = 0,

                    BytesCopied = 0,

                    Duration = TimeSpan.Zero,

                    AverageSpeedMBPerSecond = 0,

                    WasCancelled = false,

                    IsSuccessful = false
                };

                // =====================================================
                // STEP 2
                // Check destination and missing files
                // =====================================================

                bool destinationIsEmpty =
                    !System.IO.Directory.Exists(destination) ||
                    !System.IO.Directory.EnumerateFileSystemEntries(destination).Any();


                // =====================================================
                // Destination is empty
                // =====================================================

                if (destinationIsEmpty &&
                    analysis.MissingFiles.Count > 0)
                {
                    var emptyDestinationWindow =
                        new EmptyDestinationWindow(
                            analysis.MissingFiles.Count)
                        {
                            Owner = this
                        };

                    bool? emptyDestinationResult =
                        emptyDestinationWindow.ShowDialog();

                    if (emptyDestinationResult != true)
                    {
                        if (selectedRow.Children[3]
                            is TextBlock cancelStatusCell)
                        {
                            cancelStatusCell.Text = "Cancelled";
                        }

                        CurrentFileText.Text =
                            "Current: -";

                        copyResult.WasCancelled = true;
                        SetStatusBadge("Cancelled");

                        return;
                    }
                }


                // =====================================================
                // Destination has content + missing files
                // =====================================================

                if (!destinationIsEmpty)
                {
                    var reviewWindow =
                        new CopyReviewWindow(analysis)
                        {
                            Owner = this
                        };

                    bool? reviewResult =
                        reviewWindow.ShowDialog();

                    if (reviewResult != true)
                    {
                        if (selectedRow.Children[3]
                            is TextBlock cancelStatusCell)
                        {
                            cancelStatusCell.Text = "Cancelled";
                        }

                        CurrentFileText.Text =
                            "Current: -";

                        copyResult.WasCancelled = true;
                        SetStatusBadge("Cancelled");

                        return;
                    }
                }

                // =====================================================
                // STEP 3
                // Handle conflicting files
                // =====================================================

                if (analysis.DifferentFiles.Count > 0)
                {
                    var conflictWindow =
                        new ConflictWindow(
                            analysis.DifferentFiles)
                        {
                            Owner = this
                        };

                    bool? conflictResult =
                        conflictWindow.ShowDialog();

                    if (conflictResult != true)
                    {
                        if (selectedRow.Children[3]
                            is TextBlock cancelStatusCell)
                        {
                            cancelStatusCell.Text = "Cancelled";
                        }

                        CurrentFileText.Text =
                            "Current: -";

                        copyResult.WasCancelled = true;
                        SetStatusBadge("Cancelled");

                        return;
                    }
                }

                copyResult.FilesOverwritten =
                    analysis.DifferentFiles.Count(
                        file => file.Decision ==
                                CopyDecision.Overwrite);

                copyResult.FilesSkipped +=
                    analysis.DifferentFiles.Count(
                        file => file.Decision ==
                                CopyDecision.Skip);

                // =====================================================
                // STEP 4
                // Build final copy plan
                // =====================================================

                plan = robocopyService.BuildCopyPlan(
                    analysis);

                totalFiles = plan.TotalFiles;
                totalBytes = plan.TotalBytes;

                completedFiles = 0;
                completedBytes = 0;

                currentFileName = string.Empty;

                lastMeasuredBytes = 0;

                transferStopwatch.Restart();
                lastSpeedUpdate = DateTime.Now;

                // =====================================================
                // STEP 5
                // Nothing needs to be copied
                // =====================================================

                if (plan.TotalFiles == 0)
                {
                    OverallProgressBar.Value = 100;

                    OverallProgressText.Text =
                        "Overall Progress: 100%";

                    CurrentFileText.Text =
                        "Current: Nothing to copy";

                    EtaText.Text =
                        "ETA: 00:00:00";

                    SpeedText.Text =
                        "Speed: 0 MB/s";

                    if (selectedRow.Children[3]
                        is TextBlock noCopyStatusCell)
                    {
                        noCopyStatusCell.Text = "Completed";
                    }

                    SetStatusBadge("Completed");

                    if (selectedRow.Children[4]
                        is TextBlock noCopyLastRunCell)
                    {
                        noCopyLastRunCell.Text =
                            DateTime.Now.ToString(
                                "dd/MM/yyyy hh:mm tt");
                    }

                    if (selectedRow.Children[5]
                        is TextBlock noCopyRemarksCell)
                    {
                        noCopyRemarksCell.Text =
                            "No changes";
                    }

                    return;
                }

                // =====================================================
                // STEP 6
                // Start actual copy
                // =====================================================

                if (selectedRow.Children[3]
                    is TextBlock runningStatusCell)
                {
                    runningStatusCell.Text = "Running";
                }

                SetStatusBadge("Copying");

                copyCancellationTokenSource?.Dispose();
                copyCancellationTokenSource = new CancellationTokenSource();
                isCopyRunning = true;

                CurrentFileText.Text =
                    $"Current: 0 / {plan.TotalFiles}";

                int exitCode =
                    await robocopyService.RunCopyPlanAsync(
                        plan,

                        line =>
                        {
                            System.Diagnostics.Debug.WriteLine(line);

                            var fileMatch =
                                System.Text.RegularExpressions.Regex.Match(
                                    line,
                                    @"^\s*(New File|Newer|Older|Modified)\s+\d+\s+(.+)$");

                            if (fileMatch.Success)
                            {
                                string filePath =
                                    fileMatch.Groups[2]
                                        .Value
                                        .Trim();

                                currentFileName =
                                    System.IO.Path.GetFileName(
                                    filePath);

                                Dispatcher.Invoke(() =>
                                {
                                    CurrentFileText.Text =
                                        $"Current: {currentFileName}";
                                });

                                return;
                            }

                            var percentageMatch =
                                System.Text.RegularExpressions.Regex.Match(
                                    line,
                                    @"^\s*(\d+)%");

                            if (percentageMatch.Success &&
                                int.TryParse(
                                    percentageMatch.Groups[1].Value,
                                    out int percentage))
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    if (!string.IsNullOrWhiteSpace(
                                        currentFileName))
                                    {
                                        CurrentFileText.Text =
                                            $"Current: {currentFileName} — {percentage}%";
                                    }
                                });
                            }
                        },

                        transferredBytes =>
                        {
                            completedBytes = transferredBytes;

                            if (totalBytes <= 0)
                            {
                                return;
                            }

                            int overallPercentage =
                                (int)Math.Min(
                                    100,
                                    transferredBytes * 100 / totalBytes);

                            UpdateTransferStats(
                                transferredBytes);

                            Dispatcher.Invoke(() =>
                            {
                                OverallProgressBar.Value =
                                    overallPercentage;

                                OverallProgressText.Text =
                                    $"Overall Progress: {overallPercentage}%";
                            });
                        },
                        copyCancellationTokenSource.Token);

                transferStopwatch.Stop();

                copyResult.Duration =
                    transferStopwatch.Elapsed;

                copyResult.BytesCopied =
                    completedBytes;

                copyResult.AverageSpeedMBPerSecond =
                    copyResult.Duration.TotalSeconds > 0
                        ? copyResult.BytesCopied /
                          (1024.0 * 1024.0) /
                          copyResult.Duration.TotalSeconds
                        : 0;

                // =====================================================
                // STEP 7
                // Final status
                // =====================================================
                if (exitCode <= 7)
                {
                    copyResult.IsSuccessful = true;
                    copyResult.FailedFiles = 0;
                }
                else
                {
                    copyResult.IsSuccessful = false;
                    copyResult.FailedFiles = 1;
                }

                if (exitCode <= 7)
                {
                    OverallProgressBar.Value = 100;

                    OverallProgressText.Text =
                        "Overall Progress: 100%";

                    EtaText.Text =
                        "ETA: 00:00:00";

                    if (selectedRow.Children[3]
                        is TextBlock completedStatusCell)
                    {
                        completedStatusCell.Text =
                            "Completed";
                    }

                    SetStatusBadge("Completed");
                }
                else
                {
                    if (selectedRow.Children[3]
                        is TextBlock failedStatusCell)
                    {
                        failedStatusCell.Text =
                            "Failed";
                    }

                    SetStatusBadge("Failed");
                }

                if (selectedRow.Children[4]
                    is TextBlock lastRunCell)
                {
                    lastRunCell.Text =
                        DateTime.Now.ToString(
                            "dd/MM/yyyy hh:mm tt");
                }

                if (selectedRow.Children[5]
                    is TextBlock remarksCell)
                {
                    remarksCell.Text =
                        exitCode switch
                        {
                            0 => "No changes",
                            1 => "Files copied",
                            2 => "Extra files detected",
                            3 => "Files copied, extra files detected",
                            4 => "Mismatched files detected",
                            5 => "Files copied with mismatches",
                            6 => "Extra files and mismatches detected",
                            7 => "Files copied with extras and mismatches",
                            _ => "Copy failed"
                        };
                }

                CurrentFileText.Text =
                    exitCode <= 7
                    ? "Current: Completed"
                    : "Current: Copy failed";

                // =====================================================
                // STEP 8
                // Show copy summary
                // =====================================================
                var summaryWindow =
                    new CopySummaryWindow(copyResult)
                    {
                        Owner = this
                    };

                summaryWindow.ShowDialog();
            }

            catch (OperationCanceledException)
            {
                transferStopwatch.Stop();

                if (selectedRow?.Children[3] is TextBlock cancelledStatusCell)
                {
                    cancelledStatusCell.Text = "Cancelled";
                }

                SetStatusBadge("Cancelled");

                CurrentFileText.Text = "Current: Cancelled";
                EtaText.Text = "ETA: --:--:--";

                if (plan != null)
                {
                    CopyCancellationSummary summary =
                        robocopyService.GetCancellationSummary(
                            plan,
                            transferStopwatch.Elapsed);

                    double transferredMB =
                        summary.DataTransferredBytes / (1024.0 * 1024.0);

                    string transferredData = transferredMB >= 1024
                        ? $"{transferredMB / 1024.0:F2} GB"
                        : $"{transferredMB:F2} MB";

                    var summaryWindow =
                        new CopyCancellationSummaryWindow(summary)
                        {
                            Owner = this
                        };

                    summaryWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                transferStopwatch.Stop();

                if (selectedRow?.Children[3] is TextBlock errorStatusCell)
                {
                    errorStatusCell.Text = "Failed";
                }

                SetStatusBadge("Failed");

                CurrentFileText.Text = "Current: Error";

                MessageBox.Show(
                    $"An error occurred while processing the job:\n\n{ex.Message}",
                    "RoboCopy Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                isCopyRunning = false;

                copyCancellationTokenSource?.Dispose();
                copyCancellationTokenSource = null;
            }
        }

            private string FindSourceFilePath(
            string source,
            string fileName,
            bool isFileSource)
        {
            if (isFileSource)
            {
                string[] files = source
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(file => file.Trim())
                    .ToArray();

                string? matchingFile = files.FirstOrDefault(
                    file => string.Equals(
                        System.IO.Path.GetFileName(file),
                        fileName,
                        StringComparison.OrdinalIgnoreCase));

                return matchingFile ?? string.Empty;
            }

            try
            {
                string? file = Directory
                    .EnumerateFiles(
                        source,
                        fileName,
                        SearchOption.AllDirectories)
                    .FirstOrDefault();

                return file ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void UpdateTransferStats(long currentTransferredBytes)
        {
            if (!transferStopwatch.IsRunning)
            {
                return;
            }

            DateTime now = DateTime.Now;

            double elapsedSeconds =
                (now - lastSpeedUpdate).TotalSeconds;

            if (elapsedSeconds < 0.5)
            {
                return;
            }

            long bytesTransferred =
                currentTransferredBytes - lastMeasuredBytes;

            double speedBytesPerSecond =
                bytesTransferred / elapsedSeconds;

            lastMeasuredBytes = currentTransferredBytes;
            lastSpeedUpdate = now;

            if (speedBytesPerSecond <= 0)
            {
                return;
            }

            double speedMBPerSecond =
                speedBytesPerSecond / (1024 * 1024);

            long remainingBytes =
                Math.Max(
                    0,
                    totalBytes - currentTransferredBytes);

            double etaSeconds =
                remainingBytes / speedBytesPerSecond;

            TimeSpan eta =
                TimeSpan.FromSeconds(etaSeconds);

            Dispatcher.Invoke(() =>
            {
                SpeedText.Text =
                    $"Speed: {speedMBPerSecond:F1} MB/s";

                EtaText.Text =
                    $"ETA: {eta:hh\\:mm\\:ss}";
            });
        }
        private TextBlock CreateCell(
            string text,
            TextAlignment textAlignment = TextAlignment.Left)
        {
            return new TextBlock
            {
                Text = text,
                ToolTip = text,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                TextAlignment = textAlignment,
                Margin = new Thickness(0, 0, 8, 0)
            };
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var window = new AboutWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }
    }

}