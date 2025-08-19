using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using flutterArbEditor.Commands;
using flutterArbEditor.Models;
using Microsoft.Win32;

namespace flutterArbEditor.ViewModels
{
    public class FilePreviewViewModel : INotifyPropertyChanged
    {
        private bool _isCurrentlyLoaded;

        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
        public string LanguageCode { get; set; } = string.Empty;
        public int KeyCount { get; set; }
        public ArbFile? ArbFile { get; set; }

        public bool IsCurrentlyLoaded
        {
            get => _isCurrentlyLoaded;
            set => SetProperty(ref _isCurrentlyLoaded, value);
        }

        public string StatusText => IsCurrentlyLoaded ? "Currently loaded" : "New file";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(StatusText));
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AddFilesDialogViewModel : INotifyPropertyChanged
    {
        private string _projectName = "Untitled Project";
        private readonly IEnumerable<ArbFileViewModel> _currentlyLoadedFiles;

        public ObservableCollection<FilePreviewViewModel> PreviewFiles { get; } = new();

        public ICommand BrowseFilesCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand LoadFilesCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand LoadProjectCommand { get; }
        public ICommand NewProjectCommand { get; }

        public bool HasNoFiles => PreviewFiles.Count == 0;
        public string UniqueLanguages => string.Join(", ", PreviewFiles.Select(f => f.LanguageCode).Distinct().OrderBy(l => l));
        public int CurrentlyLoadedCount => PreviewFiles.Count(f => f.IsCurrentlyLoaded);
        public int NewFilesCount => PreviewFiles.Count(f => !f.IsCurrentlyLoaded);

        public string ProjectName
        {
            get => _projectName;
            set
            {
                SetProperty(ref _projectName, value);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<bool>? CloseRequested;
        public event EventHandler<ProjectFile>? ProjectLoaded;

        public AddFilesDialogViewModel(IEnumerable<ArbFileViewModel> currentlyLoadedFiles)
        {
            _currentlyLoadedFiles = currentlyLoadedFiles;

            BrowseFilesCommand = new RelayCommand(BrowseFiles);
            ClearAllCommand = new RelayCommand(ClearAll);
            RemoveFileCommand = new RelayCommand<FilePreviewViewModel>(RemoveFile);
            LoadFilesCommand = new RelayCommand(LoadFiles, CanLoadFiles);
            CancelCommand = new RelayCommand(Cancel);
            SaveProjectCommand = new RelayCommand(SaveProject);
            LoadProjectCommand = new RelayCommand(LoadProject);
            NewProjectCommand = new RelayCommand(NewProject);

            PreviewFiles.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasNoFiles));
                OnPropertyChanged(nameof(UniqueLanguages));
                OnPropertyChanged(nameof(CurrentlyLoadedCount));
                OnPropertyChanged(nameof(NewFilesCount));
            };

            LoadCurrentFiles();
        }

        private void LoadCurrentFiles()
        {
            foreach (var arbFileViewModel in _currentlyLoadedFiles)
            {
                var preview = new FilePreviewViewModel
                {
                    FilePath = arbFileViewModel.ArbFile.FilePath,
                    LanguageCode = arbFileViewModel.LanguageCode,
                    KeyCount = arbFileViewModel.ArbFile.Translations.Count,
                    ArbFile = arbFileViewModel.ArbFile,
                    IsCurrentlyLoaded = true
                };
                PreviewFiles.Add(preview);
            }
        }

        private void BrowseFiles()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "ARB files (*.arb)|*.arb|All files (*.*)|*.*",
                Multiselect = true,
                Title = "Select ARB Files"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    if (PreviewFiles.Any(f => f.FilePath.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    try
                    {
                        var arbFile = ArbFile.LoadFromFile(fileName);
                        var preview = new FilePreviewViewModel
                        {
                            FilePath = fileName,
                            LanguageCode = string.IsNullOrEmpty(arbFile.LanguageCode) ? "Unknown" : arbFile.LanguageCode,
                            KeyCount = arbFile.Translations.Count,
                            ArbFile = arbFile,
                            IsCurrentlyLoaded = false
                        };
                        PreviewFiles.Add(preview);
                    }
                    catch
                    {
                        // Skip files that can't be loaded
                    }
                }
            }
        }

        private void ClearAll()
        {
            PreviewFiles.Clear();
        }

        private void RemoveFile(FilePreviewViewModel? file)
        {
            if (file != null)
            {
                PreviewFiles.Remove(file);
            }
        }

        private void LoadFiles()
        {
            CloseRequested?.Invoke(this, true);
        }

        private bool CanLoadFiles()
        {
            return PreviewFiles.Count > 0;
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, false);
        }

        private void SaveProject()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "ARB Editor Project (*.aep)|*.aep|All files (*.*)|*.*",
                DefaultExt = "aep",
                Title = "Save Project As",
                FileName = $"{ProjectName}.aep"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var projectFile = ProjectFile.CreateFromCurrentState(
                        ProjectName,
                        string.Empty, 
                        false, 
                        PreviewFiles.Select(f => f.FilePath)
                    );

                    projectFile.SaveToFile(saveFileDialog.FileName);

                    if (string.IsNullOrEmpty(ProjectName) || ProjectName == "Untitled Project")
                    {
                        ProjectName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to save project: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void LoadProject()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "ARB Editor Project (*.aep)|*.aep|All files (*.*)|*.*",
                Title = "Load Project"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var projectFile = ProjectFile.LoadFromFile(openFileDialog.FileName);
                    ProjectLoaded?.Invoke(this, projectFile);

                    PreviewFiles.Clear();

                    foreach (var filePath in projectFile.ArbFilePaths)
                    {
                        if (File.Exists(filePath))
                        {
                            try
                            {
                                var arbFile = ArbFile.LoadFromFile(filePath);
                                var preview = new FilePreviewViewModel
                                {
                                    FilePath = filePath,
                                    LanguageCode = string.IsNullOrEmpty(arbFile.LanguageCode) ? "Unknown" : arbFile.LanguageCode,
                                    KeyCount = arbFile.Translations.Count,
                                    ArbFile = arbFile,
                                    IsCurrentlyLoaded = false
                                };
                                PreviewFiles.Add(preview);
                            }
                            catch
                            {
                                // Skip files that can't be loaded
                            }
                        }
                    }

                    ProjectName = projectFile.ProjectName;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to load project: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void NewProject()
        {
            var result = System.Windows.MessageBox.Show(
                "This will clear all currently loaded files. Are you sure?",
                "New Project",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                PreviewFiles.Clear();
                ProjectName = "Untitled Project";
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}