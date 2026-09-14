using System.Net.WebSockets;

namespace MpyjCore.Protocols;

public class WebSocketProtocol : IProtocol
{
    public string Name => "WebSocket";
    public string Icon => "🔌";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        try
        {
            using var ws = new ClientWebSocket();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await ws.ConnectAsync(new Uri("wss://echo.websocket.org"), cts.Token);
            Status = ws.State == WebSocketState.Open ? ProtocolStatus.Connected : ProtocolStatus.Inactive;
            return Status == ProtocolStatus.Connected;
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