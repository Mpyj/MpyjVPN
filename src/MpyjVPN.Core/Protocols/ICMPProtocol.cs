using System.Net.NetworkInformation;

namespace MpyjCore.Protocols;

public class ICMPProtocol : IProtocol
{
    public string Name => "ICMP";
    public string Icon => "📡";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 3000);
            Status = reply.Status == IPStatus.Success ? ProtocolStatus.Connected : ProtocolStatus.Inactive;
            return reply.Status == IPStatus.Success;
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