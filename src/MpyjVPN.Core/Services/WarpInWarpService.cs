using System.Diagnostics;

namespace MpyjCore.Services;

public class WarpInWarpService
{
    public event Action<string, string>? LogMessage;

    public bool IsRunning { get; private set; }

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
            LogMessage?.Invoke("🧅 Starting WARP in WARP...", "info");

            await DisconnectWarpAsync();
            await Task.Delay(1000);

            LogMessage?.Invoke("🌐 Layer 1: Connecting WARP...", "info");
            var layer1 = await ConnectWarpWithRetryAsync(5);

            if (!layer1)
            {
                LogMessage?.Invoke("❌ Layer 1 failed after retries", "error");
                return false;
            }

            LogMessage?.Invoke("✅ Layer 1 connected", "success");
            await Task.Delay(2000);

            LogMessage?.Invoke("🌐 Layer 2: WARP over WARP...", "info");
            var layer2 = await ConnectWarpWithRetryAsync(3);

            if (!layer2)
            {
                LogMessage?.Invoke("⚠️ Layer 2 failed, using single WARP", "warning");
                IsRunning = true;
                return true;
            }

            IsRunning = true;
            LogMessage?.Invoke("✅ WARP in WARP active!", "success");
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"❌ WARP in WARP error: {ex.Message}", "error");
            return false;
        }
    }

    private async Task<bool> ConnectWarpWithRetryAsync(int maxRetries)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            LogMessage?.Invoke($"🔄 Attempt {i + 1}/{maxRetries}...", "info");

            var result = await ConnectWarpOnceAsync();
            if (result) return true;

            if (i < maxRetries - 1)
            {
                var delay = 2000 + (i * 1000);
                await Task.Delay(delay);
            }
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
            await Task.Delay(4000);

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

            var combined = output + error;

            if (combined.Contains("Connected", StringComparison.OrdinalIgnoreCase)
                && !combined.Contains("Connecting"))
            {
                LogMessage?.Invoke("✅ WARP Connected", "success");
                return true;
            }

            if (combined.Contains("Error") || combined.Contains("Failed"))
            {
                LogMessage?.Invoke($"⚠️ WARP: {combined.Trim()}", "warning");
            }

            return false;
        }
        catch
        {
            return false;
        }
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
        LogMessage?.Invoke("✅ WARP in WARP disconnected", "success");
    }
}