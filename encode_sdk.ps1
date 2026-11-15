# Run this once on any machine that has PluginSDK.dll.
# Paste the output as a GitHub Actions secret named PLUGIN_SDK_B64.
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File encode_sdk.ps1 path\to\PluginSDK.dll

param(
    [Parameter(Mandatory=$true)]
    [string]$DllPath
)

if (-not (Test-Path $DllPath)) {
    Write-Error "File not found: $DllPath"
    exit 1
}

$bytes  = [System.IO.File]::ReadAllBytes($DllPath)
$b64    = [System.Convert]::ToBase64String($bytes)

Write-Host ""
Write-Host "=== PLUGIN_SDK_B64 secret value ==="
Write-Host $b64
Write-Host "=== END ==="
Write-Host ""
Write-Host "Paste the string above (without the === lines) as a GitHub Actions secret named PLUGIN_SDK_B64."
