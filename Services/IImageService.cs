using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageGalleryApp.Models;

namespace ImageGalleryApp.Services
{
    public class ImageIndexResult
    {
        public List<string> AllImagePaths { get; set; } = new();
        public List<string> NewImagePaths { get; set; } = new();
        public List<string> RemovedImagePaths { get; set; } = new();
    }

    public interface IImageService
    {
        Task<BitmapImage> CreateThumbnailAsync(string imagePath, int size = 200);
        Task<BitmapImage> LoadFullImageAsync(string imagePath);
        Task<ImageIndexResult> GetImageFilesAsync(string folderPath);
        bool IsImageFile(string filePath);
        Task<List<ImageItem>> LoadImagesAsync(List<string> imagePaths, int thumbnailSize);
        void ClearCaches();
    }
}