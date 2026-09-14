# Read MainWindow.axaml with UTF8
$content = [System.IO.File]::ReadAllText("$PWD\MainWindow.axaml", [System.Text.Encoding]::UTF8)
$lines = $content -split "`r?`n"

# Extract Home section (lines 104-684)
$homeLines = $lines[103..683]
$homeContent = $homeLines -join "`r`n"

# Build HomeView.axaml
$homeView = @"
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:MpyjVPN.Avalonia.ViewModels"
             xmlns:local="clr-namespace:MpyjVPN.Avalonia.Controls"
             x:Class="MpyjVPN.Avalonia.Views.Home.HomeView"
             x:CompileBindings="False">

    <StackPanel Spacing="24">
$homeContent
    </StackPanel>

</UserControl>
"@

# Save HomeView
[System.IO.File]::WriteAllText("$PWD\Home\HomeView.axaml", $homeView, [System.Text.Encoding]::UTF8)

Write-Host "HomeView.axaml saved!" -ForegroundColor Green
Write-Host "Lines: $($homeLines.Count)" -ForegroundColor Cyan