using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;

namespace MpyjVPN.Avalonia.Services;

public static class ThemeService
{
    public static void ApplyTheme(string theme)
    {
        if (global::Avalonia.Application.Current == null) return;
        
        var isLight = theme == "Light";
        
        // Theme Variant
        global::Avalonia.Application.Current.RequestedThemeVariant = isLight
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
        
        // Load resource dictionary
        var uri = isLight
            ? "avares://MpyjVPN.Avalonia/Styles/LightTheme.axaml"
            : "avares://MpyjVPN.Avalonia/Styles/DarkTheme.axaml";
        
        try
        {
            var dict = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(uri));
            
            if (global::Avalonia.Application.Current.Resources.MergedDictionaries.Count > 0)
                global::Avalonia.Application.Current.Resources.MergedDictionaries.Clear();
            
            global::Avalonia.Application.Current.Resources.MergedDictionaries.Add(dict);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Theme load error: {ex.Message}");
        }
    }
}