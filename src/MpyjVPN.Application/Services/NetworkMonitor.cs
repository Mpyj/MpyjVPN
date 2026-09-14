using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace MpyjVPN.Application.Services;

public class NetworkMonitor : IDisposable
{
    private long _lastBytesReceived;
    private long _lastBytesSent;
    private DateTime _lastCheckTime;
    private Timer? _timer;
    private bool _isRunning;
    private int _consecutiveFails = 0;

    public event Action<double, double>? SpeedUpdated;
    public event Action<int>? PingUpdated;
    public event Action<double>? LossUpdated;

    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;
        _consecutiveFails = 0;

        var (rx, tx) = GetTotalBytes();
        _lastBytesReceived = rx;
        _lastBytesSent = tx;
        _lastCheckTime = DateTime.Now;

        _timer = new Timer(OnTick, null, 0, 1000);

        _ = PingLoopAsync();
    }

    public void Stop()
    {
        _isRunning = false;
        _timer?.Dispose();
        _timer = null;
    }

    private void OnTick(object? state)
    {
        if (!_isRunning) return;

        try
        {
            var (rx, tx) = GetTotalBytes();
            var now = DateTime.Now;
            var elapsed = (now - _lastCheckTime).TotalSeconds;

            if (elapsed <= 0) return;

            var downloadBps = (rx - _lastBytesReceived) / elapsed;
            var uploadBps = (tx - _lastBytesSent) / elapsed;

            var downloadMBps = downloadBps / 1024.0 / 1024.0;
            var uploadMBps = uploadBps / 1024.0 / 1024.0;

            SpeedUpdated?.Invoke(
                Math.Max(0, downloadMBps),
                Math.Max(0, uploadMBps));

            _lastBytesReceived = rx;
            _lastBytesSent = tx;
            _lastCheckTime = now;
        }
        catch { }
    }

    private async Task PingLoopAsync()
    {
        // اولین بار صبر کن تا WARP راه بیفته
        await Task.Delay(3000);

        while (_isRunning)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("1.1.1.1", 3000);

                if (reply.Status == IPStatus.Success)
                {
                    _consecutiveFails = 0;
                    PingUpdated?.Invoke((int)reply.RoundtripTime);
                    LossUpdated?.Invoke(0);
                }
                else
                {
                    _consecutiveFails++;

                    // فقط اگه 3 بار متوالی fail شد، 100% اعلام کن
                    if (_consecutiveFails >= 3)
                    {
                        PingUpdated?.Invoke(-1);
                        LossUpdated?.Invoke(100);
                    }
                }
            }
            catch
            {
                _consecutiveFails++;

                if (_consecutiveFails >= 3)
                {
                    PingUpdated?.Invoke(-1);
                    LossUpdated?.Invoke(100);
                }
            }

            await Task.Delay(5000);
        }
    }

    private (long rx, long tx) GetTotalBytes()
    {
        long totalRx = 0;
        long totalTx = 0;

        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var stats = ni.GetIPv4Statistics();
                totalRx += stats.BytesReceived;
                totalTx += stats.BytesSent;
            }
        }
        catch { }

        return (totalRx, totalTx);
    }

    public void Dispose()
    {
        Stop();
    }
}
