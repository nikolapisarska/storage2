using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace storage;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        ProductInput.KeyDown += ProductInput_KeyDown;
        QuantityInput.AddHandler(InputElement.KeyDownEvent, QuantityInput_KeyDown, RoutingStrategies.Tunnel);
        SearchBoxInputControl.KeyDown += SearchBoxInputControl_KeyDown;
        MainTabControl.SelectionChanged += MainTabControl_SelectionChanged;
    }

    private void SaveBoxButton_Click(object? sender, RoutedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => { ProductInput.Focus(); });
    }

    private void QuantityInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            if (DataContext is MainWindowViewModel viewModel && viewModel.ProductScannedCommand.CanExecute(null))
            {
                viewModel.ProductScannedCommand.Execute(null);
            }
            Avalonia.Threading.Dispatcher.UIThread.Post(() => { ProductInput.Focus(); });
        }
    }

    private void MainTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MainTabControl.SelectedItem is TabItem selectedTab)
        {
            if (selectedTab.Header?.ToString() == "Podgląd zawartości")
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => { SearchBoxInputControl.Focus(); });
            }
            else if (selectedTab.Header?.ToString() == " Kompletacja do kartonu")
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => { ProductInput.Focus(); });
            }
        }
    }

    private void ProductInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => { ProductInput.Focus(); });
        }
    }
    
    private void SearchBoxInputControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => { SearchBoxInputControl.Focus(); });
        }
    }
}