using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SchedulerClientApp.Services;
using SchedulerClientApp.ViewModels;
using SchedulerClientApp.Views;

namespace SchedulerClientApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = new MainViewModel();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            var logService = new LogService(mainViewModel);

        }

        //else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        //{
        //    singleViewPlatform.MainView = new MainView
        //    {
        //        DataContext = new MainViewModel()
        //    };
        //}

        base.OnFrameworkInitializationCompleted();
    }
}
