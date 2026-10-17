using System.Windows;
using ImageGalleryApp.Models;

namespace ImageGalleryApp.Views
{
    public partial class SettingsWindow : Window
    {
        public AppSettings Settings { get; private set; }

        public SettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            Settings = new AppSettings
            {
                MaxRerolls = settings.MaxRerolls,
                MaxBlacklistedItems = settings.MaxBlacklistedItems,
                TotalImagesToChoose = settings.TotalImagesToChoose,
                TargetFinalImages = settings.TargetFinalImages,
                ThumbnailSize = settings.ThumbnailSize,
                FapRouletteImageCount = settings.FapRouletteImageCount,
                FavoritesImageCount = settings.FavoritesImageCount,
                LastSelectedFolder = settings.LastSelectedFolder,
                LastSaveFolder = settings.LastSaveFolder
            };

            LoadSettings();
        }

        private void LoadSettings()
        {
            MaxRerollsTextBox.Text = Settings.MaxRerolls.ToString();
            MaxBlacklistedTextBox.Text = Settings.MaxBlacklistedItems.ToString();
            TotalImagesTextBox.Text = Settings.TotalImagesToChoose.ToString();
            TargetFinalTextBox.Text = Settings.TargetFinalImages.ToString();
            ThumbnailSizeTextBox.Text = Settings.ThumbnailSize.ToString();
            FapRouletteImageCountTextBox.Text = Settings.FapRouletteImageCount.ToString();
            FavoritesImageCountTextBox.Text = Settings.FavoritesImageCount.ToString();
        }

        private bool ValidateAndSaveSettings()
        {
            try
            {
                Settings.MaxRerolls = int.Parse(MaxRerollsTextBox.Text);
                Settings.MaxBlacklistedItems = int.Parse(MaxBlacklistedTextBox.Text);
                Settings.TotalImagesToChoose = int.Parse(TotalImagesTextBox.Text);
                Settings.TargetFinalImages = int.Parse(TargetFinalTextBox.Text);
                Settings.ThumbnailSize = int.Parse(ThumbnailSizeTextBox.Text);
                Settings.FapRouletteImageCount = int.Parse(FapRouletteImageCountTextBox.Text);
                Settings.FavoritesImageCount = int.Parse(FavoritesImageCountTextBox.Text);

                if (Settings.MaxRerolls < 0 || Settings.MaxBlacklistedItems < 0 ||
                    Settings.TotalImagesToChoose < 1 || Settings.TargetFinalImages < 1 ||
                    Settings.ThumbnailSize < 50 || Settings.ThumbnailSize > 500 ||
                    Settings.FapRouletteImageCount < 1 || Settings.FavoritesImageCount < 1)
                {
                    MessageBox.Show("Please enter valid positive numbers within reasonable ranges.",
                        "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (Settings.TargetFinalImages >= Settings.TotalImagesToChoose)
                {
                    MessageBox.Show("Target final images must be less than total images to choose.",
                        "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }
            catch
            {
                MessageBox.Show("Please enter valid numbers for all fields.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateAndSaveSettings())
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}