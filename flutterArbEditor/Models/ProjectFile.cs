using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace flutterArbEditor.Models
{
    public class ProjectFile
    {
        public string Version { get; set; } = "1.0";
        public string ProjectName { get; set; } = string.Empty;
        public string FlutterProjectPath { get; set; } = string.Empty;
        public bool SortKeysOnSave { get; set; } = false;
        public List<string> ArbFilePaths { get; set; } = new();
        public DateTime LastModified { get; set; } = DateTime.Now;

        public static ProjectFile LoadFromFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var project = JsonConvert.DeserializeObject<ProjectFile>(content) ?? new ProjectFile();
            return project;
        }

        public void SaveToFile(string filePath)
        {
            LastModified = DateTime.Now;
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static ProjectFile CreateFromCurrentState(
            string projectName,
            string flutterProjectPath,
            bool sortKeysOnSave,
            IEnumerable<string> arbFilePaths)
        {
            return new ProjectFile
            {
                ProjectName = projectName,
                FlutterProjectPath = flutterProjectPath,
                SortKeysOnSave = sortKeysOnSave,
                ArbFilePaths = [.. arbFilePaths],
                LastModified = DateTime.Now
            };
        }
    }
}