using System.Diagnostics;
using System.Net.Http;

namespace MpyjCore.Services;

public class WarpWorkerService
{
    public event Action<string, string>? LogMessage;

    public bool IsRunning { get; private set; }
    public string WorkerUrl { get; set; } = "https://shy-glade-59ba.urihghioihrerg.workers.dev";

    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };

    private string GetWarpCliPath()
    {
        var paths = new[]
        {
            @"C:\Program Files\Cloudflare\Cloudflare WARP\warp-cli.exe",
            @"C:\Program Files (x86)\Cloudflare\Cloudflare WARP\warp-cli.exe"
        };

        foreach (var path in paths)
        {
            if (File.Exists(path)) return path;
        }

        return "warp-cli";
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            LogMessage?.Invoke("🧅 Starting WARP + Worker...", "info");

            await DisconnectWarpAsync();
            await Task.Delay(1000);

            LogMessage?.Invoke("🌐 Layer 1: Connecting WARP...", "info");
            var warpOk = await ConnectWarpWithRetryAsync(5);

            if (!warpOk)
            {
                LogMessage?.Invoke("❌ WARP failed after retries", "error");
                return false;
            }

            LogMessage?.Invoke("✅ WARP connected", "success");
            await Task.Delay(3000);

            LogMessage?.Invoke("☁️ Layer 2: Testing Worker...", "info");
            var workerOk = await TestWorkerWithRetryAsync(3);

            if (!workerOk)
            {
                LogMessage?.Invoke("⚠️ Worker failed, using WARP only", "warning");
                IsRunning = true;
                return true;
            }

            LogMessage?.Invoke("✅ Worker connected", "success");
            IsRunning = true;
            LogMessage?.Invoke("✅ WARP + Worker active!", "success");
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ Error: {ex.Message}", "error");
            return false;
        }
    }

    private async Task<bool> ConnectWarpWithRetryAsync(int maxRetries)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            LogMessage?.Invoke($"🔄 WARP attempt {i + 1}/{maxRetries}...", "info");

            var result = await ConnectWarpOnceAsync();
            if (result) return true;

            if (i < maxRetries - 1)
                await Task.Delay(3000);
        }
        return false;
    }

    private async Task<bool> ConnectWarpOnceAsync()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = GetWarpCliPath(),
                Arguments = "connect",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            await process.WaitForExitAsync();
            await Task.Delay(15000);

            return await CheckWarpStatusAsync();
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckWarpStatusAsync()
    {
        try
        {
            var statusPsi = new ProcessStartInfo
            {
                FileName = GetWarpCliPath(),
                Arguments = "status",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var statusProcess = Process.Start(statusPsi);
            if (statusProcess == null) return false;

            string output = await statusProcess.StandardOutput.ReadToEndAsync();
            string error = await statusProcess.StandardError.ReadToEndAsync();
            await statusProcess.WaitForExitAsync();

            return (output + error).Contains("Connected", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> TestWorkerWithRetryAsync(int maxRetries)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            LogMessage?.Invoke($"🔄 Worker attempt {i + 1}/{maxRetries}...", "info");

            try
            {
                var response = await _httpClient.GetAsync($"{WorkerUrl}/?url=https://example.com");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Length > 100)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"⚠️ Worker error: {ex.Message}", "warning");
            }

            if (i < maxRetries - 1)
                await Task.Delay(2000);
        }
        return false;
    }

    private async Task DisconnectWarpAsync()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = GetWarpCliPath(),
                Arguments = "disconnect",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            if (process != null)
                await process.WaitForExitAsync();
        }
        catch { }
    }

    public async Task DisconnectAsync()
    {
        await DisconnectWarpAsync();
        IsRunning = false;
    }
}