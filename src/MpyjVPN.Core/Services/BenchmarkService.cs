using System.Diagnostics;
using MpyjCore.Protocols;

namespace MpyjCore.Services;

public class BenchmarkService
{
    private readonly List<IProtocol> _protocols;
    
    public BenchmarkService(List<IProtocol> protocols)
    {
        _protocols = protocols;
    }
    
    public async Task<Dictionary<string, BenchmarkResult>> RunBenchmarkAsync()
    {
        var results = new Dictionary<string, BenchmarkResult>();
        
        var tasks = _protocols.Select(async protocol =>
        {
            var sw = Stopwatch.StartNew();
            var working = await protocol.TestAsync();
            sw.Stop();
            
            results[protocol.Name] = new BenchmarkResult
            {
                Working = working,
                PingMs = sw.ElapsedMilliseconds,
                Score = working ? CalculateScore(sw.ElapsedMilliseconds) : 0
            };
        });
        
        await Task.WhenAll(tasks);
        
        return results;
    }
    
    private int CalculateScore(double pingMs)
    {
        if (pingMs < 50) return 90;
        if (pingMs < 100) return 80;
        if (pingMs < 200) return 65;
        if (pingMs < 500) return 50;
        return 30;
    }
    
    public string GetBestProtocol(Dictionary<string, BenchmarkResult> results)
    {
        if (results.Count == 0) return "None";
        
        return results
            .Where(r => r.Value.Working)
            .OrderByDescending(r => r.Value.Score)
            .FirstOrDefault().Key ?? "None";
    }
}

public class BenchmarkResult
{
    public bool Working { get; set; }
    public double PingMs { get; set; }
    public int Score { get; set; }
}