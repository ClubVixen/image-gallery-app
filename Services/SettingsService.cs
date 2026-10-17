using System.IO;
using ImageGalleryApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageGalleryApp.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private readonly string _favoritesPath;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "ImageGalleryApp");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");
            _favoritesPath = Path.Combine(appFolder, "favorites.json");
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }

            return new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch { }
        }

        public async Task<List<string>> LoadFavoritesAsync()
        {
            try
            {
                if (File.Exists(_favoritesPath))
                {
                    var json = await File.ReadAllTextAsync(_favoritesPath);
                    return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                }
            }
            catch { }

            return new List<string>();
        }

        public async Task SaveFavoritesAsync(List<string> favoritePaths)
        {
            try
            {
                var json = JsonConvert.SerializeObject(favoritePaths, Newtonsoft.Json.Formatting.Indented);
                await File.WriteAllTextAsync(_favoritesPath, json);
            }
            catch { }
        }
    }
}