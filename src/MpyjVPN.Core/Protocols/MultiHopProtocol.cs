using System.Net.Sockets;

namespace MpyjCore.Protocols;

public class MultiHopProtocol : IProtocol
{
    public string Name => "Multi-hop";
    public string Icon => "🔗";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        var hosts = new[] { "google.com", "cloudflare.com" };
        
        foreach (var host in hosts)
        {
            try
            {
                using var client = new TcpClient();
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await client.ConnectAsync(host, 443, cts.Token);
            }
            catch
            {
                return false;
            }
        }
        
        Status = ProtocolStatus.Connected;
        return true;
    }
    
    public ProtocolResult GetResult()
    {
        return new ProtocolResult
        {
            Working = Status == ProtocolStatus.Connected,
            Score = Status == ProtocolStatus.Connected ? 65 : 0
        };
    }
}