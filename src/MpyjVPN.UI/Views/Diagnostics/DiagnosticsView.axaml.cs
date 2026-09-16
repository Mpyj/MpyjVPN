using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MpyjVPN.Application.Services;
using MpyjVPN.Application.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MpyjVPN.UI.Views.Diagnostics;

public partial class DiagnosticsView : UserControl
{
    private readonly CliService _cli = new();

    public ObservableCollection<string> Logs => LogService.Logs;
    public ObservableCollection<ScannedIPViewModel> ScannedIPs { get; } = new();

    public DiagnosticsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        this.DataContext = this;
        AddLog("✅ Diagnostics ready");
    }

    private void AddLog(string message)
    {
        LogService.Add(message);
    }

    private async void OnRunDiagnostics(object? sender, RoutedEventArgs e)
    {
        AddLog("🔍 Running diagnostics...");
        try
        {
            var result = await _cli.RunDiagnosticsAsync();
            if (string.IsNullOrWhiteSpace(result))
            {
                AddLog("⚠️ No result from CLI");
                return;
            }

            foreach (var line in result.Split('\n'))
                if (!string.IsNullOrWhiteSpace(line))
                    AddLog(line.Trim());

            AddLog("✅ Diagnostics complete");
        }
        catch (Exception ex)
        {
            AddLog($"❌ Error: {ex.Message}");
        }
    }

    private async void OnScanIPs(object? sender, RoutedEventArgs e)
    {
        AddLog("📡 Scanning IPs...");

        try
        {
            var result = await _cli.ScanIPsAsync("cloudflare");

            if (string.IsNullOrWhiteSpace(result))
            {
                AddLog("⚠️ No result from CLI");
                return;
            }

            foreach (var line in result.Split('\n'))
                if (!string.IsNullOrWhiteSpace(line))
                    AddLog(line.Trim());

            // Parse IPs
            AddLog($"🔧 Parsing result...");

            var ips = IPScannerService.Parse(result);
            AddLog($"🔧 Parsed {ips.Count} IPs");

            ScannedIPs.Clear();
            for (int i = 0; i < ips.Count; i++)
            {
                var ip = ips[i];
                var rank = i switch { 0 => "🥇", 1 => "🥈", 2 => "🥉", _ => $"#{i + 1}" };

                ScannedIPs.Add(new ScannedIPViewModel
                {
                    Rank = rank,
                    Address = ip.Address,
                    PingText = $"{ip.Ping}ms",
                    PingValue = ip.Ping,
                    IsWorking = ip.IsWorking
                });
            }

            var panel = this.FindControl<Border>("IPScannerPanel");
            if (panel != null)
            {
                panel.IsVisible = ScannedIPs.Count > 0;
                AddLog($"🔧 Panel visibility: {panel.IsVisible}");
            }

            var countLabel = this.FindControl<TextBlock>("IPScannerCount");
            if (countLabel != null)
                countLabel.Text = $"{ScannedIPs.Count} found";

            AddLog($"✅ Scan complete - {ScannedIPs.Count} IPs");
        }
        catch (Exception ex)
        {
            AddLog($"❌ Error: {ex.Message}");
        }
    }

    private async void OnGetLayers(object? sender, RoutedEventArgs e)
    {
        AddLog("🧅 Loading layers...");
        try
        {
            var result = await _cli.GetLayersAsync();
            if (string.IsNullOrWhiteSpace(result))
            {
                AddLog("⚠️ No result from CLI");
                return;
            }

            foreach (var line in result.Split('\n'))
                if (!string.IsNullOrWhiteSpace(line))
                    AddLog(line.Trim());

            AddLog("✅ Layers loaded");
        }
        catch (Exception ex)
        {
            AddLog($"❌ Error: {ex.Message}");
        }
    }

    private async void OnCopyLog(object? sender, RoutedEventArgs e)
    {
        try
        {
            var text = string.Join("\n", Logs);
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text);
                AddLog("📋 Copied to clipboard");
            }
        }
        catch (Exception ex)
        {
            AddLog($"❌ Copy failed: {ex.Message}");
        }
    }

    private void OnClearLog(object? sender, RoutedEventArgs e)
    {
        LogService.Clear();
    }

    private void OnSortIPs(object? sender, RoutedEventArgs e)
    {
        var sorted = ScannedIPs.OrderBy(x => x.PingValue).ToList();
        ScannedIPs.Clear();
        for (int i = 0; i < sorted.Count; i++)
        {
            var ip = sorted[i];
            ip.Rank = i switch { 0 => "🥇", 1 => "🥈", 2 => "🥉", _ => $"#{i + 1}" };
            ScannedIPs.Add(ip);
        }
    }

    private async void OnCopyIPs(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (ScannedIPs.Count == 0) return;
            var sb = new System.Text.StringBuilder();
            foreach (var ip in ScannedIPs)
                sb.AppendLine($"{ip.Address} ({ip.PingText})");

            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(sb.ToString());
                AddLog($"📋 Copied {ScannedIPs.Count} IPs");
            }
        }
        catch (Exception ex)
        {
            AddLog($"❌ Copy failed: {ex.Message}");
        }
    }
}