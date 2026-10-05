param([Parameter(Mandatory = $true)][string]$ProfileFile, [string]$WatchProcess)

$ErrorActionPreference = 'Stop'
$toolDirectory = Split-Path -Parent $PSScriptRoot
$logFile = Join-Path $toolDirectory 'launch.log'
[System.IO.File]::WriteAllText($logFile, '')
function Write-LaunchLog([string]$message) {
    Add-Content -Path $logFile -Value ((Get-Date -Format o) + ' ' + $message)
}
Write-LaunchLog ('Launch started; cwd=' + (Get-Location).Path)
try {
$bridge = Join-Path $toolDirectory 'Keys2Pad.exe'
$profile = Get-Content $ProfileFile -Raw | ConvertFrom-Json
if ([string]::IsNullOrWhiteSpace($profile.Name)) { throw 'Profile has no name.' }
$directory = Join-Path $env:LOCALAPPDATA 'Keys2Pad'
$configFile = Join-Path $directory 'config.json'
Write-LaunchLog ('Bridge=' + $bridge + '; config=' + $configFile)
$driver = Get-Service -Name ViGEmBus -ErrorAction SilentlyContinue
Write-LaunchLog ('ViGEmBus service=' + $(if ($driver) { $driver.Status } else { 'not found' }))
New-Item -ItemType Directory -Path $directory -Force | Out-Null

if (Test-Path $configFile) {
    $config = Get-Content $configFile -Raw | ConvertFrom-Json
} else {
    $config = [pscustomobject]@{
        ActiveProfile = $profile.Name
        StartEnabled = $false
        MinimizeToTray = $true
        PollIntervalMs = 8
        Profiles = @()
    }
}

$profiles = @($config.Profiles | Where-Object { $_.Name -ne $profile.Name }) + @($profile)
$old = $config | ConvertTo-Json -Depth 20 -Compress
$config.Profiles = $profiles
$new = $config | ConvertTo-Json -Depth 20 -Compress
if ($old -ne $new -or -not (Test-Path $configFile)) {
    $exitProcess = Start-Process -FilePath $bridge -ArgumentList '--exit' -PassThru
    $exitProcess.WaitForExit()
    Get-Process -Name Keys2Pad -ErrorAction SilentlyContinue | Wait-Process -Timeout 5 -ErrorAction SilentlyContinue
    if (Test-Path $configFile) {
        Copy-Item $configFile ($configFile + '.backup-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    }
    $tempFile = $configFile + '.tmp'
    [System.IO.File]::WriteAllText($tempFile, ($config | ConvertTo-Json -Depth 20), [System.Text.UTF8Encoding]::new($false))
    Move-Item $tempFile $configFile -Force
}
Write-LaunchLog ('Selecting profile ' + $profile.Name)
$command = Start-Process -FilePath $bridge -ArgumentList ('--profile "' + $profile.Name.Replace('"', '\"') + '"') -PassThru
$command.WaitForExit()
Write-LaunchLog ('Profile command exit=' + $command.ExitCode)
if ($command.ExitCode -ne 0) { throw 'Profile selection failed.' }
$command = Start-Process -FilePath $bridge -ArgumentList '--start' -PassThru
$command.WaitForExit()
Write-LaunchLog ('Start command exit=' + $command.ExitCode)
if ($command.ExitCode -ne 0) { throw 'Bridge start command failed.' }
Start-Sleep -Milliseconds 1000
$command = Start-Process -FilePath $bridge -ArgumentList '--status' -PassThru
$command.WaitForExit()
Write-LaunchLog ('Status command exit=' + $command.ExitCode + '; see Keys2Pad.log for response.')
if ($WatchProcess) {
    $command = Start-Process -FilePath $bridge -ArgumentList ('--watch-process "' + $WatchProcess + '"') -PassThru
    $command.WaitForExit()
    if ($command.ExitCode -ne 0) { throw 'Game process watch failed.' }
    Write-LaunchLog ('Watching game process ' + $WatchProcess)
}
Write-LaunchLog 'Launch setup completed.'
} catch {
    Write-LaunchLog ('FAILED: ' + $_.Exception.ToString() + '; ' + $_.ScriptStackTrace)
    exit 1
}
