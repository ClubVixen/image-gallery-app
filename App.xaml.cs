using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ImageGalleryApp.Services;
using ImageGalleryApp.ViewModels;
using ImageGalleryApp.Views;
using System;
using System.Text;

namespace ImageGalleryApp
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                _serviceProvider = serviceCollection.BuildServiceProvider();

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine("An unhandled error occurred during application startup.");
                sb.AppendLine("This is often caused by missing dependencies or configuration errors.");
                sb.AppendLine("\nError Details:");
                sb.AppendLine($"Message: {ex.Message}");

                var innerEx = ex.InnerException;
                int count = 0;
                while (innerEx != null && count < 5)
                {
                    sb.AppendLine($"\nInner Exception ({count + 1}):");
                    sb.AppendLine($"Message: {innerEx.Message}");
                    innerEx = innerEx.InnerException;
                    count++;
                }

                sb.AppendLine($"\nStack Trace:\n{ex.StackTrace}");

                MessageBox.Show(sb.ToString(), "Fatal Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Ensure the application shuts down
                if (Current != null)
                {
                    Current.Shutdown(-1);
                }
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IImageService, ImageService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}