using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using flutterArbEditor.ViewModels;

namespace flutterArbEditor.Views
{
    public partial class AddFilesDialog : Window
    {
        public AddFilesDialogViewModel ViewModel { get; }

        public AddFilesDialog(IEnumerable<ArbFileViewModel> currentlyLoadedFiles)
        {
            InitializeComponent();
            ViewModel = new AddFilesDialogViewModel(currentlyLoadedFiles);
            DataContext = ViewModel;

            ViewModel.CloseRequested += (sender, result) =>
            {
                DialogResult = result;
                Close();
            };
        }

        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string filePath)
            {
                if (File.Exists(filePath))
                {
                    // Show file in Explorer
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{filePath}\"",
                        UseShellExecute = true
                    });
                }
            }
        }
    }
}