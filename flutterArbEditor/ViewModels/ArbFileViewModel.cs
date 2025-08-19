using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

using flutterArbEditor.Models;

namespace flutterArbEditor.ViewModels
{
    public class ArbFileViewModel : INotifyPropertyChanged
    {
        private string _languageCode;

        public ArbFile ArbFile { get; }
        public string FileName => Path.GetFileName(ArbFile.FilePath);

        public string LanguageCode
        {
            get => _languageCode;
            set
            {
                if (SetProperty(ref _languageCode, value))
                {
                    ArbFile.LanguageCode = value;
                }
            }
        }

        public ArbFileViewModel(ArbFile arbFile)
        {
            ArbFile = arbFile;
            _languageCode = arbFile.LanguageCode;
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