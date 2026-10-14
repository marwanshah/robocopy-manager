using System.Windows;

namespace RoboCopyManager
{
    public partial class EmptyDestinationWindow : Window
    {
        public EmptyDestinationWindow(int fileCount)
        {
            InitializeComponent();

            SummaryText.Text =
                $"{fileCount:N0} file(s) are ready to be copied to this destination.";
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