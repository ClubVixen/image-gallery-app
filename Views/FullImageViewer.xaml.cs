using System.Windows;
using System.Windows.Input;
using ImageGalleryApp.Models;
using System.Windows.Media;

namespace ImageGalleryApp.Views
{
    public partial class FullImageViewer : Window
    {
        private Point? _lastDragPoint;

        public FullImageViewer(ImageItem imageItem)
        {
            InitializeComponent();
            DataContext = imageItem;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            // Add a reset view key 'R'
            if (e.Key == Key.R)
            {
                if (ImageContainer != null && ImageContainer.RenderTransform is MatrixTransform transform)
                {
                    transform.Matrix = Matrix.Identity;
                }
            }
        }

        private void ImageContainer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is UIElement element && element.RenderTransform is MatrixTransform transform)
            {
                var matrix = transform.Matrix;
                var scale = e.Delta > 0 ? 1.15 : (1.0 / 1.15);
                var position = e.GetPosition(element);
                matrix.ScaleAt(scale, scale, position.X, position.Y);
                transform.Matrix = matrix;
            }
        }

        private void ImageContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                _lastDragPoint = e.GetPosition(this); // Position relative to the window
                element.CaptureMouse();
            }
        }

        private void ImageContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.ReleaseMouseCapture();
                _lastDragPoint = null;
            }
        }

        private void ImageContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lastDragPoint.HasValue && e.LeftButton == MouseButtonState.Pressed && sender is UIElement element && element.RenderTransform is MatrixTransform transform)
            {
                var position = e.GetPosition(this);
                var matrix = transform.Matrix;
                var delta = Point.Subtract(position, _lastDragPoint.Value);
                matrix.Translate(delta.X, delta.Y);
                transform.Matrix = matrix;
                _lastDragPoint = position;
            }
        }
    }
}
