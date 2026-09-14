using System.Diagnostics;

namespace MpyjCore.Services;

public class ResourceManager : IDisposable
{
    private readonly Process _currentProcess;
    private Timer? _optimizeTimer;
    private bool _disposed;
    
    public event Action<string, string>? LogMessage;
    
    public ResourceManager()
    {
        _currentProcess = Process.GetCurrentProcess();
    }
    
    public void Start()
    {
        // بهینه‌سازی هر ۶۰ ثانیه
        _optimizeTimer = new Timer(Optimize, null, 60000, 60000);
    }
    
    public void Stop()
    {
        _optimizeTimer?.Dispose();
        _optimizeTimer = null;
    }
    
    private void Optimize(object? state)
    {
        try
        {
            // ۱. پاک‌سازی حافظه
            GC.Collect(2, GCCollectionMode.Optimized);
            GC.WaitForPendingFinalizers();
            
            // ۲. تنظیم Priority
            try
            {
                if (_currentProcess.PriorityClass != ProcessPriorityClass.BelowNormal)
                {
                    _currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                }
            }
            catch { }
            
            // ۳. لاگ مصرف
            _currentProcess.Refresh();
            var memoryMB = _currentProcess.WorkingSet64 / 1024 / 1024;
            var cpuTime = _currentProcess.TotalProcessorTime.TotalSeconds;
            var threads = _currentProcess.Threads.Count;
            
            LogMessage?.Invoke($"💻 Memory: {memoryMB}MB | CPU: {cpuTime:F1}s | Threads: {threads}", "info");
        }
        catch { }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _currentProcess?.Dispose();
    }
}