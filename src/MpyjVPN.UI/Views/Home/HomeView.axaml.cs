using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MpyjVPN.UI.Controls;
using MpyjVPN.Application.Services;
using MpyjCore;
using MpyjVPN.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MpyjVPN.UI.Views.Home;

public partial class HomeView : UserControl
{
    private readonly CliService _cli = new();
    private readonly VpnEngine _engine = new();
    private readonly NetworkMonitor _netMonitor = new();

    private bool _isConnected = false;
    private bool _isConnecting = false;
    private string _currentMode = "auto";

    private ProtocolViewModel? _activeConfig;

    // Elements
    private PowerOrb? _powerButton;
    private Button? _autoButton;
    private Button? _onionButton;
    private Button? _protocolsButton;
    private Button? _ultraButton;
    private Button? _configsButton;
    private TextBlock? _statusText;
    private TextBlock? _serverInfo;
    private Border? _connectionCard;
    private Ellipse? _ipDot;
    private TextBlock? _ipText;
    private TextBlock? _downloadVal;
    private TextBlock? _uploadVal;
    private TextBlock? _lossVal;
    private TextBlock? _pingVal;

    public ObservableCollection<ProtocolViewModel> ImportedConfigs { get; } = new();
    public ObservableCollection<StackLayerViewModel> StackLayers { get; } = new();

    public HomeView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _powerButton = this.FindControl<PowerOrb>("PowerButton");
        _autoButton = this.FindControl<Button>("AutoButton");
        _onionButton = this.FindControl<Button>("OnionButton");
        _protocolsButton = this.FindControl<Button>("ProtocolsButton");
        _ultraButton = this.FindControl<Button>("UltraButton");
        _configsButton = this.FindControl<Button>("ConfigsButton");
        _statusText = this.FindControl<TextBlock>("StatusText");
        _serverInfo = this.FindControl<TextBlock>("ServerInfo");
        _connectionCard = this.FindControl<Border>("ConnectionCard");
        _ipDot = this.FindControl<Ellipse>("IpDot");
        _ipText = this.FindControl<TextBlock>("IpText");
        _downloadVal = this.FindControl<TextBlock>("DownloadVal");
        _uploadVal = this.FindControl<TextBlock>("UploadVal");
        _lossVal = this.FindControl<TextBlock>("LossVal");
        _pingVal = this.FindControl<TextBlock>("PingVal");

        this.DataContext = this;

        _netMonitor.SpeedUpdated += OnSpeedUpdated;
        _netMonitor.PingUpdated += OnPingUpdated;
        _netMonitor.LossUpdated += OnLossUpdated;

        PopulateLayerPicker();
        LoadStoredConfigs();

        if (_powerButton != null)
            _powerButton.Click += OnPowerButtonClick;

