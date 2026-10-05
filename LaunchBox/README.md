# LaunchBox + Keys2Pad

This folder includes separate start/stop scripts and a combined `While-Game` script for AutoHotkey v1 and v2. The scripts work whether input comes from a regular keyboard or an arcade encoder.

1. Choose the folder for your AutoHotkey version.
2. In the release package, the executable is found automatically two levels above the script. If you copy a script elsewhere in LaunchBox, place `Keys2Pad.exe` beside it or change `BridgeExe` to the installed path.
3. Change `ProfileName` if needed.
4. Add `Keys2Pad-Start.ahk` as the startup script and `Keys2Pad-Stop.ahk` as the shutdown script. Alternatively, use `Keys2Pad-While-Game.ahk` as the emulator's running script.

Important: some LaunchBox or emulator setups terminate running AHK processes forcefully. In that case, the separate shutdown script is more reliable than `OnExit` in the combined script.

For a supplied profile JSON, run `Start-Profile.ps1 -ProfileFile "path\to\profile.json"`. It backs up and merges that profile into the user configuration, waits for each CLI process separately, and writes `launch.log` beside Keys2Pad. It does not install ViGEmBus. Pass `-WatchProcess "emulator.exe"` to tie the bridge lifetime to that executable. The app attaches to the running process(es) and exits when they end, independently of LaunchBox scripts. If the game never starts, it exits after 60 seconds. For launchers, watch the actual game executable rather than the launcher. `--stop` only disables controllers; `--exit` also closes the tray app.
