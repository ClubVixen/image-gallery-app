using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageGalleryApp.Services
{
    public interface IFileService
    {
        Task<bool> MoveImagesAsync(List<string> sourcePaths, string destinationFolder);
        Task<bool> CopyImagesAsync(List<string> sourcePaths, string destinationFolder);
        string? SelectFolder(string title = "Select Folder");
        bool CreateDirectoryIfNotExists(string path);
    }
}
