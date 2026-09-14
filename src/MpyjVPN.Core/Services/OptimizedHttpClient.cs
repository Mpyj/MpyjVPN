using System.Net;
using System.Net.Http;

namespace MpyjCore.Services;

public static class OptimizedHttpClient
{
    private static HttpClient? _shared;
    private static readonly object _lock = new();
    
    public static HttpClient Get()
    {
        if (_shared == null)
        {
            lock (_lock)
            {
                _shared ??= Create();
            }
        }
        return _shared;
    }
    
    private static HttpClient Create()
    {
        // ✅ تنظیمات بهینه
        var handler = new SocketsHttpHandler
        {
            // Connection Pooling
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 10,
            
            // Timeouts
            ConnectTimeout = TimeSpan.FromSeconds(5),
            
            // Compression
            AutomaticDecompression = DecompressionMethods.All,
            
            // Keep-Alive
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests,
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            
            // Buffer Sizes
            InitialHttp2StreamWindowSize = 1024 * 1024,
            
            // DNS
            UseCookies = false,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
        };
        
        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(15),
            DefaultRequestVersion = HttpVersion.Version20,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
        };
        
        // Default Headers
        client.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        
        return client;
    }
    
    public static void Dispose()
    {
        _shared?.Dispose();
        _shared = null;
    }
}