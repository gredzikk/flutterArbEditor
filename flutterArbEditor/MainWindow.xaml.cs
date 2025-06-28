using System.Reflection;
using System.Windows;
using flutterArbEditor.ViewModels;
using flutterArbEditor.Models;
using System.IO;

namespace flutterArbEditor
{
    public partial class MainWindow : Window
    {
        private readonly ILogger _logger;
        private readonly MainViewModel _viewModel;
        public string Version { get; }

        public MainWindow(ILogger logger)
        {
            InitializeComponent();
            _logger = logger;
            _logger.LogInfo("MainWindow initialized.");
            Version = GetVersionString();

            _viewModel = new MainViewModel(_logger);
            DataContext = _viewModel;
        }

        public static string GetVersionString()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".arb")
                    {
                        try
                        {
                            var arbFile = ArbFile.LoadFromFile(file);
                            _viewModel.ArbFiles.Add(new ArbFileViewModel(arbFile));
                            _logger.LogInfo($"Loaded ARB file via drag & drop: {file}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to load ARB file via drag & drop: {file}", ex);
                        }
                    }
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}