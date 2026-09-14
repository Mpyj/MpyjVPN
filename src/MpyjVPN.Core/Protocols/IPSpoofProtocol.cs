using System.Net.Sockets;
using MpyjCore.Data;
using MpyjCore.Services;

namespace MpyjCore.Protocols;

public class IPSpoofProtocol : IProtocol
{
    private static IPCacheService? _cache;
    
    public string Name => "IP Spoof";
    public string Icon => "🕶️";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public string CurrentSpoofedIP { get; private set; } = "";
    
    public static void SetCache(IPCacheService cache)
    {
        _cache = cache;
    }
    
    public Task<bool> ConnectAsync()
    {
        var ip = GetIPFromCache() ?? WhiteIPs.GetRandomIP();
        CurrentSpoofedIP = ip;
        Status = ProtocolStatus.Connected;
        return Task.FromResult(true);
    }
    
    public Task<bool> DisconnectAsync()
    {
        Status = ProtocolStatus.Inactive;
        CurrentSpoofedIP = "";
        return Task.FromResult(true);
    }
    
    public async Task<bool> TestAsync()
    {
        try
        {
            // ✅ اول از کش استفاده کن
            var cachedIPs = _cache?.GetTopIPs(5) ?? new List<string>();
            
            if (cachedIPs.Count > 0)
            {
                // تست IP های کش شده
                foreach (var ip in cachedIPs)
                {
                    if (await TestSingleIPAsync(ip))
                    {
                        CurrentSpoofedIP = ip;
                        Status = ProtocolStatus.Connected;
                        return true;
                    }
                }
            }
            
            // اگه کش خالی بود، از لیست پیش‌فرض
            var ip2 = WhiteIPs.GetRandomCloudflareIP();
            if (await TestSingleIPAsync(ip2))
            {
                CurrentSpoofedIP = ip2;
                Status = ProtocolStatus.Connected;
                return true;
            }
            
            Status = ProtocolStatus.Inactive;
            return false;
        }
        catch
        {
            Status = ProtocolStatus.Inactive;
            return false;
        }
    }
    
    private async Task<bool> TestSingleIPAsync(string ip)
    {
        var ports = new[] { 443, 2053, 2083, 2087, 2096, 8443 };
        
        foreach (var port in ports)
        {
            try
            {
                using var client = new TcpClient();
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await client.ConnectAsync(ip, port, cts.Token);
                
                if (client.Connected)
                {
                    return true;
                }
            }
            catch { }
        }
        
        return false;
    }
    
    private string? GetIPFromCache()
    {
        return _cache?.GetBestIP();
    }
    
    public ProtocolResult GetResult()
    {
        return new ProtocolResult
        {
            Working = Status == ProtocolStatus.Connected,
            Score = Status == ProtocolStatus.Connected ? 85 : 0
        };
    }
}