using MpyjVPN.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MpyjVPN.Application.Services;

public class StoredConfig
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    public string LongDescription { get; set; } = "";
    public string Type { get; set; } = "Custom";
    public int Port { get; set; } = 443;
    public string Layers { get; set; } = "";
    public string ServerAddress { get; set; } = "";
    public bool IsEnabled { get; set; } = false;
    public string RawConfig { get; set; } = "";
}

public static class ConfigStorageService
{
    private static readonly string ConfigsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MpyjVPN", "configs.json");

    public static List<StoredConfig> Load()
    {
        try
        {
            if (File.Exists(ConfigsPath))
            {
                var json = File.ReadAllText(ConfigsPath);
                return JsonSerializer.Deserialize<List<StoredConfig>>(json) 
                    ?? new List<StoredConfig>();
            }
        }
        catch { }
        return new List<StoredConfig>();
    }

    public static void Save(List<StoredConfig> configs)
    {
        try
        {
            var dir = Path.GetDirectoryName(ConfigsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(ConfigsPath, json);
        }
        catch { }
    }

    public static void SaveFromViewModels(IEnumerable<ProtocolViewModel> protocols)
    {
        var stored = new List<StoredConfig>();

        foreach (var p in protocols)
        {
            stored.Add(new StoredConfig
            {
                Name = p.Name,
                Icon = p.Icon,
                Description = p.Description,
                LongDescription = p.LongDescription,
                Type = p.Type,
                Port = p.Port,
                Layers = p.Layers,
                ServerAddress = p.ServerAddress,
                IsEnabled = p.IsEnabled,
                RawConfig = p.RawConfig
            });
        }

        Save(stored);
    }

    public static List<ProtocolViewModel> LoadAsViewModels()
    {
        var result = new List<ProtocolViewModel>();
        var stored = Load();

        foreach (var s in stored)
        {
            result.Add(new ProtocolViewModel
            {
                Name = s.Name,
                Icon = s.Icon,
                Description = s.Description,
                LongDescription = s.LongDescription,
                Type = s.Type,
                Port = s.Port,
                Layers = s.Layers,
                ServerAddress = s.ServerAddress,
                IsEnabled = s.IsEnabled,
                RawConfig = s.RawConfig,
                Ping = "--"
            });
        }

        return result;
    }
}