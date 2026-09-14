namespace MpyjCore.Services;

public class WarpMultiService
{
    private readonly WarpInWarpService _warpInWarp = new();
    private readonly WarpWorkerService _warpWorker = new();
    private readonly DnsService _dnsService = new();
    
    public event Action<string, string>? LogMessage;
    
    public bool IsRunning { get; private set; }
    public string CurrentMode { get; private set; } = "";
    
    public WarpMultiService()
    {
        _warpInWarp.LogMessage += (msg, type) => LogMessage?.Invoke(msg, type);
        _warpWorker.LogMessage += (msg, type) => LogMessage?.Invoke(msg, type);
    }
    
    public async Task<bool> ConnectWarpInWarpAsync()
    {
        LogMessage?.Invoke("🧅 Mode: WARP in WARP", "info");
        
        var result = await _warpInWarp.ConnectAsync();
        
        if (result)
        {
            CurrentMode = "warp-in-warp";
            IsRunning = true;
        }
        
        return result;
    }
    
    public async Task<bool> ConnectWarpWorkerAsync()
    {
        LogMessage?.Invoke("🧅 Mode: WARP + Worker", "info");
        
        var result = await _warpWorker.ConnectAsync();
        
        if (result)
        {
            CurrentMode = "warp-worker";
            IsRunning = true;
        }
        
        return result;
    }
    
    public async Task<bool> ConnectFullStackAsync()
    {
        LogMessage?.Invoke("💀 Mode: FULL STACK", "info");
        LogMessage?.Invoke("Combining: WARP + WARP + Worker + DNS", "info");
        
        // لایه ۱: DNS
        LogMessage?.Invoke("🔍 Layer 1: DNS...", "info");
        await _dnsService.ChangeDnsAsync("1.1.1.1", "1.0.0.1");
        
        // لایه ۲: WARP in WARP
        LogMessage?.Invoke("🧅 Layer 2: WARP in WARP...", "info");
        var warpInWarpOk = await _warpInWarp.ConnectAsync();
        
        // لایه ۳: Worker
        LogMessage?.Invoke("☁️ Layer 3: Worker...", "info");
        var workerOk = await _warpWorker.ConnectAsync();
        
        IsRunning = warpInWarpOk || workerOk;
        CurrentMode = "full-stack";
        
        LogMessage?.Invoke(IsRunning ? "✅ FULL STACK active!" : "❌ Failed", 
            IsRunning ? "success" : "error");
        
        return IsRunning;
    }
    
    public async Task DisconnectAsync()
    {
        await _warpInWarp.DisconnectAsync();
        await _warpWorker.DisconnectAsync();
        await _dnsService.ResetDnsAsync();
        
        IsRunning = false;
        CurrentMode = "";
        LogMessage?.Invoke("✅ Disconnected", "success");
    }
}