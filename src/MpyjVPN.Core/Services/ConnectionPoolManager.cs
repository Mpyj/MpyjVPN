using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MpyjCore.Services;

public class ConnectionPoolManager : IDisposable
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections = new();
    private readonly Timer _cleanupTimer;
    private bool _disposed;
    
    // حداکثر تعداد اتصال همزمان
    private readonly int _maxConnections;
    
    public event Action<string, string>? LogMessage;
    
    public ConnectionPoolManager(int maxConnections = 10)
    {
        _maxConnections = maxConnections;
        _cleanupTimer = new Timer(Cleanup, null, 30000, 30000);
    }
    
    public async Task<TcpClient?> GetConnectionAsync(string host, int port, int timeoutMs = 5000)
    {
        var key = $"{host}:{port}";
        
        // چک اتصال موجود
        if (_connections.TryGetValue(key, out var existing))
        {
            if (existing.Client.Connected && !existing.IsExpired)
            {
                existing.LastUsed = DateTime.Now;
                return existing.Client;
            }
            
            // اتصال منقضی شده
            _connections.TryRemove(key, out _);
            existing.Client.Dispose();
        }
        
        // محدودیت تعداد
        if (_connections.Count >= _maxConnections)
        {
            // حذف قدیمی‌ترین
            var oldest = _connections
                .OrderBy(kv => kv.Value.LastUsed)
                .FirstOrDefault();
            
            if (oldest.Key != null)
            {
                _connections.TryRemove(oldest.Key, out var removed);
                removed?.Client.Dispose();
            }
        }
        
        // اتصال جدید
        try
        {
            var client = new TcpClient();
            var cts = new CancellationTokenSource(timeoutMs);
            await client.ConnectAsync(host, port, cts.Token);
            
            if (client.Connected)
            {
                _connections[key] = new ConnectionInfo
                {
                    Host = host,
                    Port = port,
                    Client = client,
                    CreatedAt = DateTime.Now,
                    LastUsed = DateTime.Now
                };
                
                return client;
            }
            
            client.Dispose();
            return null;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"Connection error: {ex.Message}", "warning");
            return null;
        }
    }
    
    private void Cleanup(object? state)
    {
        var expired = _connections
            .Where(kv => kv.Value.IsExpired)
            .ToList();
        
        foreach (var kv in expired)
        {
            if (_connections.TryRemove(kv.Key, out var info))
            {
                info.Client.Dispose();
            }
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        _cleanupTimer?.Dispose();
        
        foreach (var connection in _connections.Values)
        {
            connection.Client.Dispose();
        }
        
        _connections.Clear();
    }
    
    private class ConnectionInfo
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public TcpClient Client { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsed { get; set; }
        
        public bool IsExpired => 
            DateTime.Now - LastUsed > TimeSpan.FromMinutes(5) ||
            DateTime.Now - CreatedAt > TimeSpan.FromMinutes(30);
    }
}