        LogService.Add("✅ HomeView loaded");
    }

    // ==================== POWER ====================

    private async void OnPowerButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_isConnecting) return;
        if (_powerButton != null) _powerButton.IsEnabled = false;
        try
        {
            if (!_isConnected) await ConnectAsync();
            else await DisconnectAsync();
        }
        finally { if (_powerButton != null) _powerButton.IsEnabled = true; }
    }

    private void OnModeButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is not string mode) return;

        if (_isConnected)
        {
            if (_serverInfo != null)
                _serverInfo.Text = "⚠️ Disconnect first";
            return;
        }

        _currentMode = mode;
        // Config Mode موقتاً غیرفعال
        if (mode == "configs")
        {
            if (_serverInfo != null)
                _serverInfo.Text = "⏳ Config Mode coming soon";
            LogService.Add("⏳ Config Mode coming soon");
            return;
        }
        LogService.Add($"🎮 Mode changed: {mode.ToUpper()}");

        SetClass(_autoButton, "Active", mode == "auto");
        SetClass(_onionButton, "Active", mode == "onion");
        SetClass(_protocolsButton, "Active", mode == "protocols");
        SetClass(_ultraButton, "Active", mode == "ultra");
        SetClass(_configsButton, "Active", mode == "configs");

        var card = this.FindControl<Border>("ConfigSelectionCard");
        if (card != null)
            card.IsVisible = mode == "configs";

        var stackSection = this.FindControl<StackPanel>("StackLayersSection");
        if (stackSection != null)
            stackSection.IsVisible = mode == "configs";

        if (mode != "configs")
        {
            _activeConfig = null;
            StackLayers.Clear();
            var combo = this.FindControl<ComboBox>("ConfigSelectCombo");
            if (combo != null) combo.SelectedIndex = -1;
            UpdateConfigPreview();
            UpdateLayersUI();
        }
    }

    // ==================== CONFIG ====================

    private async void OnImportConfig(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ImportConfigWindow();
            var result = await dialog.ShowDialog<List<ProtocolViewModel>?>(TopLevel.GetTopLevel(this) as Window);

            if (result != null && result.Count > 0)
            {
                foreach (var protocol in result)
                    ImportedConfigs.Add(protocol);

                UpdateConfigsVisibility();
                SaveStoredConfigs();

                LogService.Add($"✅ Imported {result.Count} config(s)");

                if (_serverInfo != null)
                    _serverInfo.Text = $"✅ Imported {result.Count} config(s)";
            }
        }
        catch (Exception ex)
        {
            LogService.Add($"❌ Import failed: {ex.Message}");
            if (_serverInfo != null)
                _serverInfo.Text = $"❌ Import failed: {ex.Message}";
        }
    }

    private void OnDeleteConfig(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not ProtocolViewModel protocol) return;

        ImportedConfigs.Remove(protocol);
        UpdateConfigsVisibility();
        SaveStoredConfigs();

        LogService.Add($"🗑️ Deleted: {protocol.Name}");
    }

    private void OnConfigSelectChanged(object? sender, SelectionChangedEventArgs e)
    {
        var combo = this.FindControl<ComboBox>("ConfigSelectCombo");
        if (combo?.SelectedItem is ProtocolViewModel p)
        {
            _activeConfig = p;
            StackLayers.Clear();
            UpdateLayersUI();
            LogService.Add($"📄 Config selected: {p.Name}");

            if (_serverInfo != null)
                _serverInfo.Text = $"📄 {p.Name}";
        }
        else
        {
            _activeConfig = null;
        }

        UpdateConfigPreview();
    }

    private void UpdateConfigPreview()
    {
        var previewBox = this.FindControl<Border>("ConfigPreviewBox");
        var previewText = this.FindControl<TextBlock>("ConfigPreviewText");

        if (_activeConfig != null)
        {
            if (previewBox != null) previewBox.IsVisible = true;

            var parts = new List<string>();
            parts.Add($"{_activeConfig.Icon} {_activeConfig.Name}");

            foreach (var layer in StackLayers)
                parts.Add($"{layer.Icon} {layer.Name}");

            if (previewText != null)
                previewText.Text = string.Join("  →  ", parts);
        }
        else
        {
            if (previewBox != null) previewBox.IsVisible = false;
            if (previewText != null) previewText.Text = "—";
        }
    }

    private void UpdateConfigsVisibility()
    {
        var empty = this.FindControl<StackPanel>("ConfigsEmpty");
        var list = this.FindControl<ItemsControl>("ConfigsList");
        var count = this.FindControl<TextBlock>("ConfigsCount");

        if (empty != null)
            empty.IsVisible = ImportedConfigs.Count == 0;
        if (list != null)
            list.IsVisible = ImportedConfigs.Count > 0;
        if (count != null)
            count.Text = ImportedConfigs.Count.ToString();

        UpdateConfigSelectionCard();
    }

    private void UpdateConfigSelectionCard()
    {
        var emptyPanel = this.FindControl<StackPanel>("ConfigSelectionEmpty");
        var combo = this.FindControl<ComboBox>("ConfigSelectCombo");

        if (ImportedConfigs.Count == 0)
        {
            if (emptyPanel != null) emptyPanel.IsVisible = true;
            if (combo != null)
            {
                combo.IsVisible = false;
                combo.ItemsSource = null;
            }
        }
        else
        {
            if (emptyPanel != null) emptyPanel.IsVisible = false;
            if (combo != null)
            {
                combo.IsVisible = true;
                combo.ItemsSource = null;
                combo.ItemsSource = ImportedConfigs.ToList();

                if (_activeConfig != null && ImportedConfigs.Contains(_activeConfig))
                    combo.SelectedItem = _activeConfig;
            }
        }

        UpdateConfigPreview();
    }

    // ==================== LAYERS ====================

    private void OnAddLayer(object? sender, RoutedEventArgs e)
    {
        var picker = this.FindControl<ComboBox>("LayerPickerCombo");
        if (picker?.SelectedItem is not ProtocolViewModel selectedLayer) return;

        if (StackLayers.Any(l => l.Name == selectedLayer.Name))
            return;

        StackLayers.Add(new StackLayerViewModel
        {
            Name = selectedLayer.Name,
            Icon = selectedLayer.Icon,
            Index = StackLayers.Count
        });

        picker.SelectedIndex = -1;
        UpdateLayersUI();
        UpdateConfigPreview();

        LogService.Add($"🔗 Layer added: {selectedLayer.Name}");
    }

    private void OnRemoveLayer(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not StackLayerViewModel layer) return;

        StackLayers.Remove(layer);
        ReindexLayers();
        UpdateLayersUI();
        UpdateConfigPreview();

        LogService.Add($"🗑️ Layer removed: {layer.Name}");
    }

    private void OnMoveLayerUp(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not StackLayerViewModel layer) return;

        var index = StackLayers.IndexOf(layer);
        if (index > 0)
        {
            StackLayers.Move(index, index - 1);
            ReindexLayers();
            UpdateConfigPreview();
        }
    }

    private void OnMoveLayerDown(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not StackLayerViewModel layer) return;

        var index = StackLayers.IndexOf(layer);
        if (index < StackLayers.Count - 1)
        {
            StackLayers.Move(index, index + 1);
            ReindexLayers();
            UpdateConfigPreview();
        }
    }

    private void ReindexLayers()
    {
        for (int i = 0; i < StackLayers.Count; i++)
            StackLayers[i].Index = i;
    }

    private void UpdateLayersUI()
    {
        var noLayers = this.FindControl<TextBlock>("NoLayersText");
        var listContainer = this.FindControl<Border>("LayersListContainer");

        if (noLayers != null)
            noLayers.IsVisible = StackLayers.Count == 0;
        if (listContainer != null)
            listContainer.IsVisible = StackLayers.Count > 0;
    }

    private void PopulateLayerPicker()
    {
        var picker = this.FindControl<ComboBox>("LayerPickerCombo");
        if (picker == null) return;

        var layerOptions = new List<ProtocolViewModel>
        {
            new ProtocolViewModel { Name = "SNI Spoof", Icon = "🎭", Description = "SNI spoofing" },
            new ProtocolViewModel { Name = "DNS Tunnel", Icon = "🔍", Description = "DNS tunneling" },
            new ProtocolViewModel { Name = "Fragment", Icon = "📦", Description = "DPI bypass" },
            new ProtocolViewModel { Name = "Worker", Icon = "☁️", Description = "Cloudflare Worker" },
            new ProtocolViewModel { Name = "ECH", Icon = "🔐", Description = "Encrypted Client Hello" },
            new ProtocolViewModel { Name = "QUIC", Icon = "⚡", Description = "Fast UDP" },
            new ProtocolViewModel { Name = "Fronting", Icon = "🎭", Description = "Domain fronting" },
            new ProtocolViewModel { Name = "Multi-hop", Icon = "🔗", Description = "Multi-hop chain" },
            new ProtocolViewModel { Name = "IP Spoof", Icon = "🕶️", Description = "IP spoofing" },
            new ProtocolViewModel { Name = "DoH", Icon = "🔍", Description = "DNS over HTTPS" },
            new ProtocolViewModel { Name = "WARP in WARP", Icon = "🧅", Description = "WARP over WARP" },
            new ProtocolViewModel { Name = "WARP + Worker", Icon = "☁️", Description = "WARP + Cloudflare Worker" },
            new ProtocolViewModel { Name = "WARP Full Stack", Icon = "💀", Description = "Full WARP stack" },
            new ProtocolViewModel { Name = "WARP", Icon = "🌐", Description = "Cloudflare WARP" },
            new ProtocolViewModel { Name = "WebSocket", Icon = "🔌", Description = "WebSocket tunnel" },
            new ProtocolViewModel { Name = "ICMP", Icon = "📡", Description = "Ping tunnel" },
            new ProtocolViewModel { Name = "Google Proxy", Icon = "🌐", Description = "Google Translate" },
            new ProtocolViewModel { Name = "WARP in WARP", Icon = "🧅", Description = "WARP over WARP" },
            new ProtocolViewModel { Name = "WARP + Worker", Icon = "☁️", Description = "WARP + Cloudflare Worker" },
            new ProtocolViewModel { Name = "WARP Full Stack", Icon = "💀", Description = "Full WARP stack" },
        };

        picker.ItemsSource = layerOptions;
        picker.SelectedIndex = -1;
    }

    // ==================== CONNECT ====================

    private async Task ConnectAsync()
    {
        _isConnecting = true;

        if (_powerButton != null)
        {
            _powerButton.IsConnecting = true;
            _powerButton.IsConnected = false;
        }

        if (_statusText != null)
        {
            _statusText.Text = "Connecting...";
            _statusText.Foreground = new SolidColorBrush(Color.Parse("#f59e0b"));
        }

        var connectTarget = _currentMode;
        if (_currentMode == "configs" && _activeConfig != null)
            connectTarget = _activeConfig.Name;

        if (_serverInfo != null)
            _serverInfo.Text = $"Connecting {connectTarget}...";

        LogService.Add($"🚀 Connecting: {connectTarget}...");

        try
        {
            if (_currentMode == "configs" && _activeConfig != null
                && !string.IsNullOrEmpty(_activeConfig.RawConfig))
            {
                // استفاده از VpnEngine مستقیم (نه CliService)
                LogService.Add($"📄 Loading config: {_activeConfig.Name}");

                bool success;
                if (StackLayers.Count > 0)
                {
                    var layers = StackLayers.Select(l => l.Name).ToList();
                    LogService.Add($"🔗 Connecting with {layers.Count} layer(s)");
                    success = await _engine.ConnectWithConfigAndLayersAsync(_activeConfig.RawConfig, layers);
                }
                else
                {
                    success = await _engine.ConnectWithConfigAsync(_activeConfig.RawConfig);
                }

                if (!success)
                {
                    if (_statusText != null)
                    {
                        _statusText.Text = "Failed";
                        _statusText.Foreground = new SolidColorBrush(Color.Parse("#ef4444"));
                    }
                    if (_serverInfo != null)
                        _serverInfo.Text = "Config load failed";

                    LogService.Add($"❌ Config load failed");
                    _isConnecting = false;
                    if (_powerButton != null)
                    {
                        _powerButton.IsConnecting = false;
                        _powerButton.IsConnected = false;
                    }
                    return;
                }

                _isConnected = true;

                if (_powerButton != null)
                {
                    _powerButton.IsConnecting = false;
                    _powerButton.IsConnected = true;
                }

                if (_statusText != null)
                {
                    _statusText.Text = "Protected";
                    _statusText.Foreground = new SolidColorBrush(Color.Parse("#10b981"));
                }

                if (_serverInfo != null)
                    _serverInfo.Text = $"✅ Connected • {_activeConfig.Name}";

                if (_connectionCard != null)
                    _connectionCard.BorderBrush = new SolidColorBrush(Color.Parse("#10b981"));

                if (_ipDot != null)
                {
                    _ipDot.Fill = new SolidColorBrush(Color.Parse("#10b981"));
                    if (!_ipDot.Classes.Contains("pulse"))
                        _ipDot.Classes.Add("pulse");
                }

                if (_ipText != null) _ipText.Text = "185.51.200.2";

                _netMonitor.Start();

                LogService.Add($"✅ Connected • {_activeConfig.Name}");
                _isConnecting = false;
                return;
            }

            if (_currentMode == "configs" && _activeConfig == null)
            {
                if (_serverInfo != null)
                    _serverInfo.Text = "Select a config first";

                LogService.Add($"⚠️ No config selected");
                _isConnecting = false;
                if (_powerButton != null)
                {
                    _powerButton.IsConnecting = false;
                    _powerButton.IsConnected = false;
                }
                return;
            }

            var result = await _cli.ConnectAsync(_currentMode);

            if (result.Contains("✅"))
            {
                _isConnected = true;
                if (_powerButton != null)
                {
                    _powerButton.IsConnecting = false;
                    _powerButton.IsConnected = true;
                }
                if (_statusText != null)
                {
                    _statusText.Text = "Protected";
                    _statusText.Foreground = new SolidColorBrush(Color.Parse("#10b981"));
                }
                if (_serverInfo != null)
                    _serverInfo.Text = $"✅ Connected • {_currentMode.ToUpper()}";

                _netMonitor.Start();
                LogService.Add($"✅ Connected • {_currentMode.ToUpper()}");
            }
            else
            {
                if (_powerButton != null)
                {
                    _powerButton.IsConnecting = false;
                    _powerButton.IsConnected = false;
                }
                if (_statusText != null)
                {
                    _statusText.Text = "Failed";
                    _statusText.Foreground = new SolidColorBrush(Color.Parse("#ef4444"));
                }
                LogService.Add($"❌ Connection failed");
            }
        }
        catch (Exception ex)
        {
            if (_powerButton != null)
            {
                _powerButton.IsConnecting = false;
                _powerButton.IsConnected = false;
            }
            if (_serverInfo != null)
                _serverInfo.Text = $"❌ {ex.Message}";

            LogService.Add($"❌ {ex.Message}");
        }

        _isConnecting = false;
    }

    private async Task DisconnectAsync()
    {
        _isConnecting = true;
        _netMonitor.Stop();

        try
        {
            await _cli.DisconnectAsync();
            await _engine.DisconnectAsync();
        }
        catch { }

        _isConnected = false;

        if (_powerButton != null)
        {
            _powerButton.IsConnected = false;
            _powerButton.IsConnecting = false;
        }

        if (_statusText != null)
        {
            _statusText.Text = "Not Connected";
            _statusText.Foreground = new SolidColorBrush(Color.Parse("#a0a0c0"));
        }

        if (_serverInfo != null)
            _serverInfo.Text = "Ready to connect";

        if (_connectionCard != null)
            _connectionCard.BorderBrush = new SolidColorBrush(Color.Parse("#252535"));

        if (_ipDot != null)
        {
            _ipDot.Fill = new SolidColorBrush(Color.Parse("#606080"));
            _ipDot.Classes.Remove("pulse");
        }

        if (_ipText != null) _ipText.Text = "0.0.0.0";
        if (_downloadVal != null) _downloadVal.Text = "--";
        if (_uploadVal != null) _uploadVal.Text = "--";
        if (_lossVal != null) _lossVal.Text = "--";
        if (_pingVal != null) _pingVal.Text = "--";

        LogService.Add("✅ Disconnected");
        _isConnecting = false;
    }

    // ==================== MONITOR ====================

    private void OnSpeedUpdated(double downloadMBps, double uploadMBps)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_downloadVal != null) _downloadVal.Text = FormatSpeed(downloadMBps);
            if (_uploadVal != null) _uploadVal.Text = FormatSpeed(uploadMBps);
        });
    }

    private void OnPingUpdated(int pingMs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_pingVal != null) _pingVal.Text = pingMs > 0 ? $"{pingMs}ms" : "--";
        });
    }

    private void OnLossUpdated(double lossPercent)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_lossVal != null) _lossVal.Text = $"{lossPercent:F0}%";
        });
    }

    private static string FormatSpeed(double mbps)
    {
        if (mbps < 0.01) return "0 KB/s";
        if (mbps < 1) return $"{mbps * 1024:F0} KB/s";
        return $"{mbps:F1} MB/s";
    }

    // ==================== STORAGE ====================

    private void LoadStoredConfigs()
    {
        try
        {
            var loaded = ConfigStorageService.LoadAsViewModels();
            foreach (var p in loaded)
                ImportedConfigs.Add(p);

            UpdateConfigsVisibility();

            if (loaded.Count > 0)
                LogService.Add($"📂 Loaded {loaded.Count} saved config(s)");
        }
        catch { }
    }

    private void SaveStoredConfigs()
    {
        try
        {
            ConfigStorageService.SaveFromViewModels(ImportedConfigs);
        }
        catch { }
    }

    // ==================== HELPERS ====================

    private static void SetClass(Button? btn, string className, bool active)
    {
        if (btn == null) return;
        if (active)
        {
            if (!btn.Classes.Contains(className))
                btn.Classes.Add(className);
        }
        else
        {
            btn.Classes.Remove(className);
        }
    }
}