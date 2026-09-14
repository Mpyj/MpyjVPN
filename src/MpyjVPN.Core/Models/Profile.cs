namespace MpyjCore.Models;

public class ConnectionProfile
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Mode { get; set; } = "auto";
    public bool PreferLowLatency { get; set; }
    public string Dns { get; set; } = "Cloudflare";
}

public class ProfileManager
{
    public List<ConnectionProfile> Profiles { get; set; } = new();
    
    public ProfileManager()
    {
        LoadDefaultProfiles();
    }
    
    private void LoadDefaultProfiles()
    {
        Profiles.Add(new ConnectionProfile 
        { 
            Name = "🎮 Gaming", 
            Description = "Low latency", 
            PreferLowLatency = true,
            Dns = "Cloudflare"
        });
        
        Profiles.Add(new ConnectionProfile 
        { 
            Name = "🌐 Browsing", 
            Description = "Balanced", 
            PreferLowLatency = false,
            Dns = "Google"
        });
        
        Profiles.Add(new ConnectionProfile 
        { 
            Name = "🛡️ Privacy", 
            Description = "Max security", 
            Mode = "ultra",
            Dns = "Quad9"
        });
    }
}