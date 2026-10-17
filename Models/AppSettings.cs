namespace ImageGalleryApp.Models
{
    public class AppSettings
    {
        public int MaxRerolls { get; set; } = 5;
        public int MaxBlacklistedItems { get; set; } = 50;
        public int TotalImagesToChoose { get; set; } = 10;
        public int FapRouletteImageCount { get; set; } = 20;
        public int FavoritesImageCount { get; set; } = 20;
        public int TargetFinalImages { get; set; } = 3;
        public int ThumbnailSize { get; set; } = 200;
        public string LastSelectedFolder { get; set; } = "";
        public string LastSaveFolder { get; set; } = "";
    }
}