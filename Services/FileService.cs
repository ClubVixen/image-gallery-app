using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageGalleryApp.Services
{
    public class FileService : IFileService
    {
        public async Task<bool> MoveImagesAsync(List<string> sourcePaths, string destinationFolder)
        {
            try
            {
                CreateDirectoryIfNotExists(destinationFolder);

                await Task.Run(() =>
                {
                    foreach (var sourcePath in sourcePaths)
                    {
                        var fileName = Path.GetFileName(sourcePath);
                        var destinationPath = Path.Combine(destinationFolder, fileName);

                        // Handle duplicate names
                        var counter = 1;
                        while (File.Exists(destinationPath))
                        {
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            var extension = Path.GetExtension(fileName);
                            var newFileName = $"{nameWithoutExt}_{counter}{extension}";
                            destinationPath = Path.Combine(destinationFolder, newFileName);
                            counter++;
                        }

                        File.Move(sourcePath, destinationPath);
                    }
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CopyImagesAsync(List<string> sourcePaths, string destinationFolder)
        {
            try
            {
                CreateDirectoryIfNotExists(destinationFolder);

                await Task.Run(() =>
                {
                    foreach (var sourcePath in sourcePaths)
                    {
                        var fileName = Path.GetFileName(sourcePath);
                        var destinationPath = Path.Combine(destinationFolder, fileName);

                        var counter = 1;
                        while (File.Exists(destinationPath))
                        {
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            var extension = Path.GetExtension(fileName);
                            var newFileName = $"{nameWithoutExt}_{counter}{extension}";
                            destinationPath = Path.Combine(destinationFolder, newFileName);
                            counter++;
                        }

                        File.Copy(sourcePath, destinationPath);
                    }
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string? SelectFolder(string title = "Select Folder")
        {
            var dialog = new OpenFolderDialog
            {
                Title = title,
                Multiselect = false
            };

            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        public bool CreateDirectoryIfNotExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
