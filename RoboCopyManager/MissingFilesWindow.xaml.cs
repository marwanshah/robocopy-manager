using System.Windows;

namespace RoboCopyManager
{
    public partial class MissingFilesWindow : Window
    {
        public MissingFilesWindow(int missingFileCount)
        {
            InitializeComponent();

            SummaryText.Text =
                $"{missingFileCount} file(s) are missing from the destination.";
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