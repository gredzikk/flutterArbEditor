using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

using flutterArbEditor.Models;

using Newtonsoft.Json.Linq;

namespace flutterArbEditor.ViewModels
{
    public class TranslationPairViewModel : INotifyPropertyChanged
    {
        private readonly ArbFile _arbFile;
        private readonly string _key;
        private string _translation;
        private string _placeholderJson;

        public string LanguageCode => _arbFile.LanguageCode;
        public string FileName => Path.GetFileName(_arbFile.FilePath);

        public string Translation
        {
            get => _translation;
            set
            {
                if (SetProperty(ref _translation, value))
                {
                    _arbFile.Translations[_key] = value;
                }
            }
        }

        public string PlaceholderJson
        {
            get => _placeholderJson;
            set
            {
                if (SetProperty(ref _placeholderJson, value))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            _arbFile.Placeholders.Remove(_key);
                        }
                        else
                        {
                            var placeholder = JObject.Parse(value);
                            _arbFile.Placeholders[_key] = placeholder;
                        }
                    }
                    catch
                    {
                        // Invalid JSON, ignore for now
                    }
                }
            }
        }

        public TranslationPairViewModel(ArbFile arbFile, string key, string translation, JObject? placeholder)
        {
            _arbFile = arbFile;
            _key = key;
            _translation = translation;
            _placeholderJson = placeholder?.ToString() ?? string.Empty;
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