using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MpyjVPN.Application.Services;

public class CliService
{
    private readonly string _cliPath;
    
    public CliService()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _cliPath = Path.Combine(baseDir, "cli", "MpyjCLI.exe");
        
        if (!File.Exists(_cliPath))
        {
            _cliPath = Path.Combine(baseDir, "MpyjCLI.exe");
        }
    }
    
    public string CliPath => _cliPath;
    public bool IsAvailable => File.Exists(_cliPath);
    
    // ==================== CONNECT ====================
    
    public async Task<string> ConnectAsync(string mode)
    {
        return await ExecutePersistentAsync(20000, "connect", mode);
    }
    
    public async Task<string> LoadConfigAsync(string config)
    {
        // 5 Ø«Ø§Ù†ÛŒÙ‡ ØµØ¨Ø± (Ú©Ø§ÙÛŒÙ‡ Ø¨Ø±Ø§ÛŒ VLess parsed + Xray started)
        return await ExecutePersistentAsync(5000, "config", config);
    }
    
    public async Task<string> ConnectLayersAsync(string layers)
    {
        return await ExecutePersistentAsync(4000, "connect-layers", layers);
    }
    
    // ==================== DISCONNECT ====================
    
    public async Task<string> DisconnectAsync()
    {
        KillAllCliProcesses();
        await Task.Delay(200);
        
        var result = await ExecuteWithTimeoutAsync(3000, "disconnect");
        
        KillAllCliProcesses();
        KillXrayProcesses();
        
        return result;
    }
    
    // ==================== SCAN ====================
    
    public async Task<string> ScanIPsAsync(string service = "all")
    {
        return await ExecuteWithTimeoutAsync(60000, "scan", service);
    }
    
    // ==================== DIAGNOSTICS ====================
    
    public async Task<string> RunDiagnosticsAsync()
    {
        return await ExecuteWithTimeoutAsync(30000, "diagnostics");
    }
    
    public async Task<string> GetProtocolsAsync()
    {
        return await ExecuteWithTimeoutAsync(5000, "protocols");
    }
    
    public async Task<string> GetLayersAsync()
    {
        return await ExecuteWithTimeoutAsync(10000, "layers");
    }
    
    // ==================== CORE ====================
    
    private async Task<string> ExecutePersistentAsync(int waitMs, params string[] args)
    {
        try
        {
            KillAllCliProcesses();
            await Task.Delay(200);
            
            var psi = new ProcessStartInfo
            {
                FileName = _cliPath,
                WorkingDirectory = Path.GetDirectoryName(_cliPath) ?? "",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            
            foreach (var arg in args)
                psi.ArgumentList.Add(arg);
            
            var process = Process.Start(psi);
            if (process == null) return "âŒ Failed to start CLI";
            
            var stdoutBuilder = new StringBuilder();
            var stderrBuilder = new StringBuilder();
            
            var stdoutTask = Task.Run(async () =>
            {
                try
                {
                    var reader = process.StandardOutput;
                    while (!process.HasExited)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line == null) break;
                        lock (stdoutBuilder) stdoutBuilder.AppendLine(line);
                    }
                }
                catch { }
            });
            
            var stderrTask = Task.Run(async () =>
            {
                try
                {
                    var reader = process.StandardError;
                    while (!process.HasExited)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line == null) break;
                        lock (stderrBuilder) stderrBuilder.AppendLine(line);
                    }
                }
                catch { }
            });
            
            await Task.Delay(waitMs);
            
            try
            {
                if (!process.HasExited)
                    process.Kill(true);
            }
            catch { }
            
            await Task.Delay(300);
            
            var result = stdoutBuilder.ToString();
            if (stderrBuilder.Length > 0)
                result += stderrBuilder.ToString();
            
            return result;
        }
        catch (Exception ex)
        {
            return $"âŒ Error: {ex.Message}";
        }
    }
    
    private async Task<string> ExecuteWithTimeoutAsync(int timeoutMs, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _cliPath,
                WorkingDirectory = Path.GetDirectoryName(_cliPath) ?? "",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            
            foreach (var arg in args)
                psi.ArgumentList.Add(arg);
            
            using var process = Process.Start(psi);
            if (process == null) return "âŒ Failed to start CLI";
            
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            
            await Task.WhenAny(
                Task.WhenAll(stdoutTask, stderrTask),
                Task.Delay(timeoutMs));
            
            var stdout = "";
            var stderr = "";
            
            try
            {
                if (stdoutTask.IsCompleted) stdout = await stdoutTask;
                if (stderrTask.IsCompleted) stderr = await stderrTask;
            }
            catch { }
            
            try
            {
                if (!process.HasExited)
                    process.Kill(true);
            }
            catch { }
            
            if (!string.IsNullOrEmpty(stderr))
                return stdout + stderr;
            
            return stdout;
        }
        catch (Exception ex)
        {
            return $"âŒ Error: {ex.Message}";
        }
    }
    
    private void KillAllCliProcesses()
    {
        try
        {
            var processes = Process.GetProcessesByName("MpyjCLI");
            foreach (var p in processes)
            {
                try { p.Kill(true); } catch { }
            }
        }
        catch { }
    }
    
    private void KillXrayProcesses()
    {
        try
        {
            var processes = Process.GetProcessesByName("xray");
            foreach (var p in processes)
            {
                try { p.Kill(true); } catch { }
            }
        }
        catch { }
    }
}