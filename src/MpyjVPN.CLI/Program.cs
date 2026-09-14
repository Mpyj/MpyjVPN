using System.Text;
using MpyjCore;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }
        
        var engine = new VpnEngine();
        engine.LogMessage += (msg, type) => Console.WriteLine($"[{type}] {msg}");
        
        try
        {
            switch (args[0].ToLower())
            {
                case "connect":
                    var mode = args.Length > 1 ? args[1] : "auto";
                    engine.CurrentMode = mode;
                    await engine.ConnectAsync();
                    break;
                    
                case "connect-layers":
                    var layers = new List<string>();
                    
                    if (args.Length > 1)
                    {
                        var combined = string.Join(",", args.Skip(1));
                        layers = combined
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .ToList();
                    }
                    
                    if (layers.Count == 0)
                        layers = new List<string> { "DNS", "WARP", "DoH", "Fragment" };
                    
                    await engine.ConnectOnionAsync(layers);
                    break;
                    
                case "disconnect":
                    await engine.DisconnectAsync();
                    break;
                    
                case "warp-in-warp":
                    await engine.ConnectWarpModeAsync("in-warp");
                    break;
                    
                case "warp-worker":
                    await engine.ConnectWarpModeAsync("worker");
                    break;
                    
                case "warp-full":
                    await engine.ConnectWarpModeAsync("full");
                    break;
                    
                case "config":
                    if (args.Length > 1)
                        await engine.ConnectWithConfigAsync(args[1]);
                    else
                        Console.WriteLine("Error: config URL/path required");
                    break;
                    
                case "config-layers":
                    if (args.Length > 1)
                    {
                        var configUrl = args[1];
                        var layerList = new List<string>();
                        
                        if (args.Length > 2)
                        {
                            var combined = string.Join(",", args.Skip(2));
                            layerList = combined
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .ToList();
                        }
                        
                        await engine.ConnectWithConfigAndLayersAsync(configUrl, layerList);
                    }
                    else
                    {
                        Console.WriteLine("Error: config URL/path required");
                    }
                    break;
                    
                case "scan":
                    var service = args.Length > 1 ? args[1] : "all";
                    await engine.ScanIPsAsync(service);
                    break;
                    
                case "scan-cache":
                    await engine.ScanAndCacheIPsAsync("cloudflare");
                    break;
                    
                case "diagnostics":
                    await engine.RunDiagnosticsAsync();
                    break;
                    
                case "perf":
                    var stats = engine.GetPerformanceStats();
                    Console.WriteLine("📊 Performance Stats:");
                    Console.WriteLine(stats.ToFullString());
                    break;
                    
                case "protocols":
                    Console.WriteLine("Available Protocols:");
                    foreach (var p in engine.GetProtocols())
                    {
                        Console.WriteLine($"  {p.Icon} {p.Name} - {p.Status}");
                    }
                    break;
                    
                case "layers":
                    Console.WriteLine("Layer Status:");
                    foreach (var layer in engine.LayerStatus)
                    {
                        Console.WriteLine($"  {(layer.Value ? "✅" : "❌")} {layer.Key}");
                    }
                    break;
                    
                case "modes":
                    Console.WriteLine("Available Modes:");
                    Console.WriteLine("  auto           - Auto Mode");
                    Console.WriteLine("  onion          - Onion Mode");
                    Console.WriteLine("  protocols      - Protocols Mode");
                    Console.WriteLine("  ultra          - Ultra Mode");
                    Console.WriteLine("  warp-in-warp   - WARP in WARP");
                    Console.WriteLine("  warp-worker    - WARP + Worker");
                    Console.WriteLine("  warp-full      - Full Stack");
                    break;
                    
                default:
                    Console.WriteLine($"Unknown command: {args[0]}");
                    PrintUsage();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    static void PrintUsage()
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║         MPYJ VPN CLI v1.0              ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine("");
        Console.WriteLine("Usage: MpyjCLI <command> [args]");
        Console.WriteLine("");
        Console.WriteLine("Commands:");
        Console.WriteLine("  connect [mode]              Connect with mode");
        Console.WriteLine("  connect-layers <names>      Connect with layers (comma-separated)");
        Console.WriteLine("  disconnect                  Disconnect");
        Console.WriteLine("  config <url>                Load config");
        Console.WriteLine("  config-layers <url> <names> Load config + layers");
        Console.WriteLine("  scan [service]              Scan IPs");
        Console.WriteLine("  scan-cache                  Scan and cache IPs");
        Console.WriteLine("  diagnostics                 Run diagnostics");
        Console.WriteLine("  perf                        Show performance stats");
        Console.WriteLine("  protocols                   List protocols");
        Console.WriteLine("  layers                      Show layer status");
        Console.WriteLine("  modes                       Show available modes");
        Console.WriteLine("");
        Console.WriteLine("Examples:");
        Console.WriteLine("  MpyjCLI connect auto");
        Console.WriteLine("  MpyjCLI connect-layers \"SNI Spoof,DNS Tunnel\"");
        Console.WriteLine("  MpyjCLI config \"vless://...\"");
    }
}
