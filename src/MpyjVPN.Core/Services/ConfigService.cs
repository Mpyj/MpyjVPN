using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MpyjCore.Services;

public class ConfigService
{
    private readonly HttpClient _httpClient = new();
    private Process? _xrayProcess;
    
    public event Action<string, string>? LogMessage;
    
    public string ActiveConfig { get; private set; } = "";
    public bool IsRunning { get; private set; }
    
    public ConfigType DetectConfigType(string config)
    {
        config = config.Trim();
        
        if (config.StartsWith("vmess://"))
            return ConfigType.VMessLink;
        if (config.StartsWith("vless://"))
            return ConfigType.VLessLink;
        if (config.StartsWith("trojan://"))
            return ConfigType.TrojanLink;
        if (config.StartsWith("ss://"))
            return ConfigType.ShadowsocksLink;
        if (config.StartsWith("{"))
            return ConfigType.JsonConfig;
        if (config.StartsWith("http"))
            return ConfigType.SubscriptionUrl;
        
        return ConfigType.Unknown;
    }
    
    public async Task<bool> LoadConfigAsync(string config)
    {
        var type = DetectConfigType(config);
        
        return type switch
        {
            ConfigType.VMessLink => await LoadVMessAsync(config),
            ConfigType.VLessLink => await LoadVLessAsync(config),
            ConfigType.TrojanLink => await LoadTrojanAsync(config),
            ConfigType.ShadowsocksLink => await LoadShadowsocksAsync(config),
            ConfigType.JsonConfig => await LoadJsonAsync(config),
            ConfigType.SubscriptionUrl => await LoadSubscriptionAsync(config),
            _ => false
        };
    }
    
    private async Task<bool> LoadVMessAsync(string link)
    {
        try
        {
            var base64 = link.Replace("vmess://", "");
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            LogMessage?.Invoke("✅ VMess config parsed", "success");
            ActiveConfig = link;
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ VMess error: {ex.Message}", "error");
            return false;
        }
    }
    
    private async Task<bool> LoadVLessAsync(string link)
    {
        try
        {
            LogMessage?.Invoke("✅ VLess config parsed", "success");
            ActiveConfig = link;
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ VLess error: {ex.Message}", "error");
            return false;
        }
    }
    
    private async Task<bool> LoadTrojanAsync(string link)
    {
        try
        {
            LogMessage?.Invoke("✅ Trojan config parsed", "success");
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
        try
        {
            LogMessage?.Invoke("✅ Shadowsocks config parsed", "success");
            ActiveConfig = link;
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ Shadowsocks error: {ex.Message}", "error");
            return false;
        }
    }
    
    private async Task<bool> LoadJsonAsync(string json)
    {
        try
        {
            var config = JsonDocument.Parse(json);
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
            
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(response));
                var lines = decoded.Split('\n');
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("vmess://") || line.StartsWith("vless://") ||
                        line.StartsWith("trojan://") || line.StartsWith("ss://"))
                    {
                        LogMessage?.Invoke($"🔗 Found: {DetectConfigType(line)}", "info");
                    }
                }
                
                ActiveConfig = url;
                return true;
            }
            catch
            {
                var lines = response.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("vmess://") || line.StartsWith("vless://") ||
                        line.StartsWith("trojan://") || line.StartsWith("ss://"))
                    {
                        LogMessage?.Invoke($"🔗 Found: {DetectConfigType(line)}", "info");
                    }
                }
                
                ActiveConfig = url;
                return true;
            }
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ Subscription error: {ex.Message}", "error");
            return false;
        }
    }
    
    public async Task<bool> StartXrayAsync(string xrayPath = "xray")
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = xrayPath,
                Arguments = "run",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            _xrayProcess = Process.Start(psi);
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
    
    public void StopXray()
    {
        if (_xrayProcess != null && !_xrayProcess.HasExited)
        {
            _xrayProcess.Kill();
            _xrayProcess.Dispose();
        }
        _xrayProcess = null;
        IsRunning = false;
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
    public string port { get; set; } = "";
    public string id { get; set; } = "";
    public string aid { get; set; } = "0";
    public string net { get; set; } = "ws";
    public string type { get; set; } = "none";
    public string host { get; set; } = "";
    public string path { get; set; } = "/";
    public string tls { get; set; } = "";
}