using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MpyjVPN.Application.ViewModels;

public class ProtocolDetailViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private bool _hasSelection = false;
    public bool HasSelection
    {
        get => _hasSelection;
        set { _hasSelection = value; OnPropertyChanged(); }
    }

    private string _name = "";
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private string _icon = "";
    public string Icon
    {
        get => _icon;
        set { _icon = value; OnPropertyChanged(); }
    }

    private string _description = "";
    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    private string _longDescription = "";
    public string LongDescription
    {
        get => _longDescription;
        set { _longDescription = value; OnPropertyChanged(); }
    }

    private string _type = "Built-in";
    public string Type
    {
        get => _type;
        set { _type = value; OnPropertyChanged(); }
    }

    private string _port = "443";
    public string Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    private string _layers = "";
    public string Layers
    {
        get => _layers;
        set { _layers = value; OnPropertyChanged(); }
    }

    private string _serverAddress = "";
    public string ServerAddress
    {
        get => _serverAddress;
        set { _serverAddress = value; OnPropertyChanged(); }
    }

    private bool _isEnabled = false;
    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    private string _statusText = "Inactive";
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    private string _statusIcon = "â­•";
    public string StatusIcon
    {
        get => _statusIcon;
        set { _statusIcon = value; OnPropertyChanged(); }
    }

    private string _pingText = "--";
    public string PingText
    {
        get => _pingText;
        set { _pingText = value; OnPropertyChanged(); }
    }

    private bool _isTesting = false;
    public bool IsTesting
    {
        get => _isTesting;
        set { _isTesting = value; OnPropertyChanged(); }
    }
}