using System.Windows;
using ImageGalleryApp.ViewModels;

namespace ImageGalleryApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}