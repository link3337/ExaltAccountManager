using System.Text.Json;

namespace ExaltAccountManager.Core.Settings
{
    public class SettingsManager<T> where T : class
    {
        private readonly string filePath;

        public SettingsManager(string fileName) => filePath = GetLocalFilePath(fileName);

        public T? LoadSettings() => File.Exists(filePath) ? JsonSerializer.Deserialize<T>(File.ReadAllText(filePath)) : null;

        public void SaveSettings(T settings)
        {
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(filePath, json);
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
