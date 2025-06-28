using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using flutterArbEditor.Models;
using flutterArbEditor.Commands;

namespace flutterArbEditor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ILogger _logger;
        private string _selectedKey = string.Empty;
        private string _flutterProjectPath = string.Empty;
        private ArbFileViewModel? _selectedFile;
        private string _newKeyName = string.Empty;
        private bool _sortKeysOnSave = false;

        public ObservableCollection<ArbFileViewModel> ArbFiles { get; } = new();
        public ObservableCollection<string> TranslationKeys { get; } = new();
        public ObservableCollection<TranslationPairViewModel> CurrentTranslations { get; } = new();

        public ICommand AddFilesCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand SaveAllCommand { get; }
        public ICommand RunFlutterGenCommand { get; }
        public ICommand SelectFlutterProjectCommand { get; }
        public ICommand AddNewKeyCommand { get; }
        public ICommand RemoveKeyCommand { get; }
        public ICommand SortKeysCommand { get; }

        public string SelectedKey
        {
            get => _selectedKey;
            set
            {
                if (SetProperty(ref _selectedKey, value))
                {
                    LoadTranslationsForKey(value);
                }
            }
        }

        public string FlutterProjectPath
        {
            get => _flutterProjectPath;
            set => SetProperty(ref _flutterProjectPath, value);
        }

        public ArbFileViewModel? SelectedFile
        {
            get => _selectedFile;
            set => SetProperty(ref _selectedFile, value);
        }

        public string NewKeyName
        {
            get => _newKeyName;
            set => SetProperty(ref _newKeyName, value);
        }

        public bool SortKeysOnSave
        {
            get => _sortKeysOnSave;
            set => SetProperty(ref _sortKeysOnSave, value);
        }

        public MainViewModel(ILogger logger)
        {
            _logger = logger;
            AddFilesCommand = new RelayCommand(AddFiles);
            RemoveFileCommand = new RelayCommand<ArbFileViewModel>(RemoveFile);
            SaveAllCommand = new RelayCommand(SaveAll);
            RunFlutterGenCommand = new RelayCommand(RunFlutterGen);
            SelectFlutterProjectCommand = new RelayCommand(SelectFlutterProject);
            AddNewKeyCommand = new RelayCommand(AddNewKey, CanAddNewKey);
            RemoveKeyCommand = new RelayCommand(RemoveKey, CanRemoveKey);
            SortKeysCommand = new RelayCommand(SortKeys);
        }

        private void AddFiles()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ARB files (*.arb)|*.arb|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    try
                    {
                        var arbFile = ArbFile.LoadFromFile(fileName);
                        ArbFiles.Add(new ArbFileViewModel(arbFile));
                        _logger.LogInfo($"Loaded ARB file: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to load ARB file: {fileName}", ex);
                    }
                }
                RefreshTranslationKeys();
            }
        }

        private void RemoveFile(ArbFileViewModel? fileViewModel)
        {
            if (fileViewModel != null)
            {
                ArbFiles.Remove(fileViewModel);
                RefreshTranslationKeys();
            }
        }

        private void SaveAll()
        {
            foreach (var arbFileViewModel in ArbFiles)
            {
                try
                {
                    arbFileViewModel.ArbFile.SaveToFile(SortKeysOnSave);
                    _logger.LogInfo($"Saved ARB file: {arbFileViewModel.ArbFile.FilePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to save ARB file: {arbFileViewModel.ArbFile.FilePath}", ex);
                }
            }
        }

        private void RunFlutterGen()
        {
            if (string.IsNullOrEmpty(FlutterProjectPath))
            {
                _logger.LogWarning("Flutter project path not set");
                return;
            }

            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "flutter",
                        Arguments = "gen-l10n",
                        WorkingDirectory = FlutterProjectPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInfo("Flutter gen-l10n completed successfully");
                    _logger.LogInfo(output);
                }
                else
                {
                    _logger.LogError($"Flutter gen-l10n failed with exit code {process.ExitCode}");
                    _logger.LogError(error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run flutter gen-l10n", ex);
            }
        }

        private void SelectFlutterProject()
        {
            var folderDialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Flutter Project Directory",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (folderDialog.ShowDialog() == true)
            {
                FlutterProjectPath = folderDialog.FolderName;
                _logger.LogInfo($"Flutter project path set to: {FlutterProjectPath}");
            }
        }

        private void AddNewKey()
        {
            if (string.IsNullOrWhiteSpace(NewKeyName))
                return;

            string keyName = NewKeyName.Trim();

            // Check if key already exists
            if (ArbFiles.Any(f => f.ArbFile.HasTranslationKey(keyName)))
            {
                _logger.LogWarning($"Translation key '{keyName}' already exists");
                return;
            }

            // Add key to all ARB files
            foreach (var arbFile in ArbFiles)
            {
                arbFile.ArbFile.AddTranslationKey(keyName, string.Empty);
            }

            RefreshTranslationKeys();
            SelectedKey = keyName;
            NewKeyName = string.Empty;
            _logger.LogInfo($"Added new translation key: {keyName}");
        }

        private bool CanAddNewKey()
        {
            return !string.IsNullOrWhiteSpace(NewKeyName) && ArbFiles.Count > 0;
        }

        private void RemoveKey()
        {
            if (string.IsNullOrEmpty(SelectedKey))
                return;

            // Remove key from all ARB files
            foreach (var arbFile in ArbFiles)
            {
                arbFile.ArbFile.RemoveTranslationKey(SelectedKey);
            }

            string removedKey = SelectedKey;
            RefreshTranslationKeys();
            SelectedKey = string.Empty;
            _logger.LogInfo($"Removed translation key: {removedKey}");
        }

        private bool CanRemoveKey()
        {
            return !string.IsNullOrEmpty(SelectedKey);
        }

        private void SortKeys()
        {
            RefreshTranslationKeys();
            _logger.LogInfo("Translation keys sorted alphabetically");
        }

        private void RefreshTranslationKeys()
        {
            TranslationKeys.Clear();
            var allKeys = ArbFiles.SelectMany(f => f.ArbFile.Translations.Keys).Distinct().OrderBy(k => k);
            foreach (var key in allKeys)
            {
                TranslationKeys.Add(key);
            }
        }

        private void LoadTranslationsForKey(string key)
        {
            CurrentTranslations.Clear();
            foreach (var arbFile in ArbFiles)
            {
                var translation = arbFile.ArbFile.Translations.TryGetValue(key, out var value) ? value : string.Empty;
                var placeholder = arbFile.ArbFile.Placeholders.TryGetValue(key, out var ph) ? ph : null;
                CurrentTranslations.Add(new TranslationPairViewModel(arbFile.ArbFile, key, translation, placeholder));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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