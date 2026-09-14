using System.Collections.Generic;

namespace MpyjVPN.Application.Services;

public static class LocalizationService
{
    public static string CurrentLanguage { get; private set; } = "فارسی";

    public static bool IsRTL => CurrentLanguage == "فارسی";

    public static event Action? LanguageChanged;

    private static readonly Dictionary<string, (string fa, string en)> _strings = new()
    {
        // ==================== SIDEBAR ====================
        ["NavHome"] = ("🏠  خانه", "🏠  Home"),
        ["NavProtocols"] = ("🔓  پروتکل‌ها", "🔓  Protocols"),
        ["NavDiagnostics"] = ("📊  عیب‌یابی", "📊  Diagnostics"),
        ["NavSettings"] = ("⚙️  تنظیمات", "⚙️  Settings"),
        ["SidebarDisconnected"] = ("قطع", "Disconnected"),
        ["SidebarConnected"] = ("متصل", "Connected"),

        // ==================== HOME ====================
        ["HomeTitle"] = ("اتصال", "Connection"),
        ["HomeSubtitle"] = ("اتصال VPN خود را مدیریت کنید", "Manage your VPN connection"),
        ["Status"] = ("وضعیت", "STATUS"),
        ["NotConnected"] = ("قطع", "Not Connected"),
        ["Protected"] = ("محافظت‌شده", "Protected"),
        ["Connecting"] = ("در حال اتصال...", "Connecting..."),
        ["Failed"] = ("ناموفق", "Failed"),
        ["ReadyToConnect"] = ("آماده اتصال", "Ready to connect"),

        // ==================== STATS ====================
        ["Statistics"] = ("آمار", "STATISTICS"),
        ["Download"] = ("دانلود", "DOWNLOAD"),
        ["Upload"] = ("آپلود", "UPLOAD"),
        ["Loss"] = ("افت بسته", "LOSS"),
        ["Ping"] = ("پینگ", "PING"),

        // ==================== MODES ====================
        ["ConnectionMode"] = ("حالت اتصال", "CONNECTION MODE"),
        ["Auto"] = ("خودکار", "AUTO"),
        ["AutoDesc"] = ("بهترین", "Best"),
        ["Onion"] = ("پیازی", "ONION"),
        ["OnionDesc"] = ("ضد فیلتر", "Filter"),
        ["ProtocolsShort"] = ("پروتکل", "PROTO"),
        ["ProtocolsDescShort"] = ("دستی", "Manual"),
        ["Ultra"] = ("اولترا", "ULTRA"),
        ["UltraDesc"] = ("چندگانه", "Multi"),
        ["ConfigsShort"] = ("کانفیگ", "CONFIG"),
        ["ConfigsDesc"] = ("سفارشی", "Custom"),

        // ==================== CONFIG SELECTION ====================
        ["SelectConfig"] = ("📄 انتخاب کانفیگ", "📄 SELECT A CONFIG"),
        ["PressPowerHint"] = ("💡 دکمه Power رو بزن تا وصل بشی", "💡 Press Power button to connect"),

        // ==================== STACK LAYERS ====================
        ["StackLayers"] = ("🔗 لایه‌های چیدمان (اختیاری)", "🔗 STACK LAYERS (Optional)"),
        ["SelectLayer"] = ("لایه انتخاب کن...", "Select a layer..."),
        ["AddLayer"] = ("+ افزودن", "+ Add"),
        ["NoLayers"] = ("لایه‌ای اضافه نشده. کانفیگ مستقیم وصل می‌شه.", "No layers added. Config will connect directly."),

        // ==================== IMPORTED CONFIGS ====================
        ["ImportedConfigs"] = ("📄 کانفیگ‌های وارد‌شده", "📄 IMPORTED CONFIGS"),
        ["NoConfigsYet"] = ("هنوز کانفیگی اضافه نشده. Import بزن.", "No configs yet. Click Import to add."),
        ["Import"] = ("+ وارد کردن", "+ Import"),

        // ==================== PROTOCOLS PAGE ====================
        ["ProtocolsTitle"] = ("پروتکل‌ها", "Protocols"),
        ["ProtocolsSubtitle"] = ("مدیریت پروتکل‌های فعال", "Manage active protocols"),
        ["SearchPlaceholder"] = ("🔍 جستجوی پروتکل...", "🔍 Search protocols..."),
        ["Refresh"] = ("🔄 بروزرسانی", "🔄 Refresh"),

        // ==================== PROTOCOL DETAIL ====================
        ["SelectProtocol"] = ("یک پروتکل انتخاب کنید", "Select a protocol"),
        ["ProtocolAbout"] = ("درباره", "About"),
        ["ProtocolStatus"] = ("وضعیت:", "Status:"),
        ["ProtocolPing"] = ("پینگ:", "Ping:"),
        ["ProtocolType"] = ("نوع:", "Type:"),
        ["ProtocolPort"] = ("پورت:", "Port:"),
        ["ProtocolLayers"] = ("لایه‌ها:", "Layers:"),
        ["TestPing"] = ("🧪 تست پینگ", "🧪 Test Ping"),
        ["ToggleProtocol"] = ("🔄 تغییر وضعیت", "🔄 Toggle"),

        // ==================== DIAGNOSTICS ====================
        ["DiagnosticsTitle"] = ("عیب‌یابی", "Diagnostics"),
        ["DiagnosticsSubtitle"] = ("اطلاعات سیستم و عملکرد", "System information and performance"),
        ["QuickActions"] = ("عملیات سریع", "QUICK ACTIONS"),
        ["Diagnose"] = ("عیب‌یابی", "Diagnose"),
        ["ScanIP"] = ("اسکن IP", "Scan IP"),
        ["Layers"] = ("لایه‌ها", "Layers"),
        ["CopyLog"] = ("کپی لاگ", "Copy Log"),
        ["ClearLog"] = ("پاک کردن لاگ", "Clear Log"),
        ["Log"] = ("لاگ", "LOG"),
        ["IPScanner"] = ("🌐 IPهای اسکن‌شده", "🌐 SCANNED IPs"),

        // ==================== SETTINGS ====================
        ["SettingsTitle"] = ("تنظیمات", "Settings"),
        ["SettingsSubtitle"] = ("پیکربندی VPN", "Configure your VPN"),
        ["General"] = ("🌐 عمومی", "🌐 GENERAL"),
        ["Network"] = ("🔌 شبکه", "🔌 NETWORK"),
        ["WarpProxy"] = ("☁️ پروکسی WARP", "☁️ WARP PROXY"),
        ["About"] = ("ℹ️ درباره", "ℹ️ ABOUT"),
        ["Language"] = ("زبان", "Language"),
        ["Theme"] = ("تم", "Theme"),
        ["AutoReconnect"] = ("اتصال مجدد خودکار", "Auto-Reconnect"),
        ["PrimaryDns"] = ("DNS اصلی", "Primary DNS"),
        ["SecondaryDns"] = ("DNS ثانویه", "Secondary DNS"),
        ["WorkerUrl"] = ("آدرس Worker", "Worker URL"),
        ["Version"] = ("نسخه", "Version"),
        ["CreatedBy"] = ("ساخته شده توسط Mpyj", "Created by Mpyj"),

        // ==================== TOAST/LOG ====================
        ["Connected"] = ("متصل شد", "Connected"),
        ["Disconnected"] = ("قطع شد", "Disconnected"),
        ["ConnectionFailed"] = ("اتصال ناموفق", "Connection failed"),
        ["Mode"] = ("حالت", "Mode"),
        ["Ready"] = ("آماده", "Ready"),
        ["DisconnectFirst"] = ("⚠️ اول قطع کنید", "⚠️ Disconnect first"),

        // ==================== COPY LOG ====================
        ["LogEmpty"] = ("لاگ خالیه", "Log is empty"),
        ["LogCopied"] = ("لاگ کپی شد", "Log copied to clipboard"),
        ["ClipboardNotAvailable"] = ("کلیپ‌بورد در دسترس نیست", "Clipboard not available"),
        ["CopyFailed"] = ("کپی ناموفق", "Copy failed"),
        // ==================== CONTACT ====================
        ["Contact"] = ("📢 ارتباط با ما", "📢 CONTACT US"),
        ["ContactDescription"] = ("جهت انتقاد، پیشنهاد، همکاری و یا پروژه‌های بیشتر به صفحات مجازی ما مراجعه کنید.", "For feedback, suggestions, collaboration or more projects, please visit our social media."),
    };

    public static void SetLanguage(string language)
    {
        CurrentLanguage = language;
        LanguageChanged?.Invoke();
    }

    public static string Get(string key)
    {
        if (!_strings.TryGetValue(key, out var val))
            return key;

        return IsRTL ? val.fa : val.en;
    }
}