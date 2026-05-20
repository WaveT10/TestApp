using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TestApp.Services;

namespace TestApp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var collection = new ServiceCollection();
            collection.AddServices();
            var services = collection.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new FunctionsWindow();
                var clipboard = TopLevel.GetTopLevel(mainWindow)?.Clipboard;
                var pendingChangesDialogService = new PendingChangesDialogService(mainWindow);

                mainWindow.DataContext = new FunctionsViewModel(services.GetRequiredService<IModelConverterFactory>(), 
                                                                 clipboard, 
                                                                 pendingChangesDialogService);
                desktop.MainWindow = mainWindow;
            }

            Dispatcher.UnhandledException += (sender, args) =>
            {
                args.Handled = true;
                var errorWindow = new ErrorWindow(args.Exception.Message);
                errorWindow.Show();
            };

            base.OnFrameworkInitializationCompleted();
        }
    }
}