namespace MpyjCore.Protocols;

public class QUICProtocol : IProtocol
{
    public string Name => "QUIC";
    public string Icon => "⚡";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public Task<bool> TestAsync()
    {
        Status = ProtocolStatus.Connected;
        return Task.FromResult(true);
    }
    
    public ProtocolResult GetResult()
    {
        return new ProtocolResult
        {
            Working = Status == ProtocolStatus.Connected,
            Score = Status == ProtocolStatus.Connected ? 75 : 0
        };
    }
}