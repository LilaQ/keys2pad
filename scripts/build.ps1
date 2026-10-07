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
dotnet publish $project -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $output

Copy-Item (Join-Path $root "LaunchBox") $output -Recurse -Force
$docsOutput = Join-Path $output "docs"
New-Item -ItemType Directory -Path $docsOutput -Force | Out-Null
foreach ($document in @("PRIVACY.md", "THIRD-PARTY-NOTICES.md", "licenses")) {
    Copy-Item (Join-Path $root ("docs/" + $document)) $docsOutput -Recurse -Force
}
$assets = Get-Content (Join-Path $root "src/Keys2Pad/obj/project.assets.json") -Raw | ConvertFrom-Json
$licensesOutput = Join-Path $docsOutput "licenses"
foreach ($library in $assets.libraries.PSObject.Properties) {
    if ($library.Name -like "Microsoft.NETCore.App.Runtime.$Runtime/*" -or
        $library.Name -like "Microsoft.WindowsDesktop.App.Runtime.$Runtime/*") {
        foreach ($folder in $assets.packageFolders.PSObject.Properties.Name) {
            $packageDirectory = Join-Path $folder $library.Value.path
            foreach ($notice in @("LICENSE", "LICENSE.TXT", "THIRD-PARTY-NOTICES.TXT")) {
                $sourceNotice = Join-Path $packageDirectory $notice
                if (Test-Path $sourceNotice) {
                    $noticeName = $library.Name.Split('/')[0] + '-' + $notice + '.txt'
                    Copy-Item $sourceNotice (Join-Path $licensesOutput $noticeName) -Force
                }
            }
        }
    }
}
Copy-Item (Join-Path $root "README.md") $output -Force

Write-Host "Fertig: $output"
