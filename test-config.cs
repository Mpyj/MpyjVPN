using MpyjCore.Services;

var service = new ConfigService();
service.LogMessage += (msg, type) => Console.WriteLine($"[{type}] {msg}");

// یه کانفیگ VLess تستی
var vlessLink = "vless://YOUR-UUID@YOUR-SERVER:443?type=ws&security=tls&path=/&host=YOUR-HOST#Test";

Console.WriteLine("=== Testing LoadConfigAsync ===");
var result = await service.LoadConfigAsync(vlessLink);
Console.WriteLine($"Result: {result}");

Console.WriteLine("\n=== Config file path ===");
var configPath = Path.Combine(Path.GetTempPath(), "mpyj_xray_config.json");
Console.WriteLine($"Path: {configPath}");
Console.WriteLine($"Exists: {File.Exists(configPath)}");

if (File.Exists(configPath))
{
    Console.WriteLine("\n=== Config content ===");
    Console.WriteLine(await File.ReadAllTextAsync(configPath));
}
