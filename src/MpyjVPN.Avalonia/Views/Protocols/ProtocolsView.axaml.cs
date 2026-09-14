using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MpyjVPN.Application.ViewModels;
using MpyjVPN.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MpyjVPN.Avalonia.Views.Protocols;

public partial class ProtocolsView : UserControl
{
    private ProtocolViewModel? _selectedProtocol;

    public ObservableCollection<ProtocolViewModel> Protocols { get; } = new();
    private List<ProtocolViewModel> _allProtocols = new();

    public ProtocolsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        this.DataContext = this;

        var search = this.FindControl<TextBox>("ProtocolSearch");
        if (search != null)
            search.TextChanged += OnProtocolSearchChanged;

        LoadProtocols();
    }

    private void LoadProtocols()
    {
        _allProtocols.Clear();

        _allProtocols.Add(new ProtocolViewModel { Name = "WARP", Icon = "🌐", Description = "Fast & secure", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "42ms", PingValue = 42 });
        _allProtocols.Add(new ProtocolViewModel { Name = "DoH", Icon = "🔍", Description = "DNS over HTTPS", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "51ms", PingValue = 51 });
        _allProtocols.Add(new ProtocolViewModel { Name = "Fragment", Icon = "📦", Description = "DPI bypass", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "55ms", PingValue = 55 });
        _allProtocols.Add(new ProtocolViewModel { Name = "Worker", Icon = "☁️", Description = "Cloudflare Worker", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "67ms", PingValue = 67 });
        _allProtocols.Add(new ProtocolViewModel { Name = "ECH", Icon = "🔐", Description = "Encrypted Client Hello", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "QUIC", Icon = "⚡", Description = "Fast UDP protocol", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "WARP in WARP", Icon = "🧅", Description = "WARP over WARP", Type = "Warp-Mode", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "WARP + Worker", Icon = "☁️", Description = "WARP + Cloudflare Worker", Type = "Warp-Mode", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "WARP Full Stack", Icon = "💀", Description = "Full WARP stack", Type = "Warp-Mode", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "Fronting", Icon = "🎭", Description = "Domain fronting", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "Multi-hop", Icon = "🔗", Description = "Multi-hop chain", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "DNS Tunnel", Icon = "🔍", Description = "DNS tunneling", Type = "Built-in", Port = 53, IsEnabled = false, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "IP Spoof", Icon = "🕶️", Description = "IP spoofing", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "SNI Spoof", Icon = "🎭", Description = "SNI spoofing", Type = "Built-in", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "WebSocket", Icon = "🔌", Description = "WebSocket tunnel", Type = "Built-in", Port = 443, IsEnabled = false, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "ICMP", Icon = "📡", Description = "Ping tunnel", Type = "Built-in", Port = 0, IsEnabled = false, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "Google Proxy", Icon = "🌐", Description = "Google Translate", Type = "Built-in", Port = 443, IsEnabled = false, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "WARP in WARP", Icon = "🧅", Description = "WARP over WARP", Type = "Warp-Mode", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "WARP + Worker", Icon = "☁️", Description = "WARP + Cloudflare Worker", Type = "Warp-Mode", Port = 443, IsEnabled = true, Ping = "--" });
        _allProtocols.Add(new ProtocolViewModel { Name = "WARP Full Stack", Icon = "💀", Description = "Full WARP stack", Type = "Warp-Mode", Port = 443, IsEnabled = true, Ping = "--" });

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Protocols.Clear();

        var search = this.FindControl<TextBox>("ProtocolSearch");
        var searchText = search?.Text?.ToLower() ?? "";

        var filtered = string.IsNullOrWhiteSpace(searchText)
            ? _allProtocols
            : _allProtocols.Where(p =>
                p.Name.ToLower().Contains(searchText) ||
                p.Description.ToLower().Contains(searchText)).ToList();

        foreach (var p in filtered)
            Protocols.Add(p);
    }

    private void OnProtocolSearchChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void OnRefreshProtocols(object? sender, RoutedEventArgs e)
    {
        LoadProtocols();
    }

    private void OnProtocolClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not ProtocolViewModel protocol) return;

        _selectedProtocol = protocol;

        var emptyPanel = this.FindControl<StackPanel>("DetailEmpty");
        var contentPanel = this.FindControl<StackPanel>("DetailContent");

        if (emptyPanel != null) emptyPanel.IsVisible = false;
        if (contentPanel != null) contentPanel.IsVisible = true;

        var icon = this.FindControl<TextBlock>("DetailIcon");
        if (icon != null) icon.Text = protocol.Icon;

        var name = this.FindControl<TextBlock>("DetailName");
        if (name != null) name.Text = protocol.Name;

        var desc = this.FindControl<TextBlock>("DetailDescription");
        if (desc != null) desc.Text = protocol.Description;

        var status = this.FindControl<TextBlock>("DetailStatus");
        if (status != null)
        {
            status.Text = protocol.IsEnabled ? "✅ Enabled" : "❌ Disabled";
            status.Foreground = new SolidColorBrush(Color.Parse(
                protocol.IsEnabled ? "#10b981" : "#ef4444"));
        }

        var ping = this.FindControl<TextBlock>("DetailPing");
        if (ping != null) ping.Text = protocol.Ping;

        var type = this.FindControl<TextBlock>("DetailType");
        if (type != null) type.Text = protocol.Type;

        var port = this.FindControl<TextBlock>("DetailPort");
        if (port != null) port.Text = protocol.Port.ToString();
    }

    private async void OnTestPing(object? sender, RoutedEventArgs e)
    {
        if (_selectedProtocol == null) return;

        var address = string.IsNullOrEmpty(_selectedProtocol.ServerAddress)
            ? "1.1.1.1"
            : _selectedProtocol.ServerAddress;

        var ping = await PingService.PingAsync(address, 3000);

        if (ping > 0)
        {
            _selectedProtocol.Ping = $"{ping}ms";
            _selectedProtocol.PingValue = ping;

            var pingTb = this.FindControl<TextBlock>("DetailPing");
            if (pingTb != null) pingTb.Text = $"{ping}ms";
        }
        else
        {
            var pingTb = this.FindControl<TextBlock>("DetailPing");
            if (pingTb != null) pingTb.Text = "Timeout";
        }
    }
}
