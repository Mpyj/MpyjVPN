using System.Net.Http;

namespace MpyjCore.Protocols;

public class GoogleProxyProtocol : IProtocol
{
    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };
    
    public string Name => "Google Proxy";
    public string Icon => "🌐";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        try
        {
            var url = "https://translate.google.com/translate?sl=auto&tl=fa&u=https://example.com";
            var response = await _client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.Length > 100)
                {
                    Status = ProtocolStatus.Connected;
                    return true;
                }
            }
        }
        catch { }
        
        Status = ProtocolStatus.Inactive;
        return false;
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