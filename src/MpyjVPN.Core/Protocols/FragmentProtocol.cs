namespace MpyjCore.Protocols;

public class FragmentProtocol : IProtocol
{
    public string Name => "Fragment";
    public string Icon => "📦";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync()
    {
        Status = ProtocolStatus.Connected;
        return Task.FromResult(true);
    }
    
    public Task<bool> DisconnectAsync()
    {
        Status = ProtocolStatus.Inactive;
        return Task.FromResult(true);
    }
    
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
            Score = Status == ProtocolStatus.Connected ? 80 : 0
        };
    }
}