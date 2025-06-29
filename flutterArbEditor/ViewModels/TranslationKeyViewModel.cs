using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace flutterArbEditor.ViewModels
{
    public class TranslationKeyViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Key { get; set; } = string.Empty;
        public bool IsMissingInSomeFiles { get; set; }
        public int TotalFiles { get; set; }
        public int FilesWithKey { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string StatusText => IsMissingInSomeFiles
            ? $"{FilesWithKey}/{TotalFiles} files"
            : "Complete";

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