using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MpyjCore.Protocols;

public class SNISpoofProtocol : IProtocol
{
    public string Name => "SNI Spoof";
    public string Icon => "🎭";
    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Inactive;
    
    public Task<bool> ConnectAsync() => Task.FromResult(true);
    public Task<bool> DisconnectAsync() => Task.FromResult(true);
    
    public async Task<bool> TestAsync()
    {
        try
        {
            // تست SNI Spoof با اتصال به یک سایت
            var result = await TestSniSpoofAsync("google.com", "www.google.com");
            Status = result ? ProtocolStatus.Connected : ProtocolStatus.Inactive;
            return result;
        }
        catch
        {
            Status = ProtocolStatus.Inactive;
            return false;
        }
    }
    
    private async Task<bool> TestSniSpoofAsync(string targetHost, string sniHost)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(targetHost, 443);
            
            using var sslStream = new SslStream(
                client.GetStream(),
                false,
                (sender, cert, chain, errors) => true
            );
            
            // اینجا SNI فرق داره با host
            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = sniHost,  // SNI جعلی
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                                      System.Security.Authentication.SslProtocols.Tls13
            });
            
            return sslStream.IsAuthenticated;
        }
        catch
        {
            return false;
        }
    }
    
    public ProtocolResult GetResult()
    {
        return new ProtocolResult
        {
            Working = Status == ProtocolStatus.Connected,
            Score = Status == ProtocolStatus.Connected ? 80 : 0
        };
    }
}