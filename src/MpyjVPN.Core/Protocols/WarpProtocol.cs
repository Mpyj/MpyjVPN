using System.Diagnostics;

namespace MpyjCore.Protocols;

public class WarpProtocol : IProtocol
{
    public string Name => "WARP";
    public string Icon => "🌐";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public async Task<bool> ConnectAsync()
    {
        return await ExecuteAsync("connect", 15000);
    }
    
    public async Task<bool> DisconnectAsync()
    {
        return await ExecuteAsync("disconnect", 5000);
    }
    
    public async Task<bool> TestAsync()
    {
        var output = await ExecuteWithOutputAsync("status", 5000);
        
        // چک کردن وضعیت
        if (output.Contains("Connected", StringComparison.OrdinalIgnoreCase))
        {
            Status = ProtocolStatus.Connected;
            return true;
        }
        
        // اگه وصل نیست، امتحان کن وصل بشه
        await ConnectAsync();
        
        output = await ExecuteWithOutputAsync("status", 5000);
        var connected = output.Contains("Connected", StringComparison.OrdinalIgnoreCase);
        Status = connected ? ProtocolStatus.Connected : ProtocolStatus.Inactive;
        
        return connected;
    }
    
    public ProtocolResult GetResult()
    {
        return new ProtocolResult
        {
            Working = Status == ProtocolStatus.Connected,
            Score = Status == ProtocolStatus.Connected ? 90 : 0
        };
    }
    
    private Task<bool> ExecuteAsync(string args, int timeout)
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "warp-cli",
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null) return false;
                
                process.WaitForExit(timeout);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });
    }
    
    private Task<string> ExecuteWithOutputAsync(string args, int timeout)
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "warp-cli",
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(psi);
                if (process == null) return "";
                
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit(timeout);
                
                return output + error;
            }
            catch (Exception)
            {
                return "";
            }
        });
    }
}