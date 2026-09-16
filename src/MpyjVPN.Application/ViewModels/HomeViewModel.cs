using MpyjVPN.Application.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MpyjVPN.Application.ViewModels;

public class HomeViewModel : INotifyPropertyChanged
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

    private readonly CliService _cli = new();
    private readonly NetworkMonitor _netMonitor = new();

    public CliService Cli => _cli;
    public NetworkMonitor NetworkMonitor => _netMonitor;

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
        set => SetProperty(ref _currentMode, value);
    }

    private ProtocolViewModel? _activeConfig;
    public ProtocolViewModel? ActiveConfig
    {
        get => _activeConfig;
        set => SetProperty(ref _activeConfig, value);
    }

    public ObservableCollection<ProtocolViewModel> ImportedConfigs { get; } = new();
    public ObservableCollection<StackLayerViewModel> StackLayers { get; } = new();
    public ObservableCollection<string> Logs { get; } = new();

    public HomeViewModel()
    {
        _netMonitor.SpeedUpdated += OnSpeedUpdated;
        _netMonitor.PingUpdated += OnPingUpdated;
        _netMonitor.LossUpdated += OnLossUpdated;
    }

    private void OnSpeedUpdated(double downloadMBps, double uploadMBps)
    {
        Download = FormatSpeed(downloadMBps);
        Upload = FormatSpeed(uploadMBps);
    }

    private void OnPingUpdated(int pingMs)
    {
        Ping = pingMs > 0 ? $"{pingMs}ms" : "--";
    }

    private void OnLossUpdated(double lossPercent)
    {
        Loss = $"{lossPercent:F0}%";
    }

    private static string FormatSpeed(double mbps)
    {
        if (mbps < 0.01) return "0 KB/s";
        if (mbps < 1) return $"{mbps * 1024:F0} KB/s";
        return $"{mbps:F1} MB/s";
    }

    public void AddLog(string message)
    {
        Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        while (Logs.Count > 100) Logs.RemoveAt(Logs.Count - 1);
    }

    public void ResetState()
    {
        IsConnected = false;
        IsConnecting = false;
        StatusText = "Not Connected";
        ServerInfo = "Ready to connect";
        IpAddress = "0.0.0.0";
        Download = "--";
        Upload = "--";
        Loss = "--";
        Ping = "--";
        _netMonitor.Stop();
    }
}