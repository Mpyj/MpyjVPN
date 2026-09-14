using System.Diagnostics;
using System.Net.NetworkInformation;

namespace MpyjCore.Services;

public class DiagnosticsService
{
    public async Task<DiagnosticsResult> RunFullDiagnosticsAsync()
    {
        var result = new DiagnosticsResult();
        
        // Ping
        result.PingMs = await TestPingAsync();
        
        // DNS
        result.DnsWorking = await TestDnsAsync();
        
        // Internet
        result.InternetWorking = await TestInternetAsync();
        
        // Speed
        (result.DownloadMbps, result.UploadMbps) = await TestSpeedAsync();
        
        // Status
        if (!result.InternetWorking)
            result.StatusText = "🔴 No Internet";
        else if (result.PingMs < 100)
            result.StatusText = "🟢 Excellent";
        else if (result.PingMs < 200)
            result.StatusText = "🟡 Good";
        else
            result.StatusText = "🟡 Poor";
        
        return result;
    }
    
    private async Task<double> TestPingAsync()
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 3000);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : 999;
        }
        catch
        {
            return 999;
        }
    }
    
    private async Task<bool> TestDnsAsync()
    {
        try
        {
            var addresses = await System.Net.Dns.GetHostAddressesAsync("google.com");
            return addresses.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<bool> TestInternetAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync("https://google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<(double, double)> TestSpeedAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            
            // Download
            var sw = Stopwatch.StartNew();
            var data = await client.GetByteArrayAsync("https://speed.cloudflare.com/__down?bytes=1000000");
            sw.Stop();
            var downloadMbps = (1.0 * 8) / sw.Elapsed.TotalSeconds;
            
            return (downloadMbps, 0);
        }
        catch
        {
            return (0, 0);
        }
    }
}

public class DiagnosticsResult
{
    public double PingMs { get; set; }
    public bool DnsWorking { get; set; }
    public bool InternetWorking { get; set; }
    public double DownloadMbps { get; set; }
    public double UploadMbps { get; set; }
    public string StatusText { get; set; } = "";
}