# Keys2Pad

**Turn keyboard keys into Xbox controllers on Windows.**

Keys2Pad is made for arcade cabinets, button boxes, regular keyboards and encoders such as the I-PAC. Map any key to up to four virtual Xbox 360 controllers. Real gamepads take priority over cabinet keys in their assigned player slots, including when connected during a game.

[![Download](https://img.shields.io/github/v/release/LilaQ/keys2pad?include_prereleases&label=Download&style=for-the-badge)](https://github.com/LilaQ/keys2pad/releases/tag/v0.4.1)
[![Windows](https://img.shields.io/badge/Windows-10%20%2F%2011-2674d9?style=for-the-badge&logo=windows)](https://github.com/LilaQ/keys2pad/releases/tag/v0.4.1)
[![Buy me a beer](https://img.shields.io/badge/Buy_me_a_beer-FFDD00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=000)](https://buymeacoffee.com/lilaq)

The window shows the live input source for every player P1–P4, alongside a controller diagram and all key bindings.

## Why it is useful

- map every button freely, including both sticks, L3/R3, D-pad, triggers and Guide;
- use any mix of real and virtual controllers across P1–P4;
- create profiles and switch them from the GUI, command line or LaunchBox;
- run quietly in the tray and optionally start with Windows;
- use the included AutoHotkey v1 and v2 scripts directly in LaunchBox;
- no accounts, telemetry, ads, or outbound network communication.

Keys2Pad preserves physical XInput slots already assigned at startup and fills empty slots with virtual controllers. Later gamepads are read through Microsoft GameInput, including in the background, and forwarded to the lowest available virtual player slots. Cabinet keys pause for those players. Disconnecting a forwarded gamepad restores the cabinet keys; the virtual controller remains connected to the game. Profile changes and rescans also preserve these virtual devices.

When an initially native gamepad disconnects, its now-empty slot needs a replacement virtual device. Games must support that first device change. Connecting and disconnecting gamepads thereafter uses the persistent virtual device. Other controller emulators can occupy XInput slots; the UI reports Windows' actual assigned slots. Forwarded physical gamepads currently provide buttons, sticks and triggers; vibration and the Guide system button are not forwarded.

## Install

1. Install the official signed [ViGEmBus 1.22.0 release](https://github.com/nefarius/ViGEmBus/releases/tag/v1.22.0). The archived driver is required and is not installed silently by Keys2Pad.
2. Unpack the supplied Keys2Pad package and install its signed Microsoft `GameInputRedist.msi` once if GameInput is missing or outdated. This is a Windows runtime installation, separate from Keys2Pad; the app does not install it silently.
3. Start `Keys2Pad.exe`, select a controller input, and press the keyboard key you want.
4. Check the result with `Win+R` → `joy.cpl`.

## Command line

```text
Keys2Pad.exe --start
Keys2Pad.exe --stop
Keys2Pad.exe --toggle
Keys2Pad.exe --status
Keys2Pad.exe --profile "Default"
Keys2Pad.exe --show
Keys2Pad.exe --hide
Keys2Pad.exe --tray
Keys2Pad.exe --watch-process "game.exe"
Keys2Pad.exe --exit
```

LaunchBox examples are in [`LaunchBox/AHK-v2`](LaunchBox/AHK-v2) and [`LaunchBox/AHK-v1`](LaunchBox/AHK-v1).

## Build

On Windows with the .NET 8 SDK:

```powershell
./scripts/build.ps1
```

## Notes

- Keyboard input is digital: sticks and triggers output 0 or 100 percent. Physical gamepads retain their analog values.
- `--tray` and LaunchBox command starts create no visible app window; `--show` opens it explicitly.
- `--start` waits for usable player slots and returns a failure exit code if initialization fails.
- ViGEmBus is archived and no longer maintained.
- The current release is unsigned and marked as a prerelease.

Support and bug reports: [GitHub Issues](https://github.com/LilaQ/keys2pad/issues)

## Troubleshooting

`Keys2Pad.log` is written beside the executable and replaced whenever a new app session starts. If that folder is not writable, it is written under `%LOCALAPPDATA%\Keys2Pad`. It records startup, commands, controller counts and errors, but never key presses. CLI helpers append to the running session rather than clearing its log.
