using System.Collections.Generic;
using System.Windows;

namespace RoboCopyManager
{
    public partial class ConflictWindow : Window
    {
        private readonly List<CopyFileInfo> conflictFiles;

        public ConflictWindow(List<CopyFileInfo> files)
        {
            InitializeComponent();

            conflictFiles = files;

            foreach (CopyFileInfo file in conflictFiles)
            {
                file.Decision = CopyDecision.Skip;
            }

            ConflictDataGrid.ItemsSource =
                conflictFiles;

            SummaryText.Text =
                $"{conflictFiles.Count:N0} file(s) are different and require your decision.";
        }

        private void SkipAll_Click(
            object sender,
            RoutedEventArgs e)
        {
            foreach (CopyFileInfo file in conflictFiles)
            {
                file.Decision =
                    CopyDecision.Skip;
            }

            ConflictDataGrid.Items.Refresh();
        }

        private void OverwriteAll_Click(
            object sender,
            RoutedEventArgs e)
        {
            foreach (CopyFileInfo file in conflictFiles)
            {
                file.Decision =
                    CopyDecision.Overwrite;
            }

            ConflictDataGrid.Items.Refresh();
        }

        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Continue_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}