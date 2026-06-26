using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace storage;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        DataContext = new MainWindowViewModel();

        // 1. Automatyczny focus na start (pierwsza zakładka)
        this.Opened += (s, e) => ProductInput.Focus();

        // 2. Obsługa Enter w pierwszym polu (kompletacja)
        ProductInput.KeyDown += ProductInput_KeyDown;
        
        // Przechwytujemy Enter za pomocą strategii Tunnel (zanim dotrze do wnętrza NumericUpDown)
        QuantityInput.AddHandler(InputElement.KeyDownEvent, QuantityInput_KeyDown, RoutingStrategies.Tunnel);
        
        // 3. Obsługa Enter w drugim polu (podgląd)
        SearchBoxInputControl.KeyDown += SearchBoxInputControl_KeyDown;

        // 4. Wykrywanie zmiany zakładki
        MainTabControl.SelectionChanged += MainTabControl_SelectionChanged;
    }

    // Nowa, bezwzględna metoda obsługi Enter w polu ilości
    private void QuantityInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Zatrzymujemy dalsze przetwarzanie klawisza przez system Avalonia
            e.Handled = true;

            // Ręcznie wywołujemy komendę zapisu/skanowania z ViewModelu
            if (DataContext is MainWindowViewModel viewModel && viewModel.ProductScannedCommand.CanExecute(null))
            {
                viewModel.ProductScannedCommand.Execute(null);
            }

            // Przenosimy kursor z powrotem do pola kodu produktu
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                ProductInput.Focus();
            });
        }
    }

    private void MainTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MainTabControl.SelectedItem is TabItem selectedTab)
        {
            if (selectedTab.Header?.ToString() == "Podgląd zawartości")
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                { 
                    SearchBoxInputControl.Focus();
                });
            }
            else if (selectedTab.Header?.ToString() == " Kompletacja do kartonu")
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ProductInput.Focus();
                });
            }
        }
    }

    private void ProductInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                ProductInput.Focus();
            });
        }
    }
    
    private void SearchBoxInputControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                SearchBoxInputControl.Focus();
            });
        }
    }
}