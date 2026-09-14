using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MpyjVPN.Application.ViewModels;

public class ProtocolViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    
    // âœ… Ø¬Ø¯ÛŒØ¯: ØªÙˆØ¶ÛŒØ­Ø§Øª Ú©Ø§Ù…Ù„
    public string LongDescription { get; set; } = "";
    
    // âœ… Ø¬Ø¯ÛŒØ¯: Ù†ÙˆØ¹ (Built-in / Custom)
    public string Type { get; set; } = "Built-in";
    
    // âœ… Ø¬Ø¯ÛŒØ¯: Port
    public int Port { get; set; } = 443;
    
    // âœ… Ø¬Ø¯ÛŒØ¯: Ù„Ø§ÛŒÙ‡â€ŒÙ‡Ø§
    public string Layers { get; set; } = "";
    
    // âœ… Ø¬Ø¯ÛŒØ¯: Ø¢Ø¯Ø±Ø³ Ø³Ø±ÙˆØ±
    public string ServerAddress { get; set; } = "";
    
    // âœ… Ø¬Ø¯ÛŒØ¯: Ú©Ø§Ù†ÙÛŒÚ¯ Ø®Ø§Ù… (Ø¨Ø±Ø§ÛŒ Ø°Ø®ÛŒØ±Ù‡â€ŒØ³Ø§Ø²ÛŒ)
    public string RawConfig { get; set; } = "";
    
    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
    }
    
    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }
    }
    
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string _status = "Inactive";
    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string _ping = "--";
    public string Ping
    {
        get => _ping;
        set
        {
            if (_ping != value)
            {
                _ping = value;
                OnPropertyChanged();
            }
        }
    }
    
    private int _pingValue = 0;
    public int PingValue
    {
        get => _pingValue;
        set
        {
            if (_pingValue != value)
            {
                _pingValue = value;
                OnPropertyChanged();
            }
        }
    }
}