using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MpyjVPN.Avalonia.Services;
using MpyjVPN.Avalonia.Views;
using System;

namespace MpyjVPN.Avalonia;

public partial class App : global::Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;

            // ShutdownMode: فقط وقتی Exit زده شد
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // TrayIcon - ساده شده (بدون Connect/Disconnect)
            var trayService = new TrayService(
                mainWindow,
                getIsConnected: () => false,
                onConnect: () => { },
                onDisconnect: () => { }
            );
            trayService.Initialize();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
