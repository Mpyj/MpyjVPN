using System.Net.Http;

namespace MpyjCore.Protocols;

public class CloudflareWorkerProtocol : IProtocol
{
    private const string WorkerUrl = "https://shy-glade-59ba.urihghioihrerg.workers.dev";
    
    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(20)
    };
    
    public string Name => "Worker";
    public string Icon => "☁️";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        // تلاش ۳ بار
        for (int i = 0; i < 3; i++)
        {
            try
            {
                // چک زنده بودن Worker
                var response = await _client.GetAsync(WorkerUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // چک JSON
                    if (content.Contains("ok") || content.Contains("Mpyj"))
                    {
                        Status = ProtocolStatus.Connected;
                        return true;
                    }
                }
            }
            catch { }
            
            await Task.Delay(1000);
        }
        
        Status = ProtocolStatus.Inactive;
        return false;
    }
    
    public ProtocolResult GetResult()
    {
        return new ProtocolResult
        {
            Working = Status == ProtocolStatus.Connected,
            Score = Status == ProtocolStatus.Connected ? 90 : 0
        };
    }
}