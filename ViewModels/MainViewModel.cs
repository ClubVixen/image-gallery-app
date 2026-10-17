using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageGalleryApp.Models;
using ImageGalleryApp.Services;
using ImageGalleryApp.Views;
using System.Diagnostics;

namespace ImageGalleryApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IImageService _imageService;
        private readonly IFileService _fileService;
        private readonly ISettingsService _settingsService;
        private readonly Random _random = new();

        private string _selectedFolderPath = "";
        private List<ImageItem> _allImagesInFolder = new();
        private List<ImageItem> _allFavoriteImages = new();
        private ObservableCollection<ImageItem> _displayedImages = new();
        private ObservableCollection<ImageItem> _blacklistedImages = new();
        private int _rerollCount = 0;
        private bool _isLoading = false;
        private AppSettings _settings;
        private bool _isFapRouletteMode;
        private bool _isFavoritesViewActive;

        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set => SetProperty(ref _selectedFolderPath, value);
        }

        public ObservableCollection<ImageItem> DisplayedImages
        {
            get => _displayedImages;
            set
            {
                if (SetProperty(ref _displayedImages, value))
                {
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public ObservableCollection<ImageItem> BlacklistedImages
        {
            get => _blacklistedImages;
            set => SetProperty(ref _blacklistedImages, value);
        }

        public int RerollCount
        {
            get => _rerollCount;
            set => SetProperty(ref _rerollCount, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public AppSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public bool IsFapRouletteMode
        {
            get => _isFapRouletteMode;
            set
            {
                if (SetProperty(ref _isFapRouletteMode, value))
                {
                    // If switching modes, exit favorites view and clear the screen to avoid confusion
                    IsFavoritesViewActive = false;
                    DisplayedImages.Clear();
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public bool IsFavoritesViewActive
        {
            get => _isFavoritesViewActive;
            set => SetProperty(ref _isFavoritesViewActive, value);
        }

        public string StatusText
        {
            get
            {
                if (IsFavoritesViewActive)
                {
                    return $"Favorites View | Total: {_allFavoriteImages.Count} | Displayed: {DisplayedImages.Count}";
                }
                if (IsFapRouletteMode)
                {
                    return $"Fap Roulette Mode | Folder Images: {_allImagesInFolder.Count} | Displayed: {DisplayedImages.Count} | Favorites: {_allFavoriteImages.Count}";
                }
                return $"Normal Mode | Folder Images: {_allImagesInFolder.Count} | Displayed: {DisplayedImages.Count} | Kept: {DisplayedImages.Count(x => x.IsKept)} | Rerolls: {RerollCount}/{Settings.MaxRerolls}";
            }
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand LoadRandomImagesCommand { get; }
        public ICommand RerollCommand { get; }
        public ICommand ToggleKeepCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand ToggleBlacklistCommand { get; }
        public ICommand ShowFullImageCommand { get; }
        public ICommand SaveSelectedCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenDonateLinkCommand { get; }
        public ICommand OpenGitHubLinkCommand { get; }
        public ICommand StartEliminationCommand { get; }
        public ICommand ViewFavoritesCommand { get; }
        public ICommand BackToBrowseCommand { get; }

        public MainViewModel(IImageService imageService, IFileService fileService, ISettingsService settingsService)
        {
            _imageService = imageService;
            _fileService = fileService;
            _settingsService = settingsService;
            _settings = _settingsService.LoadSettings();

            SelectFolderCommand = new RelayCommand(SelectFolder);
            LoadRandomImagesCommand = new RelayCommand(LoadRandomImages, () => !string.IsNullOrEmpty(SelectedFolderPath) && !IsLoading);
            RerollCommand = new RelayCommand(Reroll, CanReroll);
            ToggleKeepCommand = new RelayCommand(ToggleKeep);
            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);
            ToggleBlacklistCommand = new RelayCommand(ToggleBlacklist);
            ShowFullImageCommand = new RelayCommand(ShowFullImage);
            SaveSelectedCommand = new RelayCommand(SaveSelected, () => DisplayedImages.Any(x => x.IsKept));
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenDonateLinkCommand = new RelayCommand(OpenDonateLink);
            OpenGitHubLinkCommand = new RelayCommand(OpenGitHubLink);
            StartEliminationCommand = new RelayCommand(StartElimination, () => DisplayedImages.Count(x => x.IsKept) > Settings.TargetFinalImages);
            ViewFavoritesCommand = new RelayCommand(ViewFavorites, () => _allFavoriteImages.Any() && !IsLoading);
            BackToBrowseCommand = new RelayCommand(BackToBrowse, () => !IsLoading);

            if (!string.IsNullOrEmpty(_settings.LastSelectedFolder))
            {
                SelectedFolderPath = _settings.LastSelectedFolder;
            }

            _ = LoadFavorites();
        }

        private async Task LoadFavorites()
        {
            var favoritePaths = await _settingsService.LoadFavoritesAsync();
            var validPaths = favoritePaths.Where(p => System.IO.File.Exists(p)).ToList();
            if (validPaths.Any())
            {
                _allFavoriteImages = validPaths.Select(p => new ImageItem(p) { IsFavorited = true }).ToList();
            }

            if (validPaths.Count != favoritePaths.Count)
            {
                await _settingsService.SaveFavoritesAsync(validPaths);
            }
            OnPropertyChanged(nameof(StatusText));
        }

        private void SelectFolder()
        {
            var folderPath = _fileService.SelectFolder("Select Image Folder");
            if (!string.IsNullOrEmpty(folderPath))
            {
                SelectedFolderPath = folderPath;
                Settings.LastSelectedFolder = folderPath;
                _settingsService.SaveSettings(Settings);
                _imageService.ClearCaches();
                _allImagesInFolder.Clear();
                DisplayedImages.Clear();
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private async void LoadRandomImages()
        {
            if (string.IsNullOrEmpty(SelectedFolderPath)) return;

            IsLoading = true;
            IsFavoritesViewActive = false;
            DisplayedImages.Clear();
            try
            {
                // Only re-index if the folder image list is empty
                if (!_allImagesInFolder.Any())
                {
                    var indexResult = await _imageService.GetImageFilesAsync(SelectedFolderPath);
                    if (!indexResult.AllImagePaths.Any())
                    {
                        MessageBox.Show("No image files found in the selected folder.", "No Images",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        _allImagesInFolder.Clear();
                        return;
                    }
                    _allImagesInFolder = indexResult.AllImagePaths.Select(path => new ImageItem(path)).ToList();

                    var favoritePaths = new HashSet<string>(_allFavoriteImages.Select(f => f.FilePath));
                    foreach (var img in _allImagesInFolder.Where(i => favoritePaths.Contains(i.FilePath)))
                    {
                        img.IsFavorited = true;
                    }
                }

                RerollCount = 0;
                await LoadRandomSelectionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private async Task LoadRandomSelectionAsync()
        {
            List<ImageItem> sourceList;
            int desiredCount;
            List<ImageItem> imagesToKeepOnScreen = new List<ImageItem>();

            if (IsFavoritesViewActive)
            {
                sourceList = _allFavoriteImages;
                desiredCount = Settings.FavoritesImageCount;
            }
            else // Normal or Fap Roulette from folder
            {
                sourceList = _allImagesInFolder;
                desiredCount = IsFapRouletteMode ? Settings.FapRouletteImageCount : Settings.TotalImagesToChoose;
                // Only keep images on screen in normal mode
                if (!IsFapRouletteMode)
                {
                    imagesToKeepOnScreen = DisplayedImages.Where(x => x.IsKept).ToList();
                    desiredCount -= imagesToKeepOnScreen.Count;
                }
            }

            var availableImages = sourceList
                .Where(img => !BlacklistedImages.Any(b => b.FilePath == img.FilePath) && !DisplayedImages.Any(d => d.FilePath == img.FilePath))
                .ToList();

            if (desiredCount <= 0)
            {
                DisplayedImages = new ObservableCollection<ImageItem>(imagesToKeepOnScreen);
                return;
            }

            if (availableImages.Count < desiredCount)
            {
                if (!IsFavoritesViewActive) // Don't show this for favorites, it's expected
                {
                    MessageBox.Show($"Not enough images available to choose from. Need {desiredCount}, but only {availableImages.Count} available (after filtering blacklisted/kept images).",
                   "Insufficient Images", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                desiredCount = availableImages.Count;
            }

            var randomImages = availableImages.OrderBy(_ => _random.Next()).Take(desiredCount).ToList();

            var loadThumbTasks = randomImages.Select(async image =>
            {
                if (image.Thumbnail == null)
                    image.Thumbnail = await _imageService.CreateThumbnailAsync(image.FilePath, Settings.ThumbnailSize);
            });
            await Task.WhenAll(loadThumbTasks);

            var newDisplayed = imagesToKeepOnScreen.Concat(randomImages).ToList();
            DisplayedImages = new ObservableCollection<ImageItem>(newDisplayed);
        }

        private bool CanReroll()
        {
            if (IsLoading) return false;
            if (IsFavoritesViewActive) return _allFavoriteImages.Any();
            if (IsFapRouletteMode) return _allImagesInFolder.Any();
            return RerollCount < Settings.MaxRerolls && _allImagesInFolder.Any();
        }

        private async void Reroll()
        {
            IsLoading = true;
            try
            {
                if (IsFavoritesViewActive)
                {
                    // For Favorites View, just get a new set of favorites.
                    DisplayedImages.Clear();
                    await LoadRandomSelectionAsync();
                }
                else if (IsFapRouletteMode)
                {
                    // For Fap Roulette, get a new set from the folder, no reroll limit.
                    DisplayedImages.Clear();
                    await LoadRandomSelectionAsync();
                }
                else // Normal Mode
                {
                    if (RerollCount >= Settings.MaxRerolls) return;
                    RerollCount++;
                    await LoadRandomSelectionAsync();
                }
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private void ToggleKeep(object? parameter)
        {
            if (parameter is ImageItem image)
            {
                image.IsKept = !image.IsKept;
                if (image.IsKept)
                    image.IsBlacklisted = false;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private async void ToggleFavorite(object? parameter)
        {
            if (parameter is ImageItem image)
            {
                image.IsFavorited = !image.IsFavorited;

                var existingFavorite = _allFavoriteImages.FirstOrDefault(f => f.FilePath == image.FilePath);
                var existingInFolderList = _allImagesInFolder.FirstOrDefault(f => f.FilePath == image.FilePath);

                if (image.IsFavorited)
                {
                    if (existingFavorite == null) _allFavoriteImages.Add(image);
                    if (existingInFolderList != null) existingInFolderList.IsFavorited = true;
                }
                else
                {
                    if (existingFavorite != null) _allFavoriteImages.Remove(existingFavorite);
                    if (existingInFolderList != null) existingInFolderList.IsFavorited = false;

                    if (IsFavoritesViewActive)
                    {
                        DisplayedImages.Remove(image);
                    }
                }
                await _settingsService.SaveFavoritesAsync(_allFavoriteImages.Select(f => f.FilePath).ToList());
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private void ToggleBlacklist(object? parameter)
        {
            if (parameter is ImageItem image)
            {
                if (BlacklistedImages.Count >= Settings.MaxBlacklistedItems && !image.IsBlacklisted)
                {
                    MessageBox.Show($"Maximum blacklisted items ({Settings.MaxBlacklistedItems}) reached.",
                        "Blacklist Full", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                image.IsBlacklisted = !image.IsBlacklisted;
                if (image.IsBlacklisted)
                {
                    image.IsKept = false;
                    image.IsFavorited = false;

                    var favoriteMatch = _allFavoriteImages.FirstOrDefault(f => f.FilePath == image.FilePath);
                    if (favoriteMatch != null) _allFavoriteImages.Remove(favoriteMatch);

                    if (!BlacklistedImages.Contains(image))
                        BlacklistedImages.Add(image);
                }
                else
                {
                    BlacklistedImages.Remove(image);
                }
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private async void ShowFullImage(object? parameter)
        {
            if (parameter is ImageItem image)
            {
                try
                {
                    if (image.FullImage == null)
                        image.FullImage = await _imageService.LoadFullImageAsync(image.FilePath);

                    var viewer = new FullImageViewer(image);
                    viewer.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading full image: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SaveSelected()
        {
            var keptImages = DisplayedImages.Where(x => x.IsKept).ToList();
            if (!keptImages.Any())
            {
                MessageBox.Show("No images selected to save.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"This will move {keptImages.Count} images from the source folder to a new location.\n\n" +
                "⚠️ WARNING: The original files will be REMOVED from the source folder!\n\n" +
                "Do you want to continue?",
                "Confirm File Move",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var saveFolder = _fileService.SelectFolder("Select Save Folder");
            if (string.IsNullOrEmpty(saveFolder))
                return;

            IsLoading = true;
            try
            {
                var imagePaths = keptImages.Select(x => x.FilePath).ToList();
                var success = await _fileService.MoveImagesAsync(imagePaths, saveFolder);

                if (success)
                {
                    MessageBox.Show($"Successfully moved {keptImages.Count} images to {saveFolder}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    foreach (var image in keptImages)
                    {
                        DisplayedImages.Remove(image);
                        _allImagesInFolder.RemoveAll(i => i.FilePath == image.FilePath);
                    }

                    Settings.LastSaveFolder = saveFolder;
                    _settingsService.SaveSettings(Settings);
                }
                else
                {
                    MessageBox.Show("Failed to move some or all images.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(StatusText));
            }
        }


        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow(Settings);
            if (settingsWindow.ShowDialog() == true)
            {
                Settings = settingsWindow.Settings;
                _settingsService.SaveSettings(Settings);
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private void StartElimination()
        {
            var keptImages = DisplayedImages.Where(x => x.IsKept).ToList();
            if (keptImages.Count <= Settings.TargetFinalImages)
                return;

            var eliminationWindow = new EliminationWindow(keptImages, Settings.TargetFinalImages);
            if (eliminationWindow.ShowDialog() == true)
            {
                var winners = eliminationWindow.Winners;

                foreach (var image in DisplayedImages)
                {
                    image.IsKept = winners.Contains(image);
                }

                MessageBox.Show($"Elimination complete! {winners.Count} remain.",
                    "Elimination Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private async void ViewFavorites()
        {
            if (IsLoading || !_allFavoriteImages.Any()) return;
            IsLoading = true;
            try
            {
                IsFavoritesViewActive = true;
                DisplayedImages.Clear();
                await LoadRandomSelectionAsync();
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private async void BackToBrowse()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                IsFavoritesViewActive = false;
                DisplayedImages.Clear();
                await LoadRandomSelectionAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenDonateLink()
        {
            OpenUrl("https://ko-fi.com/moliminous");
        }

        private void OpenGitHubLink()
        {
            OpenUrl("https://github.com/carelesswhisp");
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open the link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}