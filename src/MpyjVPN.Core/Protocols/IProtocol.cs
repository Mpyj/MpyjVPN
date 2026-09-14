namespace MpyjCore.Protocols;

public interface IProtocol
{
    string Name { get; }
    string Icon { get; }
    ProtocolStatus Status { get; }
    
    Task<bool> ConnectAsync();
    Task<bool> DisconnectAsync();
    Task<bool> TestAsync();
    ProtocolResult GetResult();
}

public enum ProtocolStatus
{
    Inactive,
    Connecting,
    Connected,
    Failed
}

public class ProtocolResult
{
    public bool Working { get; set; }
    public double PingMs { get; set; }
    public int Score { get; set; }
    public string? Error { get; set; }
}