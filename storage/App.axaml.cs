using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace storage;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var sharedViewModel = new MainWindowViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Jeśli uruchamiamy na komputerze: tworzymy tradycyjne okno
            desktop.MainWindow = new MainWindow
            {
                DataContext = sharedViewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Jeśli uruchamiamy na Androidzie (skanerze): wstrzykujemy MainView bezpośrednio na pełny ekran
            singleViewPlatform.MainView = new MainView
            {
                DataContext = sharedViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}