using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MpyjVPN.Avalonia.Services;

public static class ToastService
{
    public static async Task ShowAsync(Window owner, string message, 
        string type = "info", int durationMs = 2500)
    {
        var (bg, icon) = type switch
        {
            "success" => ("#10b981", "✅"),
            "error" => ("#ef4444", "❌"),
            "warning" => ("#f59e0b", "⚠️"),
            _ => ("#00e5ff", "ℹ️")
        };

        var toast = new Border
        {
            Background = new SolidColorBrush(Color.Parse(bg)),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20, 12),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 20, 20, 0),
            Opacity = 0,
            BoxShadow = new BoxShadows(new BoxShadow
            {
                Blur = 20,
                Color = Color.Parse(bg),
                OffsetX = 0,
                OffsetY = 4
            }),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    new TextBlock 
                    { 
                        Text = icon, 
                        FontSize = 18, 
                        VerticalAlignment = VerticalAlignment.Center 
                    },
                    new TextBlock 
                    { 
                        Text = message, 
                        FontSize = 13, 
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeight.SemiBold
                    }
                }
            }
        };

        if (owner.Content is Panel panel)
        {
            panel.Children.Add(toast);

            // Fade in
            for (double o = 0; o <= 1; o += 0.1)
            {
                toast.Opacity = o;
                await Task.Delay(20);
            }

            await Task.Delay(durationMs);

            // Fade out
            for (double o = 1; o >= 0; o -= 0.1)
            {
                toast.Opacity = o;
                await Task.Delay(20);
            }

            panel.Children.Remove(toast);
        }
    }
}