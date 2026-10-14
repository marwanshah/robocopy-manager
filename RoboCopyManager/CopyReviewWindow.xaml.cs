using System.Windows;

namespace RoboCopyManager
{
    public partial class CopyReviewWindow : Window
    {
        public CopyReviewWindow(
            CopyAnalysisResult analysis)
        {
            InitializeComponent();

            NewFilesText.Text =
                analysis.MissingFiles.Count.ToString("N0");

            SameFilesText.Text =
                analysis.SameFiles.Count.ToString("N0");

            DifferentFilesText.Text =
                analysis.DifferentFiles.Count.ToString("N0");

            TotalFilesText.Text =
                analysis.TotalFiles.ToString("N0");

            if (analysis.DifferentFiles.Count > 0)
            {
                DecisionMessageText.Text =
                    $"{analysis.DifferentFiles.Count:N0} existing file(s) " +
                    "are different and will require your decision " +
                    "in the next step.";
            }
            else
            {
                DecisionMessageText.Text =
                    "No existing files require an overwrite decision.";
            }
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