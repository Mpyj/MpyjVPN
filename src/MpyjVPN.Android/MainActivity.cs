using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.Platform;

namespace MpyjVPN.Mobile.Android;

[Activity(
    Label = "MpyjVPN",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<MpyjVPN.UI.App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .With(new AndroidPlatformOptions
            {
                // این خط مشکل صفحه سیاه رو حل می‌کنه
                RenderingMode = new[] { AndroidRenderingMode.Software }
            })
            .WithInterFont();
    }
}