using MpyjVPN.UI.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MpyjVPN.Application.Services;
using MpyjVPN.UI.Views.Home;
using MpyjVPN.UI.Views.Protocols;
using MpyjVPN.UI.Views.Diagnostics;
using MpyjVPN.UI.Views.Settings;
using System;

namespace MpyjVPN.UI.Views;

public partial class MainWindow : Window
{
    private StackPanel? _pageHome;
    private StackPanel? _pageProtocols;
    private StackPanel? _pageDiagnostics;
    private StackPanel? _pageSettings;

    private Button? _navHome;
    private Button? _navProtocols;
    private Button? _navDiagnostics;
    private Button? _navSettings;

    private HomeView? _homeView;
    private ProtocolsView? _protocolsView;
    private DiagnosticsView? _diagnosticsView;
    private SettingsView? _settingsView;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;

        // Subscribe to language changes
        LocalizationService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _pageHome = this.FindControl<StackPanel>("PageHome");
        _pageProtocols = this.FindControl<StackPanel>("PageProtocols");
        _pageDiagnostics = this.FindControl<StackPanel>("PageDiagnostics");
        _pageSettings = this.FindControl<StackPanel>("PageSettings");

        _navHome = this.FindControl<Button>("NavHome");
        _navProtocols = this.FindControl<Button>("NavProtocols");
        _navDiagnostics = this.FindControl<Button>("NavDiagnostics");
        _navSettings = this.FindControl<Button>("NavSettings");

        // Find Views
        _homeView = this.FindControl<HomeView>("HomeViewControl");
        _protocolsView = this.FindControl<ProtocolsView>("ProtocolsViewControl");
        _diagnosticsView = this.FindControl<DiagnosticsView>("DiagnosticsViewControl");
        _settingsView = this.FindControl<SettingsView>("SettingsViewControl");

        ApplyLanguage();
    }

    private void OnLanguageChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            ApplyLanguage();
        });
    }

    private void ApplyLanguage()
    {
        // Sidebar
        if (_navHome != null) _navHome.Content = LocalizationService.Get("NavHome");
        if (_navProtocols != null) _navProtocols.Content = LocalizationService.Get("NavProtocols");
        if (_navDiagnostics != null) _navDiagnostics.Content = LocalizationService.Get("NavDiagnostics");
        if (_navSettings != null) _navSettings.Content = LocalizationService.Get("NavSettings");

        // Views
        if (_homeView != null) LocalizationHelper.ApplyLanguage(_homeView);
        if (_protocolsView != null) LocalizationHelper.ApplyLanguage(_protocolsView);
        if (_diagnosticsView != null) LocalizationHelper.ApplyLanguage(_diagnosticsView);
        if (_settingsView != null) LocalizationHelper.ApplyLanguage(_settingsView);

        // RTL
        FlowDirection = LocalizationService.IsRTL
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }

    private void OnNavClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is not string page) return;

        if (_pageHome != null) _pageHome.IsVisible = false;
        if (_pageProtocols != null) _pageProtocols.IsVisible = false;
        if (_pageDiagnostics != null) _pageDiagnostics.IsVisible = false;
        if (_pageSettings != null) _pageSettings.IsVisible = false;

        switch (page)
        {
            case "home":
                if (_pageHome != null) _pageHome.IsVisible = true;
                break;
            case "protocols":
                if (_pageProtocols != null) _pageProtocols.IsVisible = true;
                break;
            case "diagnostics":
                if (_pageDiagnostics != null) _pageDiagnostics.IsVisible = true;
                break;
            case "settings":
                if (_pageSettings != null) _pageSettings.IsVisible = true;
                break;
        }

        UpdateNavActive(page);
    }

    private void UpdateNavActive(string page)
    {
        SetClass(_navHome, "Active", page == "home");
        SetClass(_navProtocols, "Active", page == "protocols");
        SetClass(_navDiagnostics, "Active", page == "diagnostics");
        SetClass(_navSettings, "Active", page == "settings");
    }

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