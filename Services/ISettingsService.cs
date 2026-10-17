using ImageGalleryApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageGalleryApp.Services
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();
        void SaveSettings(AppSettings settings);
        Task<List<string>> LoadFavoritesAsync();
        Task SaveFavoritesAsync(List<string> favoritePaths);
    }
}