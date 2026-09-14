using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MpyjVPN.Application.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
    
    // ==================== STATE ====================
    
    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }
    
    private bool _isConnecting;
    public bool IsConnecting
    {
        get => _isConnecting;
        set => SetProperty(ref _isConnecting, value);
    }
    
    private string _statusText = "Not Connected";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
    
    private string _serverInfo = "Ready to connect";
    public string ServerInfo
    {
        get => _serverInfo;
        set => SetProperty(ref _serverInfo, value);
    }
    
    private string _ipAddress = "0.0.0.0";
    public string IpAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }
    
    private string _download = "--";
    public string Download
    {
        get => _download;
        set => SetProperty(ref _download, value);
    }
    
    private string _upload = "--";
    public string Upload
    {
        get => _upload;
        set => SetProperty(ref _upload, value);
    }
    
    private string _loss = "--";
    public string Loss
    {
        get => _loss;
        set => SetProperty(ref _loss, value);
    }
    
    private string _ping = "--";
    public string Ping
    {
        get => _ping;
        set => SetProperty(ref _ping, value);
    }
    
    private string _currentMode = "auto";
    public string CurrentMode
    {
        get => _currentMode;
        set
        {
            if (SetProperty(ref _currentMode, value))
            {
                OnPropertyChanged(nameof(IsAutoMode));
                OnPropertyChanged(nameof(IsOnionMode));
                OnPropertyChanged(nameof(IsProtocolsMode));
                OnPropertyChanged(nameof(IsUltraMode));
            }
        }
    }
    
    // Ø¨Ø±Ø§ÛŒ UI Binding
    public bool IsAutoMode => CurrentMode == "auto";
    public bool IsOnionMode => CurrentMode == "onion";
    public bool IsProtocolsMode => CurrentMode == "protocols";
    public bool IsUltraMode => CurrentMode == "ultra";
    
    private int _currentPage;
    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }
}