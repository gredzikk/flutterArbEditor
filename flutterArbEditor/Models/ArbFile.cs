using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Formatting = Newtonsoft.Json.Formatting;

namespace flutterArbEditor.Models
{
    public class ArbFile
    {
        public string FilePath { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public Dictionary<string, string> Translations { get; set; } = new();
        public Dictionary<string, JObject> Placeholders { get; set; } = new();
        public JObject? Metadata { get; set; }

        public static ArbFile LoadFromFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var json = JObject.Parse(content);
            var arbFile = new ArbFile { FilePath = filePath };

            foreach (var property in json.Properties())
            {
                if (property.Name == "@@locale")
                {
                    arbFile.LanguageCode = property.Value?.ToString() ?? string.Empty;
                }
                else if (property.Name.StartsWith("@"))
                {
                    // This is metadata for a translation key
                    var keyName = property.Name.Substring(1);
                    if (property.Value is JObject metadata)
                    {
                        arbFile.Placeholders[keyName] = metadata;
                    }
                }             
                else if (property.Name.StartsWith("@@"))
                {
                    // Other metadata
                    if (arbFile.Metadata == null)
                        arbFile.Metadata = new JObject();
                    arbFile.Metadata[property.Name] = property.Value;
                }
                else
                {
                    // This is a translation
                    arbFile.Translations[property.Name] = property.Value?.ToString() ?? string.Empty;
                }
            }

            return arbFile;
        }

        public void SaveToFile(bool sortKeys = false)
        {
            var json = new JObject();

            // Add metadata first
            if (Metadata != null)
            {
                foreach (var metadata in Metadata.Properties())
                {
                    json[metadata.Name] = metadata.Value;
                }
            }

            if (!string.IsNullOrEmpty(LanguageCode))
            {
                json["@@locale"] = LanguageCode;
            }

            // Add translations (sorted or original order)
            IEnumerable<KeyValuePair<string, string>> translationsToAdd = sortKeys
                ? Translations.OrderBy(t => t.Key)
                : Translations;

            foreach (var translation in translationsToAdd)
            {
                json[translation.Key] = translation.Value;
            }

            // Add placeholders (sorted or original order)
            IEnumerable<KeyValuePair<string, JObject>> placeholdersToAdd = sortKeys
                ? Placeholders.OrderBy(p => p.Key)
                : Placeholders;

            foreach (var placeholder in placeholdersToAdd)
            {
                json[$"@{placeholder.Key}"] = placeholder.Value;
            }

            File.WriteAllText(FilePath, json.ToString(Formatting.Indented));
        }

        public void AddTranslationKey(string key, string defaultValue = "")
        {
            if (!Translations.ContainsKey(key))
            {
                Translations[key] = defaultValue;
            }
        }

        public void RemoveTranslationKey(string key)
        {
            Translations.Remove(key);
            Placeholders.Remove(key);
        }

        public bool HasTranslationKey(string key)
        {
            return Translations.ContainsKey(key);
        }

        public void EnsureTranslationKey(string key, string defaultValue = "")
        {
            if (!Translations.ContainsKey(key))
            {
                Translations[key] = defaultValue;
            }
        }
    }
}