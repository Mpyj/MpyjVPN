using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MpyjVPN.Application.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    private string _language = "fa";
    public string Language
    {
        get => _language;
        set { _language = value; OnPropertyChanged(); }
    }
    
    private string _theme = "dark";
    public string Theme
    {
        get => _theme;
        set { _theme = value; OnPropertyChanged(); }
    }
    
    private bool _autoReconnect = true;
    public bool AutoReconnect
    {
        get => _autoReconnect;
        set { _autoReconnect = value; OnPropertyChanged(); }
    }
    
    private string _primaryDns = "1.1.1.1";
    public string PrimaryDns
    {
        get => _primaryDns;
        set { _primaryDns = value; OnPropertyChanged(); }
    }
    
    private string _secondaryDns = "1.0.0.1";
    public string SecondaryDns
    {
        get => _secondaryDns;
        set { _secondaryDns = value; OnPropertyChanged(); }
    }
    
    private string _workerUrl = "https://shy-glade-59ba.urihghioihrerg.workers.dev";
    public string WorkerUrl
    {
        get => _workerUrl;
        set { _workerUrl = value; OnPropertyChanged(); }
    }
}