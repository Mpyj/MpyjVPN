using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace MpyjVPN.UI.Services;

public class TrayService
{
    private readonly Window _mainWindow;
    private readonly Func<bool> _getIsConnected;
    private readonly Action _onConnect;
    private readonly Action _onDisconnect;
    private TrayIcon? _trayIcon;

    public TrayService(
        Window mainWindow,
        Func<bool> getIsConnected,
        Action onConnect,
        Action onDisconnect)
    {
        _mainWindow = mainWindow;
        _getIsConnected = getIsConnected;
        _onConnect = onConnect;
        _onDisconnect = onDisconnect;
    }

    public void Initialize()
    {
        _trayIcon = new TrayIcon
        {
            ToolTipText = "Mpyj VPN",
            IsVisible = true,
        };

        try
        {
            var iconUri = new Uri("avares://MpyjVPN.UI/Assets/icon.ico");
            if (AssetLoader.Exists(iconUri))
            {
                _trayIcon.Icon = new WindowIcon(AssetLoader.Open(iconUri));
            }
            else
            {
                // Fallback: Ø§Ø² exe Ø¢ÛŒÚ©ÙˆÙ† Ø¨Ú¯ÛŒØ±
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                    _trayIcon.Icon = new WindowIcon(exePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Tray icon load error: {ex.Message}");
        }

        BuildMenu();

        _trayIcon.Clicked += OnTrayClicked;

        if (global::Avalonia.Application.Current != null)
        {
            TrayIcon.SetIcons(global::Avalonia.Application.Current, new TrayIcons { _trayIcon });
        }
    }

    private void BuildMenu()
    {
        if (_trayIcon == null) return;

        var menu = new NativeMenu();

        var showItem = new NativeMenuItem("Show Window");
        showItem.Click += (s, e) => ShowWindow();
        menu.Add(showItem);

        menu.Add(new NativeMenuItemSeparator());

        var connectItem = new NativeMenuItem("Connect");
        connectItem.Click += (s, e) => _onConnect();
        menu.Add(connectItem);

        var disconnectItem = new NativeMenuItem("Disconnect");
        disconnectItem.Click += (s, e) => _onDisconnect();
        menu.Add(disconnectItem);

        menu.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (s, e) => ExitApp();
        menu.Add(exitItem);

        _trayIcon.Menu = menu;
    }

    private void OnTrayClicked(object? sender, EventArgs e)
    {
        ShowWindow();
    }

    private void ShowWindow()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApp()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}