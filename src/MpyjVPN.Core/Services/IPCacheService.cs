using System.Text.Json;

namespace MpyjCore.Services;

public class IPCacheService
{
    private readonly string _cacheFile;
    private List<ScannedIP> _cachedIPs = new();
    private DateTime _lastUpdate = DateTime.MinValue;
    
    public event Action<string, string>? LogMessage;
    
    public IPCacheService()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MpyjVPN"
        );
        Directory.CreateDirectory(appData);
        _cacheFile = Path.Combine(appData, "ip_cache.json");
        
        LoadCache();
    }
    
    public List<ScannedIP> GetCachedIPs()
    {
        // اگه کش قدیمیه (بیشتر از ۱ ساعت)، به‌روزرسانی کن
        if (DateTime.Now - _lastUpdate > TimeSpan.FromHours(1))
        {
            LogMessage?.Invoke("⚠️ IP cache is old, will refresh", "warning");
        }
        
        return _cachedIPs;
    }
    
    public string? GetBestIP()
    {
        if (_cachedIPs.Count == 0) return null;
        
        // بهترین IP (کم‌ترین latency)
        var best = _cachedIPs.OrderBy(x => x.LatencyMs).FirstOrDefault();
        return best?.IP;
    }
    
    public List<string> GetTopIPs(int count = 5)
    {
        return _cachedIPs
            .OrderBy(x => x.LatencyMs)
            .Take(count)
            .Select(x => x.IP)
            .ToList();
    }
    
    public void SaveCache(List<ScannedIP> ips)
    {
        try
        {
            _cachedIPs = ips;
            _lastUpdate = DateTime.Now;
            
            var data = new
            {
                LastUpdate = _lastUpdate,
                IPs = ips
            };
            
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(_cacheFile, json);
            LogMessage?.Invoke($"✅ Cached {ips.Count} IPs", "success");
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"⚠️ Cache save error: {ex.Message}", "warning");
        }
    }
    
    private void LoadCache()
    {
        try
        {
            if (File.Exists(_cacheFile))
            {
                var json = File.ReadAllText(_cacheFile);
                var data = JsonSerializer.Deserialize<CacheData>(json);
                
                if (data != null)
                {
                    _cachedIPs = data.IPs ?? new List<ScannedIP>();
                    _lastUpdate = data.LastUpdate;
                    LogMessage?.Invoke($"📂 Loaded {_cachedIPs.Count} IPs from cache", "info");
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"⚠️ Cache load error: {ex.Message}", "warning");
        }
    }
    
    public void ClearCache()
    {
        _cachedIPs.Clear();
        _lastUpdate = DateTime.MinValue;
        
        if (File.Exists(_cacheFile))
        {
            File.Delete(_cacheFile);
        }
        
        LogMessage?.Invoke("🗑️ Cache cleared", "info");
    }
    
    private class CacheData
    {
        public DateTime LastUpdate { get; set; }
        public List<ScannedIP>? IPs { get; set; }
    }
}