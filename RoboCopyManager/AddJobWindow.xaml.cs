using System.Windows;

namespace RoboCopyManager
{
    public partial class AddJobWindow : Window
    {
        public string SourcePath { get; private set; } = string.Empty;
        public string DestinationPath { get; private set; } = string.Empty;
        public bool IsFileSource { get; private set; }

        public AddJobWindow(
            string sourcePath = "",
            string destinationPath = "",
            bool isFileSource = false)
        {
            InitializeComponent();

            SourceTextBox.Text = sourcePath;
            DestinationTextBox.Text = destinationPath;

            FolderRadioButton.IsChecked = !isFileSource;
            FilesRadioButton.IsChecked = isFileSource;
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            if (FilesRadioButton.IsChecked == true)
            {
                var dialog = new System.Windows.Forms.OpenFileDialog
                {
                    Title = "Select Source File(s)",
                    Multiselect = true,
                    CheckFileExists = true,
                    CheckPathExists = true
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SourceTextBox.Text = string.Join("; ", dialog.FileNames);
                }
            }
            else
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select Source Folder",
                    UseDescriptionForTitle = true
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SourceTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Destination Folder",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DestinationTextBox.Text = dialog.SelectedPath;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AddJob_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourceTextBox.Text))
            {
                MessageBox.Show(
                    "Please select a source folder.",
                    "Source Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(DestinationTextBox.Text))
            {
                MessageBox.Show(
                    "Please select a destination folder.",
                    "Destination Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            SourcePath = SourceTextBox.Text;
            DestinationPath = DestinationTextBox.Text;
            IsFileSource = FilesRadioButton.IsChecked == true;

            DialogResult = true;
        }
    }
}