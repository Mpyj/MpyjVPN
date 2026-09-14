using MpyjVPN.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MpyjVPN.Application.Services;

public static class ConfigParserService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public class ParseResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public List<ProtocolViewModel> Protocols { get; set; } = new();
    }

    public static async Task<ParseResult> ParseAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new ParseResult { Success = false, ErrorMessage = "Input is empty" };

        input = input.Trim();

        // Subscription URL
        if (input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return await ParseSubscriptionAsync(input);
        }

        // Single config
        var single = ParseSingle(input);
        if (single != null)
        {
            return new ParseResult
            {
                Success = true,
                Protocols = new List<ProtocolViewModel> { single }
            };
        }

        return new ParseResult
        {
            Success = false,
            ErrorMessage = "Unsupported format. Use vmess://, vless://, trojan://, ss://, JSON, or a subscription URL."
        };
    }

    // ==================== SUBSCRIPTION ====================
    private static async Task<ParseResult> ParseSubscriptionAsync(string url)
    {
        try
        {
            var content = await _http.GetStringAsync(url);
            content = content.Trim();

            string decoded;
            try
            {
                decoded = DecodeBase64(content);
                if (string.IsNullOrWhiteSpace(decoded) || !decoded.Contains("://"))
                    decoded = content;
            }
            catch
            {
                decoded = content;
            }

            var protocols = new List<ProtocolViewModel>();

            foreach (var line in decoded.Split('\n'))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                var protocol = ParseSingle(trimmed);
                if (protocol != null)
                    protocols.Add(protocol);
            }

            if (protocols.Count == 0)
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = "No valid configs found in subscription"
                };

            return new ParseResult
            {
                Success = true,
                Protocols = protocols
            };
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                Success = false,
                ErrorMessage = $"Subscription download failed: {ex.Message}"
            };
        }
    }

    // ==================== SINGLE PARSE ====================
    private static ProtocolViewModel? ParseSingle(string input)
    {
        try
        {
            if (input.StartsWith("vmess://", StringComparison.OrdinalIgnoreCase))
                return ParseVMess(input);

            if (input.StartsWith("vless://", StringComparison.OrdinalIgnoreCase))
                return ParseVLess(input);

            if (input.StartsWith("trojan://", StringComparison.OrdinalIgnoreCase))
                return ParseTrojan(input);

            if (input.StartsWith("ss://", StringComparison.OrdinalIgnoreCase))
                return ParseShadowsocks(input);

            if (input.StartsWith("{"))
                return ParseJson(input);

            return null;
        }
        catch
        {
            return null;
        }
    }

    // ==================== VMESS ====================
    private static ProtocolViewModel? ParseVMess(string input)
    {
        var base64 = input.Substring("vmess://".Length);
        var json = DecodeBase64(base64);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var name = GetString(root, "ps", "VMess Config");
        var server = GetString(root, "add", "");
        var port = GetString(root, "port", "443");

        if (string.IsNullOrEmpty(server)) return null;

        return new ProtocolViewModel
        {
            Name = name,
            Icon = "ðŸŒ",
            Description = $"VMess â€¢ {server}",
            LongDescription = $"VMess config imported. Server: {server}:{port}",
            Type = "Custom",
            Port = int.TryParse(port, out var p) ? p : 443,
            Layers = "VMess",
            ServerAddress = server,
            IsEnabled = false,
            Ping = "--",
            RawConfig = input
        };
    }

    // ==================== VLESS ====================
    private static ProtocolViewModel? ParseVLess(string input)
    {
        var match = Regex.Match(input, @"vless://([^@]+)@([^:?#]+):(\d+)([^#]*)?(#(.+))?");
        if (!match.Success) return null;

        var server = match.Groups[2].Value;
        var port = int.TryParse(match.Groups[3].Value, out var p) ? p : 443;
        var name = match.Groups[6].Success
            ? Uri.UnescapeDataString(match.Groups[6].Value)
            : "VLess Config";

        return new ProtocolViewModel
        {
            Name = name,
            Icon = "ðŸ”",
            Description = $"VLess â€¢ {server}",
            LongDescription = $"VLess config imported. Server: {server}:{port}",
            Type = "Custom",
            Port = port,
            Layers = "VLess + TLS",
            ServerAddress = server,
            IsEnabled = false,
            Ping = "--",
            RawConfig = input
        };
    }

    // ==================== TROJAN ====================
    private static ProtocolViewModel? ParseTrojan(string input)
    {
        var match = Regex.Match(input, @"trojan://([^@]+)@([^:?#]+):(\d+)([^#]*)?(#(.+))?");
        if (!match.Success) return null;

        var server = match.Groups[2].Value;
        var port = int.TryParse(match.Groups[3].Value, out var p) ? p : 443;
        var name = match.Groups[6].Success
            ? Uri.UnescapeDataString(match.Groups[6].Value)
            : "Trojan Config";

        return new ProtocolViewModel
        {
            Name = name,
            Icon = "ðŸŽ­",
            Description = $"Trojan â€¢ {server}",
            LongDescription = $"Trojan config imported. Server: {server}:{port}",
            Type = "Custom",
            Port = port,
            Layers = "Trojan + TLS",
            ServerAddress = server,
            IsEnabled = false,
            Ping = "--",
            RawConfig = input
        };
    }

    // ==================== SHADOWSOCKS ====================
    private static ProtocolViewModel? ParseShadowsocks(string input)
    {
        var content = input.Substring("ss://".Length);

        string name = "SS Config";
        var hashIndex = content.IndexOf('#');
        if (hashIndex > 0)
        {
            name = Uri.UnescapeDataString(content.Substring(hashIndex + 1));
            content = content.Substring(0, hashIndex);
        }

        var atIndex = content.LastIndexOf('@');
        string server = "";
        int port = 443;

        if (atIndex > 0)
        {
            var serverPart = content.Substring(atIndex + 1);
            var colonIndex = serverPart.LastIndexOf(':');
            if (colonIndex > 0)
            {
                server = serverPart.Substring(0, colonIndex);
                int.TryParse(serverPart.Substring(colonIndex + 1), out port);
            }
        }
        else
        {
            try
            {
                var decoded = DecodeBase64(content);
                var parts = decoded.Split('@');
                if (parts.Length == 2)
                {
                    var serverPart = parts[1];
                    var colonIndex = serverPart.LastIndexOf(':');
                    if (colonIndex > 0)
                    {
                        server = serverPart.Substring(0, colonIndex);
                        int.TryParse(serverPart.Substring(colonIndex + 1), out port);
                    }
                }
            }
            catch { }
        }

        if (string.IsNullOrEmpty(server)) return null;

        return new ProtocolViewModel
        {
            Name = name,
            Icon = "ðŸ”’",
            Description = $"Shadowsocks â€¢ {server}",
            LongDescription = $"Shadowsocks config imported. Server: {server}:{port}",
            Type = "Custom",
            Port = port,
            Layers = "Shadowsocks",
            ServerAddress = server,
            IsEnabled = false,
            Ping = "--",
            RawConfig = input
        };
    }

    // ==================== JSON ====================
    private static ProtocolViewModel? ParseJson(string input)
    {
        using var doc = JsonDocument.Parse(input);
        var root = doc.RootElement;

        var name = GetString(root, "name", GetString(root, "ps", "Custom Config"));
        var server = GetString(root, "add", GetString(root, "server", ""));
        var port = GetString(root, "port", "443");

        if (string.IsNullOrEmpty(server)) return null;

        return new ProtocolViewModel
        {
            Name = name,
            Icon = "ðŸ“„",
            Description = $"JSON â€¢ {server}",
            LongDescription = $"Config imported from JSON. Server: {server}:{port}",
            Type = "Custom",
            Port = int.TryParse(port, out var p) ? p : 443,
            Layers = "JSON",
            ServerAddress = server,
            IsEnabled = false,
            Ping = "--",
            RawConfig = input
        };
    }

    // ==================== HELPERS ====================
    private static string GetString(JsonElement element, string prop, string defaultValue)
    {
        if (element.TryGetProperty(prop, out var val))
        {
            if (val.ValueKind == JsonValueKind.String)
                return val.GetString() ?? defaultValue;
            return val.ToString();
        }
        return defaultValue;
    }

    private static string DecodeBase64(string input)
    {
        input = input.Trim().Replace('-', '+').Replace('_', '/');
        switch (input.Length % 4)
        {
            case 2: input += "=="; break;
            case 3: input += "="; break;
        }

        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }
}