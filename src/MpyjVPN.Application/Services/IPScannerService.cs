using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MpyjVPN.Application.Services;

public class ScannedIP
{
    public string IP { get; set; } = "";
    public int Port { get; set; } = 443;
    public int Ping { get; set; } = 0;
    public bool IsWorking { get; set; } = true;

    public string Address => $"{IP}:{Port}";
}

public static class IPScannerService
{
    // الگو: ⚡ 104.18.1.1:443 (113ms)
    private static readonly Regex IPRegex = new(
        @"⚡\s+(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d+)\s+\((\d+)ms\)",
        RegexOptions.Compiled);

    public static List<ScannedIP> Parse(string cliOutput)
    {
        var result = new List<ScannedIP>();

        if (string.IsNullOrWhiteSpace(cliOutput))
            return result;

        foreach (Match match in IPRegex.Matches(cliOutput))
        {
            try
            {
                var ip = match.Groups[1].Value;
                var port = int.Parse(match.Groups[2].Value);
                var ping = int.Parse(match.Groups[3].Value);

                result.Add(new ScannedIP
                {
                    IP = ip,
                    Port = port,
                    Ping = ping,
                    IsWorking = true
                });
            }
            catch { }
        }

        result.Sort((a, b) => a.Ping.CompareTo(b.Ping));

        return result;
    }
}
