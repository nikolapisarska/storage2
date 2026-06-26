using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace storage.Model;

public class Box
{
    public string BoxCode { get; set; } = string.Empty;
    public float Height { get; set; }
    public float Width { get; set; }
    public float Length { get; set; }
    public float Weight { get; set; }

    public List<BoxItem> Items { get; set; } = new List<BoxItem>();
}
public class BoxItem : INotifyPropertyChanged
{
    // Zmieniamy pole na nullable int?, aby bezpiecznie przyjąć "puste" pole z UI
    private int? _quantity = 1; 
    public Product Product { get; set; } = null!;
    
    public int? Quantity
    {
        get => _quantity;
        set
        {
            // 1. Jeśli użytkownik skasuje zawartość (null) lub wpisze < 1, ustawiamy domyślnie 1
            int validatedValue = (value == null || value < 1) ? 1 : value.Value;
        
            if (_quantity != validatedValue)
            {
                _quantity = validatedValue;
                OnPropertyChanged(nameof(Quantity)); 
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}