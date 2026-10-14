using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoboCopyManager
{
    internal class RobocopyService
    {
        public async Task<int> GetTotalFileCountAsync(
            string source,
            bool isFileSource)
        {
            if (isFileSource)
            {
                return source
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(file => file.Trim())
                    .Count(File.Exists);
            }

            return await Task.Run(() =>
                Directory
                    .EnumerateFiles(
                        source,
                        "*",
                        SearchOption.AllDirectories)
                    .Count());
        }

        public async Task<long> GetTotalFileSizeAsync(
            string source,
            bool isFileSource)
        {
            if (isFileSource)
            {
                return await Task.Run(() =>
                    source
                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(file => file.Trim())
                        .Where(File.Exists)
                        .Select(file => new FileInfo(file).Length)
                        .Sum());
            }

            return await Task.Run(() =>
                Directory
                    .EnumerateFiles(
                        source,
                        "*",
                        SearchOption.AllDirectories)
                    .Select(file => new FileInfo(file).Length)
                    .Sum());
        }

        public async Task<CopyAnalysisResult> AnalyzeAsync(
            string source,
            string destination,
            bool isFileSource)
        {
            return await Task.Run(() =>
            {
                var result = new CopyAnalysisResult();

                if (isFileSource)
                {
                    AnalyzeIndividualFiles(
                        source,
                        destination,
                        result);
                }
                else
                {
                    AnalyzeFolder(
                        source,
                        destination,
                        result);
                }

                result.TotalBytesToCopy =
                    result.MissingFiles.Sum(file => file.SourceSize) +
                    result.DifferentFiles.Sum(file => file.SourceSize);

                return result;
            });
        }

        private void AnalyzeFolder(
            string source,
            string destination,
            CopyAnalysisResult result)
        {
            if (!Directory.Exists(source))
            {
                return;
            }

            foreach (string sourceFile in Directory.EnumerateFiles(
                source,
                "*",
                SearchOption.AllDirectories))
            {
                string relativePath =
                    Path.GetRelativePath(source, sourceFile);

                string destinationFile =
                    Path.Combine(destination, relativePath);

                AnalyzeFile(
                    sourceFile,
                    destinationFile,
                    relativePath,
                    result);
            }
        }

        private void AnalyzeIndividualFiles(
            string source,
            string destination,
            CopyAnalysisResult result)
        {
            string[] files = source
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(file => file.Trim())
                .Where(File.Exists)
                .ToArray();

            foreach (string sourceFile in files)
            {
                string fileName =
                    Path.GetFileName(sourceFile);

                string destinationFile =
                    Path.Combine(destination, fileName);

                AnalyzeFile(
                    sourceFile,
                    destinationFile,
                    fileName,
                    result);
            }
        }

        private void AnalyzeFile(
            string sourceFile,
            string destinationFile,
            string relativePath,
            CopyAnalysisResult result)
        {
            var sourceInfo = new FileInfo(sourceFile);

            if (!File.Exists(destinationFile))
            {
                result.MissingFiles.Add(
                    new CopyFileInfo
                    {
                        SourcePath = sourceFile,
                        DestinationPath = destinationFile,
                        RelativePath = relativePath,
                        SourceSize = sourceInfo.Length,
                        DestinationSize = 0,
                        SourceModified = sourceInfo.LastWriteTimeUtc,
                        DestinationModified = DateTime.MinValue,
                        Action = CopyAction.Missing
                    });

                return;
            }

            var destinationInfo =
                new FileInfo(destinationFile);

            bool sameSize =
                sourceInfo.Length ==
                destinationInfo.Length;

            bool sameModified =
                sourceInfo.LastWriteTimeUtc ==
                destinationInfo.LastWriteTimeUtc;

            var fileInfo = new CopyFileInfo
            {
                SourcePath = sourceFile,
                DestinationPath = destinationFile,
                RelativePath = relativePath,
                SourceSize = sourceInfo.Length,
                DestinationSize = destinationInfo.Length,
                SourceModified = sourceInfo.LastWriteTimeUtc,
                DestinationModified = destinationInfo.LastWriteTimeUtc
            };

            if (sameSize && sameModified)
            {
                fileInfo.Action = CopyAction.Same;

                result.SameFiles.Add(fileInfo);
            }
            else
            {
                fileInfo.Action = CopyAction.Different;

                result.DifferentFiles.Add(fileInfo);
            }
        }

        public CopyPlan BuildCopyPlan(
            CopyAnalysisResult analysis)
        {
            var plan = new CopyPlan();

            // Semua file yang belum wujud di destination
            foreach (CopyFileInfo file in analysis.MissingFiles)
            {
                file.Decision = CopyDecision.Overwrite;

                plan.FilesToCopy.Add(file);
            }

            // File yang berbeza hanya akan dicopy
            // jika user pilih Overwrite
            foreach (CopyFileInfo file in analysis.DifferentFiles)
            {
                if (file.Decision == CopyDecision.Overwrite)
                {
                    plan.FilesToCopy.Add(file);
                }
            }

            // Kira jumlah sebenar bytes yang perlu dicopy
            plan.TotalBytes = plan.FilesToCopy
                .Sum(file => file.SourceSize);

            return plan;
        }

        public async Task<int> RunCopyPlanAsync(
            CopyPlan plan,
            Action<string>? outputHandler = null,
            Action<long>? progressHandler = null,
            CancellationToken cancellationToken = default)
        {
            if (plan.FilesToCopy.Count == 0)
            {
                progressHandler?.Invoke(0);
                return 0;
            }

            int highestExitCode = 0;

            var fileGroups = plan.FilesToCopy
                .GroupBy(file => new
                {
                    SourceDirectory =
                        Path.GetDirectoryName(file.SourcePath)
                        ?? string.Empty,

                    DestinationDirectory =
                        Path.GetDirectoryName(file.DestinationPath)
                        ?? string.Empty
                })
                .ToList();

            using var progressCts =
                new CancellationTokenSource();

            Task progressTask = Task.Run(async () =>
            {
                while (!progressCts.Token.IsCancellationRequested)
                {
                    long transferredBytes = 0;

                    foreach (CopyFileInfo file in plan.FilesToCopy)
                    {
                        if (progressCts.Token.IsCancellationRequested)
                            break;

                        try
                        {
                            if (File.Exists(file.DestinationPath))
                            {
                                long destinationSize =
                                    new FileInfo(file.DestinationPath).Length;

                                transferredBytes += Math.Min(
                                    destinationSize,
                                    file.SourceSize);
                            }
                        }
                        catch
                        {
                            // File may be temporarily locked
                            // while RoboCopy is writing it.
                        }
                    }

                    if (!progressCts.Token.IsCancellationRequested)
                    {
                        progressHandler?.Invoke(transferredBytes);
                    }

                    try
                    {
                        await Task.Delay(250, progressCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            });

            try
            {
                foreach (var group in fileGroups)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string sourceDirectory =
                        group.Key.SourceDirectory;

                    string destinationDirectory =
                        group.Key.DestinationDirectory;

                    string fileArguments = string.Join(
                        " ",
                        group.Select(file =>
                            $"\"{Path.GetFileName(file.SourcePath)}\""));

                    int exitCode = await RunRobocopyAsync(
                        sourceDirectory,
                        destinationDirectory,
                        fileArguments,
                        outputHandler,
                        cancellationToken);

                    if (exitCode > highestExitCode)
                    {
                        highestExitCode = exitCode;
                    }
                }
            }
            finally
            {
                // Always stop the progress task, including cancellation.
                progressCts.Cancel();

                try
                {
                    await progressTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when progress tracking is cancelled.
                }
            }

            // Final progress measurement
            long finalTransferredBytes = 0;

            foreach (CopyFileInfo file in plan.FilesToCopy)
            {
                try
                {
                    if (File.Exists(file.DestinationPath))
                    {
                        long destinationSize =
                            new FileInfo(file.DestinationPath).Length;

                        finalTransferredBytes += Math.Min(
                            destinationSize,
                            file.SourceSize);
                    }
                }
                catch
                {
                    // Ignore files that cannot be inspected.
                }
            }

            progressHandler?.Invoke(finalTransferredBytes);

            return highestExitCode;
        }

        private async Task<int> RunRobocopyAsync(
            string source,
            string destination,
            string fileArguments = "",
            Action<string>? outputHandler = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startInfo = new ProcessStartInfo
            {
                FileName = "robocopy.exe",
                Arguments = string.IsNullOrWhiteSpace(fileArguments)
                    ? $"\"{source}\" \"{destination}\" /E /MT:32 /R:1 /W:1"
                    : $"\"{source}\" \"{destination}\" {fileArguments} /MT:32 /R:1 /W:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            using CancellationTokenRegistration registration =
                cancellationToken.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(entireProcessTree: true);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Process has already been disposed.
                    }
                    catch (InvalidOperationException)
                    {
                        // Process has already exited.
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        // Process could not be terminated.
                    }
                });

            Task outputTask = Task.Run(async () =>
                    {
                        while (await process.StandardOutput.ReadLineAsync() is string line)
                        {
                            outputHandler?.Invoke(line);
                        }
                    });

            Task errorTask = Task.Run(async () =>
            {
                while (await process.StandardError.ReadLineAsync() is string line)
                {
                    outputHandler?.Invoke(line);
                }
            });

            await process.WaitForExitAsync();

            await Task.WhenAll(outputTask, errorTask);

            cancellationToken.ThrowIfCancellationRequested();

            return process.ExitCode;
        }

        public CopyCancellationSummary GetCancellationSummary(
        CopyPlan plan,
        TimeSpan duration)
        {
            var summary = new CopyCancellationSummary
            {
                TotalFiles = plan.FilesToCopy.Count,
                Duration = duration
            };

            foreach (CopyFileInfo file in plan.FilesToCopy)
            {
                try
                {
                    if (!File.Exists(file.DestinationPath))
                    {
                        summary.NotCopied++;
                        continue;
                    }

                    long destinationSize =
                        new FileInfo(file.DestinationPath).Length;

                    summary.DataTransferredBytes += Math.Min(
                        destinationSize,
                        file.SourceSize);

                    if (destinationSize == file.SourceSize)
                    {
                        summary.FullyCopied++;
                    }
                    else if (destinationSize > 0)
                    {
                        summary.PartiallyCopied++;
                    }
                    else
                    {
                        summary.NotCopied++;
                    }
                }
                catch
                {
                    summary.NotCopied++;
                }
            }

            return summary;
        }
    }
}
