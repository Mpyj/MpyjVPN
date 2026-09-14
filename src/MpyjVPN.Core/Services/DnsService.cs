using System.Diagnostics;

namespace MpyjCore.Services;

public class DnsService
{
    public async Task<bool> ChangeDnsAsync(string primaryDns, string secondaryDns)
    {
        var interfaceName = await GetActiveInterfaceAsync();
        
        var primaryOk = await ExecuteAsync(
            "netsh", 
            $"interface ip set dns \"{interfaceName}\" static {primaryDns}"
        );
        
        var secondaryOk = await ExecuteAsync(
            "netsh", 
            $"interface ip add dns \"{interfaceName}\" {secondaryDns} index=2"
        );
        
        return primaryOk;
    }
    
    public async Task<bool> ResetDnsAsync()
    {
        var interfaceName = await GetActiveInterfaceAsync();
        return await ExecuteAsync(
            "netsh", 
            $"interface ip set dns \"{interfaceName}\" dhcp"
        );
    }
    
    private async Task<string> GetActiveInterfaceAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "interface show interface",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                
                using var process = Process.Start(psi);
                if (process == null) return "WiFi";
                
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(3000);
                
                foreach (var line in output.Split('\n'))
                {
                    if (line.Contains("Connected"))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 4)
                        {
                            return string.Join(" ", parts.Skip(3));
                        }
                    }
                }
            }
            catch { }
            
            return "WiFi";
        });
    }
    
    private Task<bool> ExecuteAsync(string fileName, string args)
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                
                using var process = Process.Start(psi);
                return process?.WaitForExit(5000) ?? false;
            }
            catch
            {
                return false;
            }
        });
    }
}