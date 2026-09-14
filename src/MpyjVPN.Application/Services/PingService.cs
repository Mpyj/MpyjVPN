using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace MpyjVPN.Application.Services;

public static class PingService
{
    /// <summary>
    /// Ù¾ÛŒÙ†Ú¯ Ø¨Ù‡ ÛŒÙ‡ Ø¢Ø¯Ø±Ø³
    /// </summary>
    public static async Task<int> PingAsync(string address, int timeoutMs = 2000)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(address, timeoutMs);

            if (reply.Status == IPStatus.Success)
                return (int)reply.RoundtripTime;
        }
        catch { }

        return -1;
    }

    /// <summary>
    /// Ù¾ÛŒÙ†Ú¯ Ø¨Ù‡ ÛŒÙ‡ IP:Port (ÙÙ‚Ø· IP Ø±Ùˆ Ù¾ÛŒÙ†Ú¯ Ù…ÛŒâ€ŒÚ©Ù†Ù‡)
    /// </summary>
    public static async Task<int> PingAsync(string ip, int port, int timeoutMs = 2000)
    {
        return await PingAsync(ip, timeoutMs);
    }
}