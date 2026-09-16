using Avalonia;
using System;

namespace MpyjVPN.Avalonia;

internal class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<MpyjVPN.UI.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}