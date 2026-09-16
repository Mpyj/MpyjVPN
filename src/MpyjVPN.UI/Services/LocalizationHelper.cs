using MpyjVPN.Application.Services;
using Avalonia.Controls;
using System.Collections.Generic;

namespace MpyjVPN.UI.Services;

public static class LocalizationHelper
{
    // Map: controlName -> localization key
    private static readonly Dictionary<string, string> _map = new()
    {
        // HomeView
        ["HomeTitle"] = "HomeTitle",
        ["HomeSubtitle"] = "HomeSubtitle",
        ["StatusLabel"] = "Status",
        ["StatisticsLabel"] = "Statistics",
        ["DownloadLabel"] = "Download",
        ["UploadLabel"] = "Upload",
        ["LossLabel"] = "Loss",
        ["PingLabel"] = "Ping",
        ["ConnectionModeLabel"] = "ConnectionMode",
        ["AutoLabel"] = "Auto",
        ["AutoDescLabel"] = "AutoDesc",
        ["OnionLabel"] = "Onion",
        ["OnionDescLabel"] = "OnionDesc",
        ["ProtocolsLabel"] = "ProtocolsShort",
        ["ProtocolsDescLabel"] = "ProtocolsDescShort",
        ["UltraLabel"] = "Ultra",
        ["UltraDescLabel"] = "UltraDesc",
        ["ConfigsModeLabel"] = "ConfigsShort",
        ["ConfigsModeDescLabel"] = "ConfigsDesc",
        ["ConfigSelectionLabel"] = "SelectConfig",
        ["ConfigSelectionEmptyText"] = "NoConfigsYet",
        ["ConfigPreviewLabel"] = "Preview",
        ["ConfigHintText"] = "PressPowerHint",
        ["StackLayersLabel"] = "StackLayers",
        ["NoLayersText"] = "NoLayers",
        ["ConfigsLabel"] = "ImportedConfigs",
        ["ConfigsEmptyText"] = "NoConfigsYet",

        // ProtocolsView
        ["ProtocolsTitle"] = "ProtocolsTitle",
        ["ProtocolsSubtitle"] = "ProtocolsSubtitle",
        ["DetailEmptyText"] = "SelectProtocol",
        ["DetailStatusLabel"] = "ProtocolStatus",
        ["DetailPingLabel"] = "ProtocolPing",
        ["DetailTypeLabel"] = "ProtocolType",
        ["DetailPortLabel"] = "ProtocolPort",

        // DiagnosticsView
        ["DiagnosticsTitle"] = "DiagnosticsTitle",
        ["DiagnosticsSubtitle"] = "DiagnosticsSubtitle",
        ["QuickActionsLabel"] = "QuickActions",
        ["DiagnoseLabel"] = "Diagnose",
        ["ScanIPLabel"] = "ScanIP",
        ["LayersLabel"] = "Layers",
        ["CopyLogLabel"] = "CopyLog",
        ["ClearLogLabel"] = "ClearLog",
        ["LogLabel"] = "Log",
        ["IPScannerLabel"] = "IPScanner",

        // SettingsView
        ["SettingsTitle"] = "SettingsTitle",
        ["SettingsSubtitle"] = "SettingsSubtitle",
        ["GeneralLabel"] = "General",
        ["NetworkLabel"] = "Network",
        ["WarpProxyLabel"] = "WarpProxy",
        ["AboutLabel"] = "About",
        ["LanguageLabel"] = "Language",
        ["ThemeLabel"] = "Theme",
        ["AutoReconnectLabel"] = "AutoReconnect",
        ["PrimaryDnsLabel"] = "PrimaryDns",
        ["SecondaryDnsLabel"] = "SecondaryDns",
        ["WorkerUrlLabel"] = "WorkerUrl",
        ["ContactLabel"] = "Contact",
        ["ContactDescription"] = "ContactDescription",
    };

    public static void ApplyLanguage(Control root)
    {
        foreach (var kvp in _map)
        {
            var control = root.FindControl<TextBlock>(kvp.Key);
            if (control != null)
            {
                control.Text = LocalizationService.Get(kvp.Value);
            }
        }

        // ComboBox watermarks
        var search = root.FindControl<TextBox>("ProtocolSearch");
        if (search != null)
            search.Watermark = LocalizationService.Get("SearchPlaceholder");

        var layerPicker = root.FindControl<ComboBox>("LayerPickerCombo");
        if (layerPicker != null)
            layerPicker.PlaceholderText = LocalizationService.Get("SelectLayer");

        // Buttons with Content
        var addLayerBtn = root.FindControl<Button>("AddLayerButton");
        if (addLayerBtn != null)
            addLayerBtn.Content = LocalizationService.Get("AddLayer");

        var importBtn = root.FindControl<Button>("ImportConfigButton");
        if (importBtn != null)
            importBtn.Content = LocalizationService.Get("Import");

        var importInlineBtn = root.FindControl<Button>("ImportConfigInlineButton");
        if (importInlineBtn != null)
            importInlineBtn.Content = LocalizationService.Get("Import");

        var testPingBtn = root.FindControl<Button>("TestPingButton");
        if (testPingBtn != null)
            testPingBtn.Content = LocalizationService.Get("TestPing");
    }
}
