using System;
using System.IO;
using System.Text.Json;

namespace MpyjVPN.Application.Services;

public class AppSettings
{
    public string Language { get; set; } = "ÙØ§Ø±Ø³ÛŒ";
    public string Theme { get; set; } = "Dark";
    public bool AutoReconnect { get; set; } = true;
    public string PrimaryDns { get; set; } = "1.1.1.1";
    public string SecondaryDns { get; set; } = "1.0.0.1";
    public string WorkerUrl { get; set; } = "https://shy-glade-59ba.urihghioihrerg.workers.dev";

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MpyjVPN", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }
}