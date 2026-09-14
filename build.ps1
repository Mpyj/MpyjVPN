# MpyjVPN Build Script
# Usage: .\build.ps1

param(
    [string]$Configuration = "Debug",
    [switch]$SkipCli,
    [switch]$SkipAvalonia
)

$ErrorActionPreference = "Stop"
$rootPath = $PSScriptRoot
$startTime = Get-Date

Write-Host ""
Write-Host "=== MpyjVPN Build Script ===" -ForegroundColor Cyan
Write-Host "    Configuration: $Configuration" -ForegroundColor Cyan
Write-Host ""

# STEP 1: Publish CLI
if (-not $SkipCli) {
    Write-Host "[1/3] Publishing MpyjVPN.CLI..." -ForegroundColor Yellow
    
    $cliPath = Join-Path $rootPath "src\MpyjVPN.CLI"
    Push-Location $cliPath
    
    try {
        dotnet publish -c $Configuration -r win-x64 --self-contained false -o publish
        
        if ($LASTEXITCODE -ne 0) {
            throw "CLI publish failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "   [OK] CLI published" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
    
    # STEP 2: Copy CLI to Avalonia
    Write-Host ""
    Write-Host "[2/3] Copying CLI files..." -ForegroundColor Yellow
    
    $src = Join-Path $rootPath "src\MpyjVPN.CLI\publish"
    $dest = Join-Path $rootPath "src\MpyjVPN.Avalonia\bin\$Configuration\net10.0\cli"
    
    if (!(Test-Path $dest)) {
        New-Item -ItemType Directory -Path $dest -Force | Out-Null
    }
    
    $filesToCopy = @(
        "MpyjCLI.exe",
        "MpyjCLI.dll",
        "MpyjCore.dll",
        "MpyjCLI.deps.json",
        "MpyjCLI.runtimeconfig.json"
    )
    
    foreach ($file in $filesToCopy) {
        $sourcePath = Join-Path $src $file
        if (Test-Path $sourcePath) {
            Copy-Item $sourcePath $dest -Force
            Write-Host "   [OK] $file" -ForegroundColor Gray
        }
        else {
            Write-Host "   [!!] $file not found" -ForegroundColor DarkYellow
        }
    }
    
    $criticalFiles = @("xray.exe", "geoip.dat", "geosite.dat")
    foreach ($file in $criticalFiles) {
        $destFile = Join-Path $dest $file
        if (!(Test-Path $destFile)) {
            $searchPaths = @(
                "C:\Users\parsa\OneDrive\Desktop\پارسا\v2rayN-windows-64\bin\xray\$file",
                "C:\Users\parsa\OneDrive\Desktop\پارسا\v2rayN-windows-64\bin\$file",
                "C:\Users\parsa\OneDrive\Desktop\پارسا\fragment_fingerprint_v1\$file"
            )
            
            $found = $false
            foreach ($searchPath in $searchPaths) {
                if (Test-Path $searchPath) {
                    Copy-Item $searchPath $dest -Force
                    Write-Host "   [OK] $file (from backup)" -ForegroundColor Gray
                    $found = $true
                    break
                }
            }
            
            if (-not $found) {
                Write-Host "   [!!] $file NOT FOUND" -ForegroundColor DarkYellow
            }
        }
        else {
            Write-Host "   [OK] $file (exists)" -ForegroundColor Gray
        }
    }
    
    Write-Host "   [OK] CLI files copied" -ForegroundColor Green
}

# STEP 3: Build Solution
if (-not $SkipAvalonia) {
    Write-Host ""
    Write-Host "[3/3] Building MpyjVPN Solution..." -ForegroundColor Yellow
    
    Push-Location $rootPath
    
    try {
        dotnet build MpyjVPN.slnx -c $Configuration
        
        if ($LASTEXITCODE -ne 0) {
            throw "Solution build failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "   [OK] Solution built" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

$elapsed = (Get-Date) - $startTime

Write-Host ""
Write-Host "=== BUILD COMPLETE ===" -ForegroundColor Green
Write-Host ""
Write-Host "Time: $($elapsed.TotalSeconds.ToString('F1'))s" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run:" -ForegroundColor Yellow
Write-Host "   cd src\MpyjVPN.Avalonia" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
