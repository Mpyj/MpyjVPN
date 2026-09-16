using System.Diagnostics;

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

    // ==================== WARP AUTO-SETUP ====================

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

        return "warp-cli"; // fallback: از PATH
    }

    public async Task<bool> EnsureWarpReadyAsync()
    {
        try
        {
            var warpCli = GetWarpCliPath();

            if (!File.Exists(warpCli))
            {
                LogMessage?.Invoke("⚠️ WARP Client not found", "warning");
                return false;
            }

            LogMessage?.Invoke("🔧 Preparing WARP...", "info");

            // ۱. ثبت‌نام (اگه قبلاً انجام نشده)
            await RunWarpCliAsync(warpCli, "registration new");
            await Task.Delay(1000);

            // ۲. تغییر پروتکل به WireGuard (حل مشکل MASQUE تو ایران)
            await RunWarpCliAsync(warpCli, "tunnel protocol set WireGuard");
            await Task.Delay(500);

            // ۳. تنظیم حالت Traffic and DNS + DoH
            await RunWarpCliAsync(warpCli, "mode warp+doh");
            await Task.Delay(500);

            LogMessage?.Invoke("✅ WARP ready", "success");
            return true;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"⚠️ WARP setup failed: {ex.Message}", "warning");
            return false;
        }
    }

    private async Task RunWarpCliAsync(string cliPath, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = cliPath,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process == null) return;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(error))
                LogMessage?.Invoke($"  {error.Trim()}", "warning");
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"  CLI error: {ex.Message}", "warning");
        }
    }

    // ==================== CONNECT ====================

    public async Task<bool> ConnectWarpInWarpAsync()
    {
        LogMessage?.Invoke("🧅 Mode: WARP in WARP", "info");

        var ready = await EnsureWarpReadyAsync();
        if (!ready) return false;

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

        var ready = await EnsureWarpReadyAsync();
        if (!ready) return false;

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

        var ready = await EnsureWarpReadyAsync();
        if (!ready) return false;

        // لایه ۱: DNS
        LogMessage?.Invoke("🔧 Layer 1: DNS...", "info");
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