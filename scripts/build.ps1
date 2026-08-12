param(
    [ValidateSet("win-x64", "win-arm64")]
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/IPAC.XInputBridge/IPAC.XInputBridge.csproj"
$output = Join-Path $root "artifacts/IPAC-XInput-Bridge-$Runtime"

dotnet restore $project -r $Runtime
dotnet publish $project -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $output

Copy-Item (Join-Path $root "LaunchBox") $output -Recurse -Force
Copy-Item (Join-Path $root "docs") $output -Recurse -Force
Copy-Item (Join-Path $root "README.md") $output -Force

Write-Host "Fertig: $output"
