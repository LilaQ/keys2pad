param(
    [ValidateSet("win-x64", "win-arm64")]
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/Keys2Pad/Keys2Pad.csproj"
$output = Join-Path $root "artifacts/Keys2Pad-$Runtime"

dotnet restore $project -r $Runtime
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }
dotnet publish $project -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $output
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

Copy-Item (Join-Path $root "LaunchBox") $output -Recurse -Force
$docsOutput = Join-Path $output "docs"
New-Item -ItemType Directory -Path $docsOutput -Force | Out-Null
foreach ($document in @("PRIVACY.md", "THIRD-PARTY-NOTICES.md", "licenses")) {
    Copy-Item (Join-Path $root ("docs/" + $document)) $docsOutput -Recurse -Force
}
$assets = Get-Content (Join-Path $root "src/Keys2Pad/obj/project.assets.json") -Raw | ConvertFrom-Json
$licensesOutput = Join-Path $docsOutput "licenses"
foreach ($framework in $assets.project.frameworks.PSObject.Properties.Value) {
    foreach ($package in $framework.downloadDependencies) {
        if ($package.name -notin @("Microsoft.NETCore.App.Runtime.$Runtime", "Microsoft.WindowsDesktop.App.Runtime.$Runtime")) { continue }
        $version = $package.version.Trim('[', ']').Split(',')[0].Trim()
        foreach ($folder in $assets.packageFolders.PSObject.Properties.Name) {
            $packageDirectory = Join-Path (Join-Path $folder $package.name.ToLowerInvariant()) $version
            foreach ($notice in @("LICENSE", "LICENSE.TXT", "THIRD-PARTY-NOTICES.TXT")) {
                $sourceNotice = Join-Path $packageDirectory $notice
                if (Test-Path $sourceNotice) {
                    $noticeName = $package.name + '-' + $notice + '.txt'
                    Copy-Item $sourceNotice (Join-Path $licensesOutput $noticeName) -Force
                }
            }
        }
    }
}
Copy-Item (Join-Path $root "README.md") $output -Force

Write-Host "Fertig: $output"
