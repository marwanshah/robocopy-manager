using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RoboCopyManager
{
    public class CopyHistoryService
    {
        private readonly string logDirectory;
        private readonly string historyFilePath;

        private readonly object fileLock = new object();

        public CopyHistoryService()
        {
            logDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "RoboCopyManager",
                "Logs");

            historyFilePath = Path.Combine(
                logDirectory,
                "history.json");

            Directory.CreateDirectory(logDirectory);
        }

        public string CreateLogFilePath(string id)
        {
            string safeId = string.Concat(
                id.Where(char.IsLetterOrDigit));

            return Path.Combine(
                logDirectory,
                $"{DateTime.Now:yyyyMMdd_HHmmss}_{safeId}.log");
        }

        public void AppendLog(string logFilePath, string message)
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
                return;

            lock (fileLock)
            {
                Directory.CreateDirectory(logDirectory);

                File.AppendAllText(
                    logFilePath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
        }

        public void SaveHistory(CopyHistoryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            lock (fileLock)
            {
                Directory.CreateDirectory(logDirectory);

                List<CopyHistoryEntry> history = LoadHistoryInternal();

                int existingIndex = history.FindIndex(
                    item => item.Id == entry.Id);

                if (existingIndex >= 0)
                {
                    history[existingIndex] = entry;
                }
                else
                {
                    history.Add(entry);
                }

                string json = JsonSerializer.Serialize(
                    history,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                string temporaryFile = historyFilePath + ".tmp";

                File.WriteAllText(temporaryFile, json);

                File.Move(
                    temporaryFile,
                    historyFilePath,
                    overwrite: true);
            }
        }

        public List<CopyHistoryEntry> LoadHistory()
        {
            lock (fileLock)
            {
                return LoadHistoryInternal()
                    .OrderByDescending(entry => entry.StartTime)
                    .ToList();
            }
        }

        private List<CopyHistoryEntry> LoadHistoryInternal()
        {
            if (!File.Exists(historyFilePath))
                return new List<CopyHistoryEntry>();

            string json = File.ReadAllText(historyFilePath);

            if (string.IsNullOrWhiteSpace(json))
                return new List<CopyHistoryEntry>();

            return JsonSerializer.Deserialize<List<CopyHistoryEntry>>(
                json) ?? new List<CopyHistoryEntry>();
        }

        public string GetLogDirectory()
        {
            return logDirectory;
        }
    }
}