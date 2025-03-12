using System.Text.Json;

namespace ExaltAccountManager.Core.Settings
{
    public class SettingsManager<T> where T : class
    {
        private readonly string _filePath;

        public SettingsManager(string fileName) => _filePath = GetLocalFilePath(fileName);

        public T? LoadSettings() => File.Exists(_filePath) ? JsonSerializer.Deserialize<T>(File.ReadAllText(_filePath)) : null;

        public void SaveSettings(T settings)
        {
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_filePath, json);
        }

        private static string GetLocalFilePath(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string exaltAccountManagerDirectory = $"{appData}\\ExaltAccountManager";

            if (!Directory.Exists(exaltAccountManagerDirectory))
            {
                // create sub directory within appdata folder
                Directory.CreateDirectory(exaltAccountManagerDirectory);
            }
            return Path.Combine(exaltAccountManagerDirectory, fileName);
        }
    }
}
