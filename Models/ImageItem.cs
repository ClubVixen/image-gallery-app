using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace ImageGalleryApp.Models
{
    public class ImageItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isBlacklisted;
        private bool _isKept;
        private bool _isFavorited;
        private BitmapImage? _thumbnail;
        private BitmapImage? _fullImage;

        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsBlacklisted
        {
            get => _isBlacklisted;
            set => SetProperty(ref _isBlacklisted, value);
        }

        public bool IsKept
        {
            get => _isKept;
            set => SetProperty(ref _isKept, value);
        }

        public bool IsFavorited
        {
            get => _isFavorited;
            set => SetProperty(ref _isFavorited, value);
        }

        public BitmapImage? Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public BitmapImage? FullImage
        {
            get => _fullImage;
            set => SetProperty(ref _fullImage, value);
        }

        public ImageItem(string filePath)
        {
            FilePath = filePath;
            var fileInfo = new FileInfo(filePath);
            FileSize = fileInfo.Length;
            CreatedDate = fileInfo.CreationTime;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}