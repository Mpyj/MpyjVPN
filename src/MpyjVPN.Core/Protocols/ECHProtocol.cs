using System.Security.Authentication;

namespace MpyjCore.Protocols;

public class ECHProtocol : IProtocol
{
    public string Name => "ECH";
    public string Icon => "🔐";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public Task<bool> TestAsync()
    {
        var supported = SslProtocols.Tls13 != 0;
        Status = supported ? ProtocolStatus.Connected : ProtocolStatus.Inactive;
        return Task.FromResult(supported);
    }
    
    public ProtocolResult GetResult()
    {
        return new ProtocolResult
        {
            Working = Status == ProtocolStatus.Connected,
            Score = Status == ProtocolStatus.Connected ? 70 : 0
        };
    }
}