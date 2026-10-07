# Privacy Notice — Keys2Pad

Last updated: October 7, 2026 · Prerelease 0.5.0

## Data handling

Keys2Pad works locally on the Windows PC. The app processes:

- the current pressed/not-pressed state of keyboard keys configured by the user;
- Windows controller identities and slot information required to distinguish physical gamepads from virtual ViGEm devices;
- the current buttons, sticks and triggers of connected physical gamepads, to forward them into virtual player slots;
- profiles, key mappings, and app settings selected by the user.

Configurations are stored in the user's profile under `%LOCALAPPDATA%\Keys2Pad\config.json`. The app has no accounts, cloud sync, telemetry, advertising, or analytics SDKs. It does not send data over the network or transmit data to third parties.

Local diagnostics are written to `Keys2Pad.log` beside the executable, or under `%LOCALAPPDATA%\Keys2Pad` if the executable folder is not writable. The log records app version, process ID, working/configuration paths, profile names, commands, controller counts, device display names, input-source changes and errors. Paths may contain the Windows account name. It never records key presses or gamepad button/axis readings. A new app session replaces the previous log; the file can also be deleted manually. Logs are not uploaded automatically.

## Permissions and deletion

The app itself runs without administrator privileges. It can optionally create a startup entry under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`. This entry can be removed again in the GUI. Configuration data can be deleted by removing the folder named above.

The ViGEmBus driver, which the user installs separately, requires administrator privileges during installation. Its installation and lifecycle are outside the app. Microsoft GameInput is also a separately installed Windows component governed by the Microsoft license included in `docs/licenses/Microsoft.GameInput.txt`; it may receive Microsoft updates. Keys2Pad does not install or update these components silently.

## Controller and contact

Public version of this notice: https://github.com/LilaQ/keys2pad/blob/main/docs/PRIVACY.md

Support and bug reports: https://github.com/LilaQ/keys2pad/issues

Support contact: info@emudev.de
