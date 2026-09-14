using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MpyjVPN.Application.ViewModels;

public class DiagnosticsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    private string _ping = "--";
    public string Ping
    {
        get => _ping;
        set { _ping = value; OnPropertyChanged(); }
    }
    
    private string _dns = "--";
    public string Dns
    {
        get => _dns;
        set { _dns = value; OnPropertyChanged(); }
    }
    
    private string _internet = "--";
    public string Internet
    {
        get => _internet;
        set { _internet = value; OnPropertyChanged(); }
    }
    
    private string _status = "Not tested";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }
    
    public ObservableCollection<string> Logs { get; } = new();
    
    public void AddLog(string message)
    {
        Logs.Insert(0, $"[{System.DateTime.Now:HH:mm:ss}] {message}");
        if (Logs.Count > 100) Logs.RemoveAt(Logs.Count - 1);
    }
}