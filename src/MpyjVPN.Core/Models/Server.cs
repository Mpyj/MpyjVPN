namespace MpyjCore.Models;

public class Server
{
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public string Flag { get; set; } = "";
    public string Ip { get; set; } = "";
    public int Load { get; set; }
    public int Ping { get; set; }
    public bool Premium { get; set; }
}

public class ServerList
{
    public List<Server> Servers { get; set; } = new();
    
    public ServerList()
    {
        LoadDefaultServers();
    }
    
    private void LoadDefaultServers()
    {
        Servers.Add(new Server { Name = "Germany", Flag = "🇩🇪", Ip = "85.214.1.10", Load = 35, Ping = 45 });
        Servers.Add(new Server { Name = "USA", Flag = "🇺🇸", Ip = "104.28.2.15", Load = 50, Ping = 120 });
        Servers.Add(new Server { Name = "UK", Flag = "🇬🇧", Ip = "51.89.3.20", Load = 40, Ping = 80 });
        Servers.Add(new Server { Name = "Netherlands", Flag = "🇳🇱", Ip = "45.12.4.25", Load = 25, Ping = 55 });
        Servers.Add(new Server { Name = "France", Flag = "🇫🇷", Ip = "37.59.5.30", Load = 60, Ping = 70, Premium = true });
        Servers.Add(new Server { Name = "Japan", Flag = "🇯🇵", Ip = "103.5.6.35", Load = 70, Ping = 150, Premium = true });
    }
}