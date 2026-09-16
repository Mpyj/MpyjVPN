using System.Diagnostics;
using System.Net.NetworkInformation;
using MpyjCore.Protocols;
using MpyjCore.Services;

namespace MpyjCore;

public class VpnEngine
{
    private readonly List<IProtocol> _protocols = new();
    private readonly DnsService _dnsService = new();
    private readonly ConfigService _configService = new();
    private readonly IPScannerService _ipScanner = new();
    private readonly WarpMultiService _warpMulti = new();
    private readonly ResourceManager _resourceManager = new();
    private readonly PerformanceMonitor _perfMonitor = new();
    private readonly DnsCacheService _dnsCache = new();
    private readonly ConnectionPoolManager _connectionPool = new();
    
    public event Action<string, string>? LogMessage;
    public event Action<bool>? ConnectionChanged;
    public event Action<string>? LayerChanged;
    
    public bool IsConnected { get; private set; }
    public string CurrentMode { get; set; } = "auto";
    public bool AutoReconnect { get; set; } = true;
    
    public List<string> ActiveLayers { get; private set; } = new();
    public Dictionary<string, bool> LayerStatus { get; private set; } = new();
    
    public VpnEngine()
    {
        try
        {
            System.Runtime.GCSettings.LatencyMode = 
                System.Runtime.GCLatencyMode.SustainedLowLatency;
        }
        catch { }
        
        RegisterProtocols();
        InitializeLayers();
        
        IPSpoofProtocol.SetCache(_ipScanner.GetCache());
        
        _configService.LogMessage += (msg, type) => Log(msg, type);
        _ipScanner.LogMessage += (msg, type) => Log(msg, type);
        _warpMulti.LogMessage += (msg, type) => Log(msg, type);
        _resourceManager.LogMessage += (msg, type) => Log(msg, type);
        _connectionPool.LogMessage += (msg, type) => Log(msg, type);
        
        _resourceManager.Start();
        
        _perfMonitor.StatsUpdated += (stats) => 
        {
            Log($"ðŸ’» {stats.ToShortString()}", "info");
        };
        _perfMonitor.Start(30000);
        
        try
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
        }
        catch { }
    }
    
    private void RegisterProtocols()
    {
        _protocols.Add(new WarpProtocol());
        _protocols.Add(new DohProtocol());
        _protocols.Add(new FragmentProtocol());
        _protocols.Add(new CloudflareWorkerProtocol());
        _protocols.Add(new GoogleProxyProtocol());
        _protocols.Add(new ECHProtocol());
        _protocols.Add(new QUICProtocol());
        _protocols.Add(new ICMPProtocol());
        _protocols.Add(new WebSocketProtocol());
        _protocols.Add(new DomainFrontingProtocol());
        _protocols.Add(new MultiHopProtocol());
        _protocols.Add(new DnsTunnelProtocol());
        _protocols.Add(new IPSpoofProtocol());
        _protocols.Add(new SNISpoofProtocol());
    }
    
    private void InitializeLayers()
    {
        LayerStatus = new Dictionary<string, bool>
        {
            ["WARP"] = false,
            ["WARP in WARP"] = false,
            ["WARP + Worker"] = false,
            ["Full Stack"] = false,
            ["DNS"] = false,
            ["DoH"] = false,
            ["Fragment"] = false,
            ["Worker"] = false,
            ["Google Proxy"] = false,
            ["ECH"] = false,
            ["QUIC"] = false,
            ["ICMP"] = false,
            ["WebSocket"] = false,
            ["Fronting"] = false,
            ["Multi-hop"] = false,
            ["DNS Tunnel"] = false,
            ["IP Spoof"] = false,
            ["SNI Spoof"] = false,
        };
    }
    
    public List<IProtocol> GetProtocols()
    {
        return _protocols;
    }
    
    public ConfigService GetConfigService()
    {
        return _configService;
    }
    
    public IPScannerService GetIPScanner()
    {
        return _ipScanner;
    }
    
    public PerformanceStats GetPerformanceStats()
    {
        return _perfMonitor.GetCurrentStats();
    }
    
    // ==================== WARP Multi ====================
    
    public async Task<bool> ConnectWarpModeAsync(string warpMode)
    {
        Log($"ðŸŒ WARP Mode: {warpMode}", "info");
        
        bool result = warpMode switch
        {
            "in-warp" => await _warpMulti.ConnectWarpInWarpAsync(),
            "worker" => await _warpMulti.ConnectWarpWorkerAsync(),
            "full" => await _warpMulti.ConnectFullStackAsync(),
            _ => await ConnectWarpAsync()
        };
        
        if (result)
        {
            ActiveLayers.Clear();
            ActiveLayers.Add($"WARP-{warpMode}");
            IsConnected = true;
            ConnectionChanged?.Invoke(true);
        }
        
        return result;
    }
    
    // ==================== Ø§ØªØµØ§Ù„ Ù¾ÛŒØ§Ø²ÛŒ ====================
    
    public async Task<bool> ConnectOnionAsync(List<string>? layerNames = null)
    {
        Log("ðŸ§… Starting Onion Connection...", "info");
        
        var layers = layerNames ?? new List<string>
        {
            "WARP", "DNS", "DoH", "Fragment", "Worker", "Fronting", "Multi-hop"
        };
        
        ActiveLayers.Clear();
        
        foreach (var layer in layers)
        {
            var layerOk = await ActivateLayerAsync(layer);
            
            if (layerOk)
            {
                ActiveLayers.Add(layer);
                LayerStatus[layer] = true;
                LayerChanged?.Invoke(layer);
                Log($"âœ… Layer: {layer}", "success");
            }
            else
            {
                LayerStatus[layer] = false;
                Log($"âŒ Layer failed: {layer}", "error");
            }
            
            await Task.Delay(300);
        }
        
        IsConnected = ActiveLayers.Count > 0;
        ConnectionChanged?.Invoke(IsConnected);
        
        Log(IsConnected ? "ðŸ§… Onion layers active!" : "âŒ Onion failed!", 
            IsConnected ? "success" : "error");
        
        return IsConnected;
    }
    
    private async Task<bool> ActivateLayerAsync(string layerName)
{
    try
    {
        switch (layerName)
        {
            // ==================== لایه‌های واقعی ====================
            case "DNS":
                return await _dnsService.ChangeDnsAsync("1.1.1.1", "1.0.0.1");

            case "WARP":
            case "WARP in WARP":
                return await _warpMulti.ConnectWarpInWarpAsync();

            case "WARP + Worker":
                return await _warpMulti.ConnectWarpWorkerAsync();

            case "Full Stack":
                return await _warpMulti.ConnectFullStackAsync();

            // ==================== پروتکل‌های تست‌شدنی ====================
            case "DoH":
            case "Fragment":
            case "Worker":
            case "Google Proxy":
            case "ECH":
            case "QUIC":
            case "ICMP":
            case "WebSocket":
            case "Fronting":
            case "Multi-hop":
            case "DNS Tunnel":
            case "IP Spoof":
            case "SNI Spoof":
                return await TestProtocolAsync(layerName);

            default:
                Log($"⚠️ Unknown layer: {layerName}", "warning");
                return false;
        }
    }
    catch (Exception ex)
    {
        Log($"⚠️ {layerName}: {ex.Message}", "warning");
        return false;
    }
}
    
    // ==================== Ø­Ø§Ù„Øªâ€ŒÙ‡Ø§ ====================
    
    public async Task<bool> ConnectAutoAsync()
    {
        Log("ðŸ¤– Auto Mode", "info");
        
        Log("ðŸ” Changing DNS...", "info");
        var dnsOk = await _dnsService.ChangeDnsAsync("1.1.1.1", "1.0.0.1");
        if (dnsOk)
            Log("âœ… DNS changed", "success");
        
        await Task.Delay(500);
        
        Log("ðŸŒ Connecting WARP (with retry)...", "info");
        var warpOk = await _warpMulti.ConnectWarpInWarpAsync();
        
        if (warpOk)
        {
            Log("âœ… WARP connected", "success");
            
            await Task.WhenAll(
                TestProtocolAsync("DoH"),
                TestProtocolAsync("Fragment"),
                TestProtocolAsync("Worker")
            );
            
            return true;
        }
        
        Log("âš ï¸ WARP failed, using independent layers", "warning");
        
        await Task.WhenAll(
            TestProtocolAsync("DoH"),
            TestProtocolAsync("Fragment"),
            TestProtocolAsync("Worker"),
            TestProtocolAsync("ECH"),
            TestProtocolAsync("QUIC")
        );
        
        return true;
    }
    
    public async Task<bool> ConnectOnionModeAsync()
    {
        Log("ðŸ§… Onion Mode", "info");
        
        return await ConnectOnionAsync(new List<string>
        {
            "DNS", "WARP", "DoH", "Fragment", "Fronting", "Multi-hop", "Worker"
        });
    }
    
    public async Task<bool> ConnectProtocolsModeAsync()
{
    Log("📡 Protocols Mode", "info");

    ActiveLayers.Clear();

    foreach (var protocol in _protocols)
    {
        try
        {
            Log($"🔧 Testing: {protocol.Name}...", "info");

            var working = await protocol.TestAsync();

            if (working)
            {
                ActiveLayers.Add(protocol.Name);
                LayerStatus[protocol.Name] = true;
                LayerChanged?.Invoke(protocol.Name);
                Log($"✅ {protocol.Name}: OK", "success");
            }
            else
            {
                LayerStatus[protocol.Name] = false;
                Log($"❌ {protocol.Name}: Failed", "warning");
            }

            await Task.Delay(200);
        }
        catch (Exception ex)
        {
            Log($"⚠️ {protocol.Name}: {ex.Message}", "warning");
        }
    }

    IsConnected = ActiveLayers.Count > 0;
    ConnectionChanged?.Invoke(IsConnected);

    Log(IsConnected ? $"✅ {ActiveLayers.Count} protocol(s) active" : "❌ No protocols active",
        IsConnected ? "success" : "error");

    return IsConnected;
}
    
    public async Task<bool> ConnectUltraModeAsync()
    {
        Log("ðŸ’€ ULTRA MODE!", "info");
        
        var allLayers = new List<string>
        {
            "DNS", "WARP in WARP", "WARP + Worker",
            "DoH", "Fragment", "Worker",
            "Google Proxy", "ECH", "QUIC", "ICMP",
            "WebSocket", "Fronting", "Multi-hop", "DNS Tunnel",
            "IP Spoof", "SNI Spoof"
        };
        
        return await ConnectOnionAsync(allLayers);
    }
    
    // ==================== Ø¨Ø§ Ú©Ø§Ù†ÙÛŒÚ¯ ====================
    
    /// <summary>
    /// Ø§ØªØµØ§Ù„ Ø¨Ø§ Ú©Ø§Ù†ÙÛŒÚ¯ (ÙÙ‚Ø· Xray Ø±Ùˆ Ø±Ø§Ù‡ Ù…ÛŒâ€ŒÙ†Ø¯Ø§Ø²Ù‡ØŒ Ù„Ø§ÛŒÙ‡â€ŒÙ‡Ø§ Ø±Ùˆ Ú©Ø§Ø±Ø¨Ø± Ø®ÙˆØ¯Ø´ Ø§Ù†ØªØ®Ø§Ø¨ Ù…ÛŒâ€ŒÚ©Ù†Ù‡)
    /// </summary>
    public async Task<bool> ConnectWithConfigAsync(string config)
{
    Log("📄 Loading config...", "info");

    var configOk = await _configService.LoadConfigAsync(config);
    if (!configOk)
    {
        Log("❌ Config load failed", "error");
        return false;
    }

    Log("🚀 Starting config...", "info");
    var started = await _configService.StartXrayAsync();

    if (!started)
    {
        Log("❌ Xray failed to start", "error");
        return false;
    }

    Log("✅ Config connected!", "success");

            // ۳. تنظیم پروکسی سیستم (بعد از Xray)
            Log("🔧 Setting system proxy...", "info");
            try
            {
                _configService.SetSystemProxy();
            }
            catch (Exception ex)
            {
                Log($"⚠️ System proxy error: {ex.Message}", "warning");
            }

    // ۱. تنظیم DNS
    Log("🔧 Changing DNS...", "info");
    await _dnsService.ChangeDnsAsync("1.1.1.1", "1.0.0.1");

    // ۲. تنظیم پروکسی سیستم
    Log("🔧 Setting system proxy...", "info");
    _configService.SetSystemProxy();

    IsConnected = true;
    ConnectionChanged?.Invoke(true);

    return true;
}
    
    /// <summary>
    /// Ø§ØªØµØ§Ù„ Ø¨Ø§ Ú©Ø§Ù†ÙÛŒÚ¯ + Ù„Ø§ÛŒÙ‡â€ŒÙ‡Ø§ÛŒ Ø³ÙØ§Ø±Ø´ÛŒ
    /// </summary>
    public async Task<bool> ConnectWithConfigAndLayersAsync(string config, List<string> layers)
    {
        // Ø§ÙˆÙ„ config Ø±Ùˆ Ø±Ø§Ù‡ Ø¨Ù†Ø¯Ø§Ø²
        var configOk = await ConnectWithConfigAsync(config);
        if (!configOk) return false;
        
        // Ø¨Ø¹Ø¯ Ù„Ø§ÛŒÙ‡â€ŒÙ‡Ø§
        if (layers.Count > 0)
        {
            Log($"ðŸ”— Applying {layers.Count} layer(s)...", "info");
            await ConnectOnionAsync(layers);
        }
        
        return IsConnected;
    }
    
    // ==================== Ø§Ø³Ú©Ù†Ø± IP ====================
    
    public async Task<List<ScannedIP>> ScanIPsAsync(string service = "all")
    {
        Log($"ðŸ” Scanning {service} IPs...", "info");
        
        var results = service.ToLower() switch
        {
            "cloudflare" => await _ipScanner.ScanCloudflareAsync(),
            "google" => await _ipScanner.ScanGoogleAsync(),
            _ => await _ipScanner.ScanAsync()
        };
        
        Log($"âœ… Found {results.Count} working IPs", "success");
        
        foreach (var ip in results.Take(5))
        {
            Log($"  âš¡ {ip.IP}:{ip.Port} ({ip.LatencyMs}ms)", "info");
        }
        
        return results;
    }
    
    public async Task<List<ScannedIP>> ScanAndCacheIPsAsync(string service = "cloudflare")
    {
        Log($"ðŸ” Scanning and caching IPs...", "info");
        
        var results = await _ipScanner.ScanAsync(50, useCache: false);
        
        Log($"âœ… Cached {results.Count} IPs for future use", "success");
        
        return results;
    }
    
    // ==================== Ø§ØªØµØ§Ù„ Ø§ØµÙ„ÛŒ ====================
    
    public async Task<bool> ConnectAsync()
    {
        Log("ðŸš€ Connecting...", "info");
        
        bool success = CurrentMode switch
        {
            "auto" => await ConnectAutoAsync(),
            "onion" => await ConnectOnionModeAsync(),
            "protocols" => await ConnectProtocolsModeAsync(),
            "ultra" => await ConnectUltraModeAsync(),
            "warp-in-warp" => await ConnectWarpModeAsync("in-warp"),
            "warp-worker" => await ConnectWarpModeAsync("worker"),
            "warp-full" => await ConnectWarpModeAsync("full"),
            _ => await ConnectAutoAsync()
        };
        
        IsConnected = success;
        ConnectionChanged?.Invoke(success);
        
        Log(success ? "âœ… Ready!" : "âŒ Failed!", success ? "success" : "error");
        
        return success;
    }
    
    // ==================== Ù‚Ø·Ø¹ ====================
    
    public async Task DisconnectAsync()
    {
        Log("ðŸ›‘ Disconnecting...", "info");
        
        await _warpMulti.DisconnectAsync();
        
        foreach (var protocol in _protocols)
        {
            try
            {
                await protocol.DisconnectAsync();
            }
            catch { }
        }
        
        await _dnsService.ResetDnsAsync();
        _configService.StopXray();
        
        ActiveLayers.Clear();
        foreach (var key in LayerStatus.Keys.ToList())
        {
            LayerStatus[key] = false;
        }
        
        IsConnected = false;
        ConnectionChanged?.Invoke(false);
        Log("âœ… Disconnected", "success");
    }
    
    // ==================== Helper Ù‡Ø§ ====================
    
    private async Task<bool> ConnectWarpAsync()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "warp-cli",
                Arguments = "connect",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            using (var process = Process.Start(psi))
            {
                if (process == null) return false;
                await process.WaitForExitAsync();
            }
            
            await Task.Delay(3000);
            
            return await CheckWarpStatusAsync();
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<bool> CheckWarpStatusAsync()
    {
        try
        {
            var statusPsi = new ProcessStartInfo
            {
                FileName = "warp-cli",
                Arguments = "status",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            using var statusProcess = Process.Start(statusPsi);
            if (statusProcess == null) return false;
            
            string output = await statusProcess.StandardOutput.ReadToEndAsync();
            string error = await statusProcess.StandardError.ReadToEndAsync();
            await statusProcess.WaitForExitAsync();
            
            return (output + error).Contains("Connected", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<bool> TestProtocolAsync(string name)
    {
        var protocol = _protocols.FirstOrDefault(p => p.Name == name);
        if (protocol == null) return false;
        
        try
        {
            var working = await protocol.TestAsync();
            return working;
        }
        catch
        {
            return false;
        }
    }
    
    // ==================== Diagnostics ====================
    
    public async Task<DiagnosticsResult> RunDiagnosticsAsync()
    {
        Log("ðŸ”§ Running diagnostics...", "info");
        
        var pingMs = await TestPingAsync();
        Log($"ðŸ“¡ Ping: {pingMs}ms", pingMs < 100 ? "success" : "warning");
        
        var dnsOk = await TestDnsAsync();
        Log(dnsOk ? "âœ… DNS working" : "âŒ DNS failed", dnsOk ? "success" : "error");
        
        var internetOk = await TestInternetAsync();
        Log(internetOk ? "âœ… Internet working" : "âŒ No internet", internetOk ? "success" : "error");
        
        var stats = _perfMonitor.GetCurrentStats();
        Log($"ðŸ’» {stats.ToShortString()}", "info");
        
        var bestIP = _ipScanner.GetBestIP();
        if (!string.IsNullOrEmpty(bestIP))
        {
            Log($"ðŸ•¶ï¸ Best Cached IP: {bestIP}", "info");
        }
        
        Log("ðŸ“‹ Layer Status:", "info");
        foreach (var layer in LayerStatus)
        {
            Log($"  {(layer.Value ? "âœ…" : "âŒ")} {layer.Key}", layer.Value ? "success" : "error");
        }
        
        if (internetOk && pingMs < 100)
            Log("ðŸ“¡ Status: ðŸŸ¢ Excellent", "success");
        else if (internetOk)
            Log("ðŸ“¡ Status: ðŸŸ¡ Good", "success");
        else
            Log("ðŸ“¡ Status: ðŸ”´ No Internet", "error");
        
        return new DiagnosticsResult
        {
            PingMs = pingMs,
            DnsWorking = dnsOk,
            InternetWorking = internetOk,
            StatusText = internetOk ? "Connected" : "Disconnected"
        };
    }
    
    private async Task<double> TestPingAsync()
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 3000);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : 999;
        }
        catch
        {
            return 999;
        }
    }
    
    private async Task<bool> TestDnsAsync()
    {
        try
        {
            var addresses = await System.Net.Dns.GetHostAddressesAsync("google.com");
            return addresses.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<bool> TestInternetAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync("https://google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    public void AddLog(string message, string type)
    {
        LogMessage?.Invoke(message, type);
    }
    
    private void Log(string message, string type)
    {
        LogMessage?.Invoke(message, type);
    }
}

public class DiagnosticsResult
{
    public double PingMs { get; set; }
    public bool DnsWorking { get; set; }
    public bool InternetWorking { get; set; }
    public string StatusText { get; set; } = "";
}