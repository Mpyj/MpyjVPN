using System.Diagnostics;

namespace MpyjCore.Services;

public class PerformanceMonitor : IDisposable
{
    private readonly Process _process;
    private Timer? _timer;
    private bool _disposed;
    
    private TimeSpan _lastCpuTime;
    private DateTime _lastCheck;
    
    public event Action<PerformanceStats>? StatsUpdated;
    
    public PerformanceMonitor()
    {
        _process = Process.GetCurrentProcess();
        _lastCpuTime = _process.TotalProcessorTime;
        _lastCheck = DateTime.Now;
    }
    
    public void Start(int intervalMs = 5000)
    {
        _timer = new Timer(Check, null, 1000, intervalMs);
    }
    
    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }
    
    private void Check(object? state)
    {
        try
        {
            var stats = GetFullStats();
            StatsUpdated?.Invoke(stats);
        }
        catch { }
    }
    
    // ✅ آمار کامل همه پروسه‌ها
    public PerformanceStats GetFullStats()
    {
        _process.Refresh();
        
        // محاسبه CPU
        var currentCpuTime = _process.TotalProcessorTime;
        var currentTime = DateTime.Now;
        var cpuUsed = currentCpuTime - _lastCpuTime;
        var timePassed = currentTime - _lastCheck;
        var cpuPercent = timePassed.TotalMilliseconds > 0 ? 
            (cpuUsed.TotalMilliseconds / timePassed.TotalMilliseconds) 
            / Environment.ProcessorCount * 100 : 0;
        
        _lastCpuTime = currentCpuTime;
        _lastCheck = currentTime;
        
        // آمار CLI
        var cliMemoryMB = _process.WorkingSet64 / 1024 / 1024;
        var cliPrivateMB = _process.PrivateMemorySize64 / 1024 / 1024;
        var cliThreads = _process.Threads.Count;
        var cliHandles = _process.HandleCount;
        
        // ✅ آمار WARP
        var warpMemoryMB = 0L;
        var warpCpuPercent = 0.0;
        var warpThreads = 0;
        
        try
        {
            var warpProcs = Process.GetProcessesByName("warp-svc");
            foreach (var wp in warpProcs)
            {
                warpMemoryMB += wp.WorkingSet64 / 1024 / 1024;
                warpThreads += wp.Threads.Count;
            }
        }
        catch { }
        
        // ✅ آمار WebView
        var webviewMemoryMB = 0L;
        var webviewThreads = 0;
        
        try
        {
            var wvProcs = Process.GetProcessesByName("msedgewebview2");
            foreach (var wv in wvProcs)
            {
                webviewMemoryMB += wv.WorkingSet64 / 1024 / 1024;
                webviewThreads += wv.Threads.Count;
            }
        }
        catch { }
        
        return new PerformanceStats
        {
            CpuPercent = Math.Round(cpuPercent, 1),
            MemoryMB = cliMemoryMB,
            PrivateMemoryMB = cliPrivateMB,
            GcMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
            ThreadCount = cliThreads,
            HandleCount = cliHandles,
            GcGen0 = GC.CollectionCount(0),
            GcGen1 = GC.CollectionCount(1),
            GcGen2 = GC.CollectionCount(2),
            
            // ✅ جدید
            WarpMemoryMB = warpMemoryMB,
            WarpThreads = warpThreads,
            WebViewMemoryMB = webviewMemoryMB,
            WebViewThreads = webviewThreads,
            TotalMemoryMB = cliMemoryMB + warpMemoryMB + webviewMemoryMB,
            TotalThreads = cliThreads + warpThreads + webviewThreads,
            
            Timestamp = DateTime.Now
        };
    }
    
    public PerformanceStats GetCurrentStats()
    {
        return GetFullStats();
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _process?.Dispose();
    }
}

public class PerformanceStats
{
    // CLI
    public double CpuPercent { get; set; }
    public long MemoryMB { get; set; }
    public long PrivateMemoryMB { get; set; }
    public long GcMemoryMB { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public int GcGen0 { get; set; }
    public int GcGen1 { get; set; }
    public int GcGen2 { get; set; }
    
    // WARP
    public long WarpMemoryMB { get; set; }
    public int WarpThreads { get; set; }
    
    // WebView
    public long WebViewMemoryMB { get; set; }
    public int WebViewThreads { get; set; }
    
    // Total
    public long TotalMemoryMB { get; set; }
    public int TotalThreads { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public string ToShortString() => 
        $"CLI: {MemoryMB}MB | WARP: {WarpMemoryMB}MB | WebView: {WebViewMemoryMB}MB | " +
        $"TOTAL: {TotalMemoryMB}MB | CPU: {CpuPercent}%";
    
    public string ToFullString() => 
        $"╔════════════════════════════════════════╗\n" +
        $"║  MPYJ VPN - Performance Stats         ║\n" +
        $"╚════════════════════════════════════════╝\n" +
        $"\n" +
        $"🚀 CLI Process:\n" +
        $"   CPU: {CpuPercent}%\n" +
        $"   RAM: {MemoryMB}MB (Private: {PrivateMemoryMB}MB)\n" +
        $"   GC: {GcMemoryMB}MB (Gen0: {GcGen0}, Gen1: {GcGen1}, Gen2: {GcGen2})\n" +
        $"   Threads: {ThreadCount}\n" +
        $"   Handles: {HandleCount}\n" +
        $"\n" +
        $"☁️  WARP Service:\n" +
        $"   RAM: {WarpMemoryMB}MB\n" +
        $"   Threads: {WarpThreads}\n" +
        $"\n" +
        $"🌐 WebView2:\n" +
        $"   RAM: {WebViewMemoryMB}MB\n" +
        $"   Threads: {WebViewThreads}\n" +
        $"\n" +
        $"📊 TOTAL:\n" +
        $"   RAM: {TotalMemoryMB}MB\n" +
        $"   Threads: {TotalThreads}\n" +
        $"\n" +
        $"Time: {Timestamp:HH:mm:ss}";
}