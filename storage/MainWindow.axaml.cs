using Avalonia.Controls;
using Avalonia.Input;

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
        
        // 3. Obsługa Enter w drugim polu (podgląd)
        SearchBoxInputControl.KeyDown += SearchBoxInputControl_KeyDown;

        // 4. Wykrywanie zmiany zakładki
        MainTabControl.SelectionChanged += MainTabControl_SelectionChanged;
    }

    private void MainTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Sprawdzamy, która zakładka (TabItem) jest aktualnie wybrana
        if (MainTabControl.SelectedItem is TabItem selectedTab)
        {
            // Jeśli wybraliśmy zakładkę z podglądem zawartości
            if (selectedTab.Header?.ToString() == "Podgląd zawartości")
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                { 
                    SearchBoxInputControl.Focus();
                });
            }
            // Jeśli wróciliśmy do kompletacji
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
        // Po zeskanowaniu kodu kartonu w podglądzie, również odświeżamy focus
        if (e.Key == Key.Enter)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                SearchBoxInputControl.Focus();
            });
        }
    }
}