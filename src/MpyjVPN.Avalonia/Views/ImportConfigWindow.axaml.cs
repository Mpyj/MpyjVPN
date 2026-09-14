using Avalonia.Controls;
using Avalonia.Interactivity;
using MpyjVPN.Application.Services;
using MpyjVPN.Application.ViewModels;
using System.Collections.Generic;

namespace MpyjVPN.Avalonia.Views;

public partial class ImportConfigWindow : Window
{
    public List<ProtocolViewModel> ImportedProtocols { get; private set; } = new();

    public ImportConfigWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        var configInput = this.FindControl<TextBox>("ConfigInput");
        var nameInput = this.FindControl<TextBox>("NameInput");
        var errorBox = this.FindControl<Border>("ErrorBox");
        var errorText = this.FindControl<TextBlock>("ErrorText");
        var loadingBox = this.FindControl<Border>("LoadingBox");
        var loadingText = this.FindControl<TextBlock>("LoadingText");
        var importBtn = this.FindControl<Button>("ImportButton");

        if (configInput == null) return;

        var config = configInput.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(config))
        {
            ShowError("Please paste a config first");
            return;
        }

        // Reset UI
        if (errorBox != null) errorBox.IsVisible = false;
        if (loadingBox != null) loadingBox.IsVisible = true;
        if (loadingText != null) loadingText.Text = "Processing...";
        if (importBtn != null) importBtn.IsEnabled = false;

        try
        {
            var result = await ConfigParserService.ParseAsync(config);

            if (!result.Success || result.Protocols.Count == 0)
            {
                if (loadingBox != null) loadingBox.IsVisible = false;
                if (importBtn != null) importBtn.IsEnabled = true;
                ShowError(result.ErrorMessage);
                return;
            }

            // Name prefix
            var prefix = nameInput?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                for (int i = 0; i < result.Protocols.Count; i++)
                {
                    result.Protocols[i].Name = result.Protocols.Count > 1
                        ? $"{prefix} #{i + 1}"
                        : prefix;
                }
            }

            ImportedProtocols = result.Protocols;
            Close(ImportedProtocols);
        }
        catch (System.Exception ex)
        {
            if (loadingBox != null) loadingBox.IsVisible = false;
            if (importBtn != null) importBtn.IsEnabled = true;
            ShowError($"Error: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        var errorBox = this.FindControl<Border>("ErrorBox");
        var errorText = this.FindControl<TextBlock>("ErrorText");

        if (errorBox != null) errorBox.IsVisible = true;
        if (errorText != null) errorText.Text = message;
    }
}