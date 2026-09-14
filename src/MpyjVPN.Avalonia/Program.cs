using Avalonia;
using System;
using System.IO;

namespace MpyjVPN.Avalonia;

internal class Program
{
    private static readonly string LogPath = Path.Combine(
        Path.GetTempPath(),
        "mpyj_crash.txt");

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            File.WriteAllText(LogPath, $"=== App starting ===\n{DateTime.Now}\n");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            File.AppendAllText(LogPath, "=== App closed ===\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(LogPath, $"=== CRASH ===\n{ex.ToString()}\n");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}