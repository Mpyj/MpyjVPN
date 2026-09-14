using System.Collections.Concurrent;
using System.Net;

namespace MpyjCore.Services;

public class DnsCacheService
{
    private readonly ConcurrentDictionary<string, CachedDns> _cache = new();
    private readonly TimeSpan _ttl;
    
    public DnsCacheService(TimeSpan? ttl = null)
    {
        _ttl = ttl ?? TimeSpan.FromMinutes(10);
    }
    
    public async Task<string?> ResolveAsync(string hostname)
    {
        // چک کش
        if (_cache.TryGetValue(hostname, out var cached))
        {
            if (DateTime.Now - cached.CachedAt < _ttl)
            {
                return cached.IP;
            }
            _cache.TryRemove(hostname, out _);
        }
        
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(hostname);
            if (addresses.Length > 0)
            {
                var ip = addresses[0].ToString();
                _cache[hostname] = new CachedDns
                {
                    Hostname = hostname,
                    IP = ip,
                    CachedAt = DateTime.Now
                };
                return ip;
            }
        }
        catch { }
        
        return null;
    }
    
    public void Clear()
    {
        _cache.Clear();
    }
    
    public void Cleanup()
    {
        var expired = _cache
            .Where(kv => DateTime.Now - kv.Value.CachedAt >= _ttl)
            .Select(kv => kv.Key)
            .ToList();
        
        foreach (var key in expired)
        {
            _cache.TryRemove(key, out _);
        }
    }
    
    public int Count => _cache.Count;
    
    private class CachedDns
    {
        public string Hostname { get; set; } = "";
        public string IP { get; set; } = "";
        public DateTime CachedAt { get; set; }
    }
}