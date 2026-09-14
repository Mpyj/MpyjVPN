using System;
using System.Collections.Generic;
using System.Linq;

namespace MpyjCore.Data;

public static class WhiteIPs
{
    // ==================== Cloudflare IPs ====================
    public static readonly List<string> CloudflareIPs = new List<string>
    {
        "104.16.0.1",
        "104.16.1.1",
        "104.16.2.1",
        "104.16.3.1",
        "104.17.0.1",
        "104.17.1.1",
        "104.17.2.1",
        "104.18.0.1",
        "104.18.1.1",
        "104.19.0.1",
        "104.20.0.1",
        "104.21.0.1",
        "104.22.0.1",
        "104.23.0.1",
        "104.24.0.1",
        "104.25.0.1",
        "104.26.0.1",
        "104.27.0.1",
        "172.64.0.1",
        "172.65.0.1",
        "172.66.0.1",
        "172.67.0.1",
        "172.68.0.1",
        "172.69.0.1",
        "172.70.0.1",
    };
    
    // ==================== Google IPs ====================
    public static readonly List<string> GoogleIPs = new List<string>
    {
        "142.250.0.1",
        "142.250.1.1",
        "142.250.2.1",
        "142.250.3.1",
        "142.251.0.1",
        "142.251.1.1",
        "142.251.2.1",
        "172.217.0.1",
        "172.217.1.1",
        "172.217.2.1",
        "216.58.192.1",
        "216.58.200.1",
        "216.58.201.1",
    };
    
    // ==================== Amazon IPs ====================
    public static readonly List<string> AmazonIPs = new List<string>
    {
        "3.0.0.1",
        "3.1.0.1",
        "3.2.0.1",
        "13.32.0.1",
        "13.35.0.1",
        "18.160.0.1",
        "18.164.0.1",
        "52.84.0.1",
        "52.94.0.1",
    };
    
    // ==================== Microsoft IPs ====================
    public static readonly List<string> MicrosoftIPs = new List<string>
    {
        "20.0.0.1",
        "20.190.0.1",
        "40.76.0.1",
        "40.112.0.1",
        "52.96.0.1",
        "52.97.0.1",
        "104.40.0.1",
        "104.43.0.1",
    };
    
    // ==================== ترکیب همه ====================
    public static readonly List<string> AllIPs = new List<string>()
        .Concat(CloudflareIPs)
        .Concat(GoogleIPs)
        .Concat(AmazonIPs)
        .Concat(MicrosoftIPs)
        .ToList();
    
    // ==================== متدهای کمکی ====================
    
    private static readonly Random _random = new Random();
    
    public static string GetRandomIP()
    {
        return AllIPs[_random.Next(AllIPs.Count)];
    }
    
    public static string GetRandomCloudflareIP()
    {
        return CloudflareIPs[_random.Next(CloudflareIPs.Count)];
    }
    
    public static string GetRandomGoogleIP()
    {
        return GoogleIPs[_random.Next(GoogleIPs.Count)];
    }
    
    public static string GetRandomAmazonIP()
    {
        return AmazonIPs[_random.Next(AmazonIPs.Count)];
    }
    
    public static string GetRandomMicrosoftIP()
    {
        return MicrosoftIPs[_random.Next(MicrosoftIPs.Count)];
    }
    
    public static List<string> GetIPsByService(string service)
    {
        return service.ToLower() switch
        {
            "cloudflare" => CloudflareIPs,
            "google" => GoogleIPs,
            "amazon" => AmazonIPs,
            "microsoft" => MicrosoftIPs,
            "all" => AllIPs,
            _ => AllIPs
        };
    }
    
    public static int TotalCount => AllIPs.Count;
    public static int CloudflareCount => CloudflareIPs.Count;
    public static int GoogleCount => GoogleIPs.Count;
    public static int AmazonCount => AmazonIPs.Count;
    public static int MicrosoftCount => MicrosoftIPs.Count;
}