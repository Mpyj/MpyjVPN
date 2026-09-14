using System.Net.Http;

namespace MpyjCore.Protocols;

public class DohProtocol : IProtocol
{
    public string Name => "DoH";
    public string Icon => "🔍";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        try
        {
            // روش ۱: با IP مستقیم گوگل
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://8.8.8.8/resolve?name=google.com&type=A"
            );
            request.Headers.Host = "dns.google";
            request.Headers.Add("Accept", "application/dns-json");
            
            var response = await _client.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.Contains("Answer"))
                {
                    Status = ProtocolStatus.Connected;
                    return true;
                }
            }
        }
        catch { }
        
        try
        {
            // روش ۲: با IP دوم گوگل
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://8.8.4.4/resolve?name=google.com&type=A"
            );
            request.Headers.Host = "dns.google";
            request.Headers.Add("Accept", "application/dns-json");
            
            var response = await _client.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                Status = ProtocolStatus.Connected;
                return true;
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
            Score = Status == ProtocolStatus.Connected ? 85 : 0
        };
    }
}