using System.Net.Sockets;

namespace MpyjCore.Protocols;

public class DomainFrontingProtocol : IProtocol
{
    public string Name => "Fronting";
    public string Icon => "🎭";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        try
        {
            using var client = new TcpClient();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await client.ConnectAsync("cloudflare.com", 443, cts.Token);
            Status = client.Connected ? ProtocolStatus.Connected : ProtocolStatus.Inactive;
            return client.Connected;
        }
        catch
        {
            return false;
        }
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