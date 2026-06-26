using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using storage.Model;
using storage.Services;

namespace storage;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly StorageService _storageService;

    // --- Sekcja Skanowania ---
    private string _scanInput = string.Empty;
    
    // ZMIANA: Zmiana typu na int?, aby bezpiecznie obsługiwać puste wartości z UI
    private int? _scanQuantity = 1; 

    // --- Zakładka 1: Kompletacja do kartonu ---
    private Box _currentPackingBox = new Box();
    private ObservableCollection<BoxItem> _currentBoxItems = new ObservableCollection<BoxItem>();

    // --- Zakładka 2: Podgląd zawartości ---
    private string _searchBoxInput = string.Empty;
    private Box? _scannedResultBox;

    // --- Pasek statusu (na dole okna) ---
    private string _statusMessage = "Gotowy do pracy. Zeskanuj produkt, aby rozpocząć.";

    public MainWindowViewModel()
    {
        _storageService = new StorageService();
        
        ProductScannedCommand = new RelayCommand(OnProductScanned);
        SaveBoxCommand = new RelayCommand(OnSaveBox);
        BoxScannedCommand = new RelayCommand(OnBoxScanned);
        EditScannedBoxCommand = new RelayCommand(OnEditScannedBox);
        
        RemoveProductCommand = new RelayCommand<BoxItem>(OnRemoveProduct);
        
        _currentPackingBox.Items = new List<BoxItem>();
    }

    // Właściwości dla Skanowania i Ilości 
    public string ScanInput
    {
        get => _scanInput;
        set { _scanInput = value; OnPropertyChanged(); }
    }

    // ZMIANA: Właściwość akceptuje teraz wartości null oraz zabezpiecza przed wpisaniem wartości < 1
    public int? ScanQuantity
    {
        get => _scanQuantity;
        set 
        { 
            if (value.HasValue && value.Value < 1)
            {
                _scanQuantity = 1;
            }
            else
            {
                _scanQuantity = value; 
            }
            OnPropertyChanged(); 
        }
    }

    // Właściwości dla Zakładki 1 
    public Box CurrentPackingBox
    {
        get => _currentPackingBox;
        set { _currentPackingBox = value; OnPropertyChanged(); }
    }

    public ObservableCollection<BoxItem> CurrentBoxItems
    {
        get => _currentBoxItems;
        set { _currentBoxItems = value; OnPropertyChanged(); }
    }

    // Właściwości pośrednie dla wymiarów/wagi 
    public decimal? PackingWidth
    {
        get => CurrentPackingBox.Width == 0 ? null : (decimal)CurrentPackingBox.Width;
        set 
        { 
            CurrentPackingBox.Width = value.HasValue ? (float)value.Value : 0f;
            OnPropertyChanged(); 
        }
    }

    public decimal? PackingHeight
    {
        get => CurrentPackingBox.Height == 0 ? null : (decimal)CurrentPackingBox.Height;
        set 
        { 
            CurrentPackingBox.Height = value.HasValue ? (float)value.Value : 0f;
            OnPropertyChanged(); 
        }
    }

    public decimal? PackingLength
    {
        get => CurrentPackingBox.Length == 0 ? null : (decimal)CurrentPackingBox.Length;
        set 
        { 
            CurrentPackingBox.Length = value.HasValue ? (float)value.Value : 0f;
            OnPropertyChanged(); 
        }
    }

    public decimal? PackingWeight
    {
        get => CurrentPackingBox.Weight == 0 ? null : (decimal)CurrentPackingBox.Weight;
        set 
        { 
            CurrentPackingBox.Weight = value.HasValue ? (float)value.Value : 0f;
            OnPropertyChanged(); 
        }
    }
    // Właściwości dla Zakładki 2 
    public string SearchBoxInput
    {
        get => _searchBoxInput;
        set { _searchBoxInput = value; OnPropertyChanged(); }
    }

    public Box? ScannedResultBox
    {
        get => _scannedResultBox;
        set { _scannedResultBox = value; OnPropertyChanged(); }
    }

    // Wspólny Pasek Statusu 
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    // Deklaracje komend (ICommand) 
    public ICommand ProductScannedCommand { get; }
    public ICommand SaveBoxCommand { get; }
    public ICommand BoxScannedCommand { get; }
    public ICommand RemoveProductCommand { get; }
    public ICommand EditScannedBoxCommand { get; }

    // LOGIKA BIZNESOWA

    private void OnProductScanned()
    {
        if (string.IsNullOrWhiteSpace(ScanInput)) return;

        string code = ScanInput.Trim();
        var product = _storageService.FindProduct(code);

        // ZMIANA: Zabezpieczenie na wypadek, gdyby pole Ilość zostało puste (null) podczas skanowania
        int finalQuantity = ScanQuantity ?? 1;

        if (product != null)
        {
            var existingItem = CurrentBoxItems.FirstOrDefault(i => i.Product.CodeOrIdGraffiti == code);

            if (existingItem != null)
            {
                existingItem.Quantity += finalQuantity;
            }
            else
            {
                    CurrentBoxItems.Add(new BoxItem { Product = product, Quantity = finalQuantity });
            }

            StatusMessage = $"Dodano: {product.Name} (sztuk: {finalQuantity})";
        }
        else
        {
            StatusMessage = $"Nie znaleziono produktu o kodzie: {code}";
        }

        ScanQuantity = 1; 
        ScanInput = string.Empty;
    }

    private void OnRemoveProduct(BoxItem? item)
    {
        if (item != null && CurrentBoxItems.Contains(item))
        {
            CurrentBoxItems.Remove(item);
            StatusMessage = $"Usunięto z kartonu: {item.Product.Name}";
        }
    }

    private void OnEditScannedBox()
    {
        if (ScannedResultBox == null) return;

        CurrentPackingBox = new Box
        {
            BoxCode = ScannedResultBox.BoxCode,
            Width = ScannedResultBox.Width,
            Height = ScannedResultBox.Height,
            Length = ScannedResultBox.Length,
            Weight = ScannedResultBox.Weight
        };

        CurrentBoxItems.Clear();
        foreach (var item in ScannedResultBox.Items)
        {
            CurrentBoxItems.Add(new BoxItem 
            { 
                Product = item.Product, 
                Quantity = item.Quantity 
            });
        }

        StatusMessage = $"Wczytano karton {CurrentPackingBox.BoxCode} do edycji.";

        OnPropertyChanged(nameof(CurrentPackingBox));
        OnPropertyChanged(nameof(CurrentBoxItems));
        OnPropertyChanged(nameof(PackingWidth));
        OnPropertyChanged(nameof(PackingHeight));
        OnPropertyChanged(nameof(PackingLength));
        OnPropertyChanged(nameof(PackingWeight));

        SelectedTabIndex = 0;
    }

    private void OnSaveBox()
    {
        if (!_currentBoxItems.Any())
        {
            StatusMessage = "Nie można zapisać pustego kartonu!";
            return;
        }

        CurrentPackingBox.Items = _currentBoxItems.ToList();
        
        if (string.IsNullOrEmpty(CurrentPackingBox.BoxCode))
        {
            _storageService.SaveBox(CurrentPackingBox);
            StatusMessage = $"Pomyślnie zapisano karton: {CurrentPackingBox.BoxCode}";
        }
        else
        {
            _storageService.UpdateBox(CurrentPackingBox);
            StatusMessage = $"Pomyślnie zaktualizowano dane kartonu: {CurrentPackingBox.BoxCode}";
        }

        CurrentPackingBox = new Box { Items = new List<BoxItem>() };
        _currentBoxItems.Clear();
        
        OnPropertyChanged(nameof(CurrentPackingBox));
        OnPropertyChanged(nameof(CurrentBoxItems));
        OnPropertyChanged(nameof(PackingBoxCode));
        OnPropertyChanged(nameof(PackingWidth));
        OnPropertyChanged(nameof(PackingHeight));
        OnPropertyChanged(nameof(PackingLength));
        OnPropertyChanged(nameof(PackingWeight));
    }

    private void OnBoxScanned()
    {
        if (string.IsNullOrWhiteSpace(SearchBoxInput)) return;

        string boxCode = SearchBoxInput.Trim();
        var box = _storageService.FindBox(boxCode);

        if (box != null)
        {
            ScannedResultBox = box;
            StatusMessage = $"Wyświetlam zawartość kartonu {box.BoxCode}.";
        }
        else
        {
            ScannedResultBox = null;
            StatusMessage = $"Nie odnaleziono w bazie kartonu o kodzie: '{boxCode}'";
        }

        SearchBoxInput = string.Empty; 
    }
    private int _selectedTabIndex = 0;

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set 
        { 
            _selectedTabIndex = value; 
            OnPropertyChanged(); 
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public string PackingBoxCode
    {
        get => CurrentPackingBox.BoxCode;
        set 
        { 
            CurrentPackingBox.BoxCode = value ?? string.Empty;
            OnPropertyChanged(); 
        }
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    public RelayCommand(Action<T?> execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute((T?)parameter);
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}