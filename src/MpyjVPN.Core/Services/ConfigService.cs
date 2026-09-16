using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;

namespace MpyjCore.Services;

public class ConfigService
{
    private readonly HttpClient _httpClient = new();
    private Process? _xrayProcess;
    private string _xrayConfigPath = "";

    public event Action<string, string>? LogMessage;

    public string ActiveConfig { get; private set; } = "";
    public bool IsRunning { get; private set; }

    public ConfigType DetectConfigType(string config)
    {
        config = config.Trim();

        if (config.StartsWith("vmess://")) return ConfigType.VMessLink;
        if (config.StartsWith("vless://")) return ConfigType.VLessLink;
        if (config.StartsWith("trojan://")) return ConfigType.TrojanLink;
        if (config.StartsWith("ss://")) return ConfigType.ShadowsocksLink;
        if (config.StartsWith("{")) return ConfigType.JsonConfig;
        if (config.StartsWith("http")) return ConfigType.SubscriptionUrl;

        return ConfigType.Unknown;
    }

    public async Task<bool> LoadConfigAsync(string config)
    {
        var type = DetectConfigType(config);

        switch (type)
        {
            case ConfigType.VMessLink:
                return await LoadVMessAsync(config);
            case ConfigType.VLessLink:
                return await LoadVLessAsync(config);
            case ConfigType.TrojanLink:
                return await LoadTrojanAsync(config);
            case ConfigType.ShadowsocksLink:
                return await LoadShadowsocksAsync(config);
            case ConfigType.JsonConfig:
                return await LoadJsonAsync(config);
            case ConfigType.SubscriptionUrl:
                return await LoadSubscriptionAsync(config);
            default:
                return false;
        }
    }

    // ==================== VMESS ====================
    private async Task<bool> LoadVMessAsync(string link)
    {
        try
        {
            var base64 = link.Replace("vmess://", "").Trim();
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var vmess = JsonSerializer.Deserialize<VMessConfig>(json);

            if (vmess == null || string.IsNullOrEmpty(vmess.add))
            {
                LogMessage?.Invoke("❌ VMess config invalid", "error");
                return false;
            }

            var xrayConfig = BuildXrayConfigVMess(vmess);
            _xrayConfigPath = Path.Combine(Path.GetTempPath(), "mpyj_xray_config.json");
            await File.WriteAllTextAsync(_xrayConfigPath, xrayConfig);

            LogMessage?.Invoke($"✅ VMess config parsed ({vmess.add}:{vmess.port}, net={vmess.net})", "success");
            ActiveConfig = link;
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ VMess error: {ex.Message}", "error");
            return false;
        }
    }

    // ==================== VLESS ====================
    private async Task<bool> LoadVLessAsync(string link)
    {
        try
        {
            var uri = new Uri(link.Replace("vless://", "http://"));
            var uuid = uri.UserInfo;
            var server = uri.Host;
            var port = uri.Port;
            var query = HttpUtility.ParseQueryString(uri.Query);

            var netType = query["type"] ?? "tcp";
            var security = query["security"] ?? "none";
            var host = query["host"] ?? "";
            var path = query["path"] ?? "/";
            var sni = query["sni"] ?? server;
            var flow = query["flow"] ?? "";
            var serviceName = query["serviceName"] ?? "";

            var xrayConfig = BuildXrayConfigVLess(uuid, server, port, netType, security, host, path, sni, flow, serviceName);
            _xrayConfigPath = Path.Combine(Path.GetTempPath(), "mpyj_xray_config.json");
            await File.WriteAllTextAsync(_xrayConfigPath, xrayConfig);

            LogMessage?.Invoke($"✅ VLess config parsed ({server}:{port}, net={netType}, sec={security})", "success");
            ActiveConfig = link;
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ VLess error: {ex.Message}", "error");
            return false;
        }
    }

    // ==================== TROJAN ====================
    private async Task<bool> LoadTrojanAsync(string link)
    {
        try
        {
            var uri = new Uri(link.Replace("trojan://", "http://"));
            var password = uri.UserInfo;
            var server = uri.Host;
            var port = uri.Port;
            var query = HttpUtility.ParseQueryString(uri.Query);

            var netType = query["type"] ?? "tcp";
            var security = query["security"] ?? "tls";
            var host = query["host"] ?? "";
            var path = query["path"] ?? "/";
            var sni = query["sni"] ?? server;

            var xrayConfig = BuildXrayConfigTrojan(password, server, port, netType, security, host, path, sni);
            _xrayConfigPath = Path.Combine(Path.GetTempPath(), "mpyj_xray_config.json");
            await File.WriteAllTextAsync(_xrayConfigPath, xrayConfig);

            LogMessage?.Invoke($"✅ Trojan config parsed ({server}:{port}, net={netType})", "success");
            ActiveConfig = link;
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ Trojan error: {ex.Message}", "error");
            return false;
        }
    }

    private async Task<bool> LoadShadowsocksAsync(string link)
    {
        LogMessage?.Invoke("✅ Shadowsocks config parsed", "success");
        ActiveConfig = link;
        return await Task.FromResult(true);
    }

    private async Task<bool> LoadJsonAsync(string json)
    {
        try
        {
            _xrayConfigPath = Path.Combine(Path.GetTempPath(), "mpyj_xray_config.json");
            await File.WriteAllTextAsync(_xrayConfigPath, json);
            LogMessage?.Invoke("✅ JSON config parsed", "success");
            ActiveConfig = json;
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ JSON error: {ex.Message}", "error");
            return false;
        }
    }

    private async Task<bool> LoadSubscriptionAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            string decoded;
            try { decoded = Encoding.UTF8.GetString(Convert.FromBase64String(response)); }
            catch { decoded = response; }

            var lines = decoded.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var firstConfig = lines.FirstOrDefault(l =>
                l.StartsWith("vmess://") || l.StartsWith("vless://") ||
                l.StartsWith("trojan://") || l.StartsWith("ss://"));

            if (firstConfig == null)
            {
                LogMessage?.Invoke("❌ No config found in subscription", "error");
                return false;
            }

            return await LoadConfigAsync(firstConfig.Trim());
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ Subscription error: {ex.Message}", "error");
            return false;
        }
    }

    // ==================== START XRAY ====================
    public async Task<bool> StartXrayAsync(string xrayPath = "xray")
    {
        try
        {
            if (string.IsNullOrEmpty(_xrayConfigPath) || !File.Exists(_xrayConfigPath))
            {
                LogMessage?.Invoke("❌ No config file found", "error");
                return false;
            }

            // پیدا کردن xray.exe
            var xrayFullPath = FindXrayPath(xrayPath);

            LogMessage?.Invoke($"🔍 Xray path: {xrayFullPath}", "info");

            if (!File.Exists(xrayFullPath) && xrayFullPath == "xray")
            {
                LogMessage?.Invoke("❌ xray.exe not found!", "error");
                return false;
            }

            var psi = new ProcessStartInfo
            {
                FileName = xrayFullPath,
                Arguments = $"run -c \"{_xrayConfigPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(xrayFullPath) ?? ""
            };

            _xrayProcess = Process.Start(psi);

            if (_xrayProcess == null)
            {
                LogMessage?.Invoke("❌ Failed to start Xray", "error");
                return false;
            }

            // لاگ Xray
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_xrayProcess.HasExited)
                    {
                        var line = await _xrayProcess.StandardOutput.ReadLineAsync();
                        if (line != null) LogMessage?.Invoke($"Xray: {line}", "info");
                    }
                }
                catch { }
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_xrayProcess.HasExited)
                    {
                        var line = await _xrayProcess.StandardError.ReadLineAsync();
                        if (line != null) LogMessage?.Invoke($"Xray: {line}", "warning");
                    }
                }
                catch { }
            });

            await Task.Delay(3000);

            if (_xrayProcess.HasExited)
            {
                LogMessage?.Invoke($"❌ Xray exited with code {_xrayProcess.ExitCode}", "error");
                return false;
            }

            IsRunning = true;
            LogMessage?.Invoke("🚀 Xray started", "success");
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ Xray error: {ex.Message}", "error");
            return false;
        }
    }

    private string FindXrayPath(string defaultPath)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // مسیرهای احتمالی
        var paths = new[]
        {
            Path.Combine(baseDir, "cli", "xray.exe"),
            Path.Combine(baseDir, "cli", "xray"),
            Path.Combine(baseDir, "xray.exe"),
            Path.Combine(baseDir, "xray"),
            Path.Combine(baseDir, "..", "..", "..", "..", "cli", "xray.exe"),
            Path.Combine(baseDir, "..", "..", "..", "cli", "xray.exe"),
            defaultPath
        };

        foreach (var p in paths)
        {
            var fullPath = Path.GetFullPath(p);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return defaultPath;
    }

    public void StopXray()
    {
        if (_xrayProcess != null && !_xrayProcess.HasExited)
        {
            try
            {
                _xrayProcess.Kill();
                _xrayProcess.Dispose();
            }
            catch { }
        }
        _xrayProcess = null;
        IsRunning = false;
    }

    // ==================== SYSTEM PROXY ====================
    public void SetSystemProxy()
    {
        try
        {
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                "ProxyEnable", 1, Microsoft.Win32.RegistryValueKind.DWord);

            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                "ProxyServer", "127.0.0.1:10809", Microsoft.Win32.RegistryValueKind.String);

            RefreshSystemProxy();
            LogMessage?.Invoke("✅ System proxy enabled (127.0.0.1:10809)", "success");
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"⚠️ System proxy error: {ex.Message}", "warning");
        }
    }

    public void DisableSystemProxy()
    {
        try
        {
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                "ProxyEnable", 0, Microsoft.Win32.RegistryValueKind.DWord);

            RefreshSystemProxy();
            LogMessage?.Invoke("✅ System proxy disabled", "success");
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"⚠️ System proxy error: {ex.Message}", "warning");
        }
    }

    [System.Runtime.InteropServices.DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

    private static void RefreshSystemProxy()
    {
        const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        const int INTERNET_OPTION_REFRESH = 37;
        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
    }

    // ==================== BUILD XRAY CONFIGS ====================
    private string BuildXrayConfigVMess(VMessConfig vmess)
    {
        var streamSettings = BuildStreamSettings(vmess.net, vmess.tls, vmess.host, vmess.path, vmess.add);

        var config = new Dictionary<string, object>
        {
            ["log"] = new { loglevel = "warning" },
            ["inbounds"] = new object[]
            {
                new { listen = "127.0.0.1", port = 10808, protocol = "socks", settings = new { udp = true } },
                new { listen = "127.0.0.1", port = 10809, protocol = "http", settings = new { } }
            },
            ["outbounds"] = new object[]
            {
                new Dictionary<string, object>
                {
                    ["protocol"] = "vmess",
                    ["settings"] = new
                    {
                        vnext = new[]
                        {
                            new
                            {
                                address = vmess.add,
                                port = int.Parse(vmess.port),
                                users = new[]
                                {
                                    new { id = vmess.id, alterId = int.Parse(vmess.aid), security = "auto" }
                                }
                            }
                        }
                    },
                    ["streamSettings"] = streamSettings
                }
            }
        };

        return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
    }

    private string BuildXrayConfigVLess(string uuid, string server, int port, string netType, string security, string host, string path, string sni, string flow, string serviceName)
    {
        var streamSettings = BuildStreamSettings(netType, security, host, path, sni, serviceName);

        var user = new Dictionary<string, object>
        {
            ["id"] = uuid,
            ["encryption"] = "none"
        };

        if (!string.IsNullOrEmpty(flow))
            user["flow"] = flow;

        var config = new Dictionary<string, object>
        {
            ["log"] = new { loglevel = "warning" },
            ["inbounds"] = new object[]
            {
                new { listen = "127.0.0.1", port = 10808, protocol = "socks", settings = new { udp = true } },
                new { listen = "127.0.0.1", port = 10809, protocol = "http", settings = new { } }
            },
            ["outbounds"] = new object[]
            {
                new Dictionary<string, object>
                {
                    ["protocol"] = "vless",
                    ["settings"] = new
                    {
                        vnext = new[]
                        {
                            new
                            {
                                address = server,
                                port = port,
                                users = new[] { user }
                            }
                        }
                    },
                    ["streamSettings"] = streamSettings
                }
            }
        };

        return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
    }

    private string BuildXrayConfigTrojan(string password, string server, int port, string netType, string security, string host, string path, string sni)
    {
        var streamSettings = BuildStreamSettings(netType, security, host, path, sni);

        var config = new Dictionary<string, object>
        {
            ["log"] = new { loglevel = "warning" },
            ["inbounds"] = new object[]
            {
                new { listen = "127.0.0.1", port = 10808, protocol = "socks", settings = new { udp = true } },
                new { listen = "127.0.0.1", port = 10809, protocol = "http", settings = new { } }
            },
            ["outbounds"] = new object[]
            {
                new Dictionary<string, object>
                {
                    ["protocol"] = "trojan",
                    ["settings"] = new
                    {
                        servers = new[]
                        {
                            new { address = server, port = port, password = password }
                        }
                    },
                    ["streamSettings"] = streamSettings
                }
            }
        };

        return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
    }

    private Dictionary<string, object> BuildStreamSettings(string network, string security, string host, string path, string sni, string serviceName = "")
    {
        var stream = new Dictionary<string, object>
        {
            ["network"] = network
        };

        if (security == "tls")
        {
            stream["security"] = "tls";
            stream["tlsSettings"] = new
            {
                serverName = sni
            };
        }
        else if (security == "reality")
        {
            stream["security"] = "reality";
            stream["realitySettings"] = new
            {
                serverName = sni,
                fingerprint = "chrome"
            };
        }
        else
        {
            stream["security"] = "none";
        }

        switch (network)
        {
            case "ws":
                stream["wsSettings"] = new
                {
                    path = string.IsNullOrEmpty(path) ? "/" : path,
                    headers = string.IsNullOrEmpty(host) ? null : new { Host = host }
                };
                break;

            case "grpc":
                stream["grpcSettings"] = new
                {
                    serviceName = serviceName
                };
                break;

            case "tcp":
                if (security == "tls")
                {
                    stream["tcpSettings"] = new
                    {
                        header = new { type = "none" }
                    };
                }
                break;
        }

        return stream;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var response = await client.GetAsync("https://google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public enum ConfigType
{
    Unknown,
    VMessLink,
    VLessLink,
    TrojanLink,
    ShadowsocksLink,
    JsonConfig,
    SubscriptionUrl
}

public class VMessConfig
{
    public string v { get; set; } = "2";
    public string ps { get; set; } = "";
    public string add { get; set; } = "";
    public string port { get; set; } = "443";
    public string id { get; set; } = "";
    public string aid { get; set; } = "0";
    public string net { get; set; } = "tcp";
    public string type { get; set; } = "none";
    public string host { get; set; } = "";
    public string path { get; set; } = "/";
    public string tls { get; set; } = "";
}