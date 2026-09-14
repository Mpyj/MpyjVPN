using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Diagnostics;
using MpyjCore.Data;

namespace MpyjCore.Services;

public class IPScannerService
{
    public event Action<string, string>? LogMessage;
    public event Action<int, int>? ScanProgress;
    
    private readonly ConcurrentBag<ScannedIP> _workingIPs = new();
    private readonly IPCacheService _cache;
    private int _totalIPs;
    private int _scannedIPs;
    
    // پورت‌های Cloudflare
    private readonly int[] _cloudflarePorts = { 443, 2053, 2083, 2087, 2096, 8443 };
    
    public IPScannerService()
    {
        _cache = new IPCacheService();
        _cache.LogMessage += (msg, type) => LogMessage?.Invoke(msg, type);
    }
    
    public IPCacheService GetCache() => _cache;
    
    public async Task<List<ScannedIP>> ScanAsync(int maxIPs = 50, bool useCache = true)
    {
        // چک کش اول
        if (useCache)
        {
            var cached = _cache.GetCachedIPs();
            if (cached.Count > 0)
            {
                LogMessage?.Invoke($"📂 Using {cached.Count} cached IPs", "info");
                return cached;
            }
        }
        
        LogMessage?.Invoke("🔍 Starting IP scan...", "info");
        
        _workingIPs.Clear();
        _totalIPs = Math.Min(maxIPs, WhiteIPs.AllIPs.Count);
        _scannedIPs = 0;
        
        // ✅ Parallel با محدودیت
        var options = new ParallelOptions 
        { 
            MaxDegreeOfParallelism = 20 
        };
        
        await Parallel.ForEachAsync(
            WhiteIPs.AllIPs.Take(maxIPs),
            options,
            async (ip, ct) =>
            {
                var result = await TestIPAsync(ip);
                if (result.Working)
                {
                    _workingIPs.Add(result);
                }
                
                Interlocked.Increment(ref _scannedIPs);
                ScanProgress?.Invoke(_scannedIPs, _totalIPs);
            }
        );
        
        var sorted = _workingIPs.OrderBy(x => x.LatencyMs).ToList();
        
        LogMessage?.Invoke($"✅ Found {sorted.Count} working IPs", "success");
        
        // ذخیره در کش
        if (sorted.Count > 0)
        {
            _cache.SaveCache(sorted);
        }
        
        return sorted;
    }
    
    private async Task<ScannedIP> TestIPAsync(string ip)
    {
        var result = new ScannedIP { IP = ip };
        
        // تست چند پورت
        foreach (var port in _cloudflarePorts)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                using var client = new TcpClient();
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                
                await client.ConnectAsync(ip, port, cts.Token);
                sw.Stop();
                
                if (client.Connected)
                {
                    result.Working = true;
                    result.LatencyMs = sw.ElapsedMilliseconds;
                    result.Port = port;
                    return result;
                }
            }
            catch { }
        }
        
        result.Working = false;
        result.LatencyMs = 999;
        return result;
    }
    
    public async Task<List<ScannedIP>> ScanCloudflareAsync()
    {
        LogMessage?.Invoke("🔍 Scanning Cloudflare IPs...", "info");
        
        var tasks = WhiteIPs.CloudflareIPs.Select(TestIPAsync);
        var results = await Task.WhenAll(tasks);
        var working = results.Where(x => x.Working).OrderBy(x => x.LatencyMs).ToList();
        
        if (working.Count > 0)
        {
            _cache.SaveCache(working);
        }
        
        return working;
    }
    
    public async Task<List<ScannedIP>> ScanGoogleAsync()
    {
        LogMessage?.Invoke("🔍 Scanning Google IPs...", "info");
        
        var tasks = WhiteIPs.GoogleIPs.Select(TestIPAsync);
        var results = await Task.WhenAll(tasks);
        var working = results.Where(x => x.Working).OrderBy(x => x.LatencyMs).ToList();
        
        if (working.Count > 0)
        {
            _cache.SaveCache(working);
        }
        
        return working;
    }
    
    public string? GetBestIP()
    {
        return _cache.GetBestIP();
    }
    
    public List<string> GetTopIPs(int count = 5)
    {
        return _cache.GetTopIPs(count);
    }
}

public class ScannedIP
{
    public string IP { get; set; } = "";
    public int Port { get; set; } = 443;
    public bool Working { get; set; }
    public double LatencyMs { get; set; }
}