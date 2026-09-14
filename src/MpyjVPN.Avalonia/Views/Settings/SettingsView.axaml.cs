using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MpyjVPN.Application.Services;
using MpyjVPN.Avalonia.Services;
using System;
using System.Diagnostics;

namespace MpyjVPN.Avalonia.Views.Settings;

public partial class SettingsView : UserControl
{
    private AppSettings _settings = new();

    public SettingsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _settings = AppSettings.Load();
        ApplySettings();

        // Language
        var langCombo = this.FindControl<ComboBox>("LanguageCombo");
        if (langCombo != null)
            langCombo.SelectionChanged += OnLanguageChanged;

        // Theme
        var themeCombo = this.FindControl<ComboBox>("ThemeCombo");
        if (themeCombo != null)
            themeCombo.SelectionChanged += OnThemeChanged;

        // Auto-Reconnect
        var autoReconnect = this.FindControl<CheckBox>("AutoReconnectCheck");
        if (autoReconnect != null)
            autoReconnect.IsCheckedChanged += OnAutoReconnectChanged;

        // Primary DNS
        var primaryDns = this.FindControl<TextBox>("PrimaryDnsBox");
        if (primaryDns != null)
            primaryDns.LostFocus += (s, e) =>
            {
                _settings.PrimaryDns = primaryDns.Text ?? "1.1.1.1";
                _settings.Save();
                LogService.Add($"💾 Primary DNS saved: {_settings.PrimaryDns}");
            };

        // Secondary DNS
        var secondaryDns = this.FindControl<TextBox>("SecondaryDnsBox");
        if (secondaryDns != null)
            secondaryDns.LostFocus += (s, e) =>
            {
                _settings.SecondaryDns = secondaryDns.Text ?? "1.0.0.1";
                _settings.Save();
                LogService.Add($"💾 Secondary DNS saved: {_settings.SecondaryDns}");
            };

        // Worker URL
        var workerUrl = this.FindControl<TextBox>("WorkerUrlBox");
        if (workerUrl != null)
            workerUrl.LostFocus += (s, e) =>
            {
                _settings.WorkerUrl = workerUrl.Text ?? "";
                _settings.Save();
                LogService.Add($"💾 Worker URL saved");
            };
    }

    private void ApplySettings()
    {
        var langCombo = this.FindControl<ComboBox>("LanguageCombo");
        if (langCombo != null)
            langCombo.SelectedIndex = _settings.Language == "English" ? 1 : 0;

        var themeCombo = this.FindControl<ComboBox>("ThemeCombo");
        if (themeCombo != null)
            themeCombo.SelectedIndex = _settings.Theme == "Light" ? 1 : 0;

        var autoReconnect = this.FindControl<CheckBox>("AutoReconnectCheck");
        if (autoReconnect != null)
            autoReconnect.IsChecked = _settings.AutoReconnect;

        var primaryDns = this.FindControl<TextBox>("PrimaryDnsBox");
        if (primaryDns != null) primaryDns.Text = _settings.PrimaryDns;

        var secondaryDns = this.FindControl<TextBox>("SecondaryDnsBox");
        if (secondaryDns != null) secondaryDns.Text = _settings.SecondaryDns;

        var workerUrl = this.FindControl<TextBox>("WorkerUrlBox");
        if (workerUrl != null) workerUrl.Text = _settings.WorkerUrl;

        ThemeService.ApplyTheme(_settings.Theme);
    }

    private void OnLanguageChanged(object? sender, SelectionChangedEventArgs e)
    {
        var combo = this.FindControl<ComboBox>("LanguageCombo");
        if (combo?.SelectedItem is ComboBoxItem item)
        {
            _settings.Language = item.Content?.ToString() ?? "فارسی";
            _settings.Save();

            LocalizationService.SetLanguage(_settings.Language);
            LogService.Add($"🌐 Language: {_settings.Language}");
        }
    }

    private void OnThemeChanged(object? sender, SelectionChangedEventArgs e)
    {
        var combo = this.FindControl<ComboBox>("ThemeCombo");
        if (combo?.SelectedItem is ComboBoxItem item)
        {
            _settings.Theme = item.Content?.ToString() ?? "Dark";
            _settings.Save();

            ThemeService.ApplyTheme(_settings.Theme);
            LogService.Add($"🎨 Theme: {_settings.Theme}");
        }
    }

    private void OnAutoReconnectChanged(object? sender, RoutedEventArgs e)
    {
        var check = this.FindControl<CheckBox>("AutoReconnectCheck");
        if (check != null)
        {
            _settings.AutoReconnect = check.IsChecked ?? true;
            _settings.Save();
        }
    }

    // ==================== CONTACT LINKS ====================

    private void OnTwitterClick(object? sender, RoutedEventArgs e)
    {
        OpenUrl("https://x.com/Mpyj_X");
    }

    private void OnGithubClick(object? sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/Mpyj");
    }

    private void OnTelegramClick(object? sender, RoutedEventArgs e)
    {
        OpenUrl("https://t.me/MpyjTelegram");
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
            LogService.Add($"🔗 Opening: {url}");
        }
        catch (Exception ex)
        {
            LogService.Add($"❌ Failed to open URL: {ex.Message}");
        }
    }
}
