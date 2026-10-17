using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageGalleryApp.Models;
using System;
using Newtonsoft.Json;

namespace ImageGalleryApp.Services
{
    public class ImageService : IImageService
    {
        private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
        private readonly Dictionary<string, BitmapImage> _thumbnailCache = new();
        private readonly Dictionary<string, BitmapImage> _fullImageCache = new();

        private Dictionary<string, List<string>> _imageIndex = new();
        private readonly string _indexFilePath;

        public ImageService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "ImageGalleryApp");
            Directory.CreateDirectory(appFolder);
            _indexFilePath = Path.Combine(appFolder, "image_index.json");
            LoadIndex();
        }

        private void LoadIndex()
        {
            try
            {
                if (File.Exists(_indexFilePath))
                {
                    var json = File.ReadAllText(_indexFilePath);
                    _imageIndex = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json) ?? new Dictionary<string, List<string>>();
                }
            }
            catch
            {
                _imageIndex = new Dictionary<string, List<string>>();
            }
        }

        private async Task SaveIndexAsync()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_imageIndex, Formatting.Indented);
                await File.WriteAllTextAsync(_indexFilePath, json);
            }
            catch { /* best effort save */ }
        }

        public async Task<BitmapImage> CreateThumbnailAsync(string imagePath, int size = 200)
        {
            var cacheKey = $"{imagePath}_{size}";
            if (_thumbnailCache.TryGetValue(cacheKey, out var cached))
                return cached;

            return await Task.Run(() =>
            {
                try
                {
                    BitmapImage thumbnail = new BitmapImage();
                    using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            thumbnail.BeginInit();
                            thumbnail.CacheOption = BitmapCacheOption.OnLoad;

                            // This can fail for corrupted images or unsupported metadata formats.
                            try
                            {
                                var frame = BitmapFrame.Create(memoryStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                                if (frame.PixelWidth > frame.PixelHeight)
                                {
                                    thumbnail.DecodePixelWidth = size;
                                }
                                else
                                {
                                    thumbnail.DecodePixelHeight = size;
                                }
                            }
                            catch (NotSupportedException)
                            {
                                // The image format might be corrupt or unsupported by WPF's decoder.
                                // Don't set DecodePixelWidth/Height and let it try to load normally.
                            }

                            memoryStream.Position = 0; // Reset stream after potential frame read
                            thumbnail.StreamSource = memoryStream;
                            thumbnail.EndInit();
                        }
                    }
                    thumbnail.Freeze(); 

                    _thumbnailCache[cacheKey] = thumbnail;
                    return thumbnail;
                }
                catch
                {
                    return CreateErrorThumbnail(size);
                }
            });
        }

        public async Task<BitmapImage> LoadFullImageAsync(string imagePath)
        {
            if (_fullImageCache.TryGetValue(imagePath, out var cached))
                return cached;

            return await Task.Run(() =>
            {
                try
                {
                    BitmapImage bitmapImage;
                    using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();
                        }
                    }
                    bitmapImage.Freeze();

                    _fullImageCache[imagePath] = bitmapImage;
                    return bitmapImage;
                }
                catch
                {
                    return CreateErrorThumbnail(200);
                }
            });
        }

        public Task<ImageIndexResult> GetImageFilesAsync(string folderPath)
        {
            return Task.Run(async () =>
            {
                var result = new ImageIndexResult();
                if (!Directory.Exists(folderPath))
                    return result;

                var currentFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(IsImageFile)
                    .ToList();

                _imageIndex.TryGetValue(folderPath, out var indexedFiles);
                indexedFiles ??= new List<string>();

                var newFiles = currentFiles.Except(indexedFiles).ToList();
                var removedFiles = indexedFiles.Except(currentFiles).ToList();

                result.AllImagePaths = currentFiles;
                result.NewImagePaths = newFiles;
                result.RemovedImagePaths = removedFiles;

                if (newFiles.Any() || removedFiles.Any())
                {
                    _imageIndex[folderPath] = currentFiles;
                    await SaveIndexAsync();
                }

                return result;
            });
        }

        public bool IsImageFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return _supportedExtensions.Contains(extension);
        }

        public async Task<List<ImageItem>> LoadImagesAsync(List<string> imagePaths, int thumbnailSize)
        {
            var tasks = imagePaths.Select(async path =>
            {
                var imageItem = new ImageItem(path);
                imageItem.Thumbnail = await CreateThumbnailAsync(path, thumbnailSize);
                return imageItem;
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        public void ClearCaches()
        {
            _thumbnailCache.Clear();
            _fullImageCache.Clear();
        }

        private BitmapImage CreateErrorThumbnail(int size)
        {
            try
            {
                var writeableBitmap = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgr32, null);

                int stride = writeableBitmap.BackBufferStride;
                byte[] pixelData = new byte[size * stride];
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    pixelData[i] = 128; // Blue
                    pixelData[i + 1] = 128; // Green
                    pixelData[i + 2] = 128; // Red
                    pixelData[i + 3] = 255; // Alpha
                }
                writeableBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, size, size), pixelData, stride, 0);

                writeableBitmap.Freeze();

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));

                using var stream = new MemoryStream();
                encoder.Save(stream);
                stream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
            catch
            {
                return new BitmapImage();
            }
        }
    }
}