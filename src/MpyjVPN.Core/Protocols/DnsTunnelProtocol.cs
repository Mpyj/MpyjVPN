using System.Net.Sockets;

namespace MpyjCore.Protocols;

public class DnsTunnelProtocol : IProtocol
{
    public string Name => "DNS Tunnel";
    public string Icon => "🔍";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        try
        {
            var addresses = await System.Net.Dns.GetHostAddressesAsync("google.com");
            Status = addresses.Length > 0 ? ProtocolStatus.Connected : ProtocolStatus.Inactive;
            return addresses.Length > 0;
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
            Score = Status == ProtocolStatus.Connected ? 85 : 0
        };
    }
}