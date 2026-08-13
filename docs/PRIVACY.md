# Privacy Notice — Keys2Pad

Last updated: August 13, 2026 · Prerelease 0.4.1

## Data handling

Keys2Pad works locally on the Windows PC. The app processes:

- the current pressed/not-pressed state of keyboard keys configured by the user;
- Windows device information required to count physical XInput controllers and exclude the app's own ViGEm devices;
- profiles, key mappings, and app settings selected by the user.

Configurations are stored in the user's profile under `%LOCALAPPDATA%\Keys2Pad\config.json`. The app has no accounts, cloud sync, telemetry, advertising, or analytics SDKs. It does not send data over the network or transmit data to third parties.

## Permissions and deletion

The app itself runs without administrator privileges. It can optionally create a startup entry under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`. This entry can be removed again in the GUI. Configuration data can be deleted by removing the folder named above.

The ViGEmBus driver, which the user installs separately, requires administrator privileges during installation. Its installation and lifecycle are outside the app.

## Controller and contact

Public version of this notice: https://github.com/LilaQ/keys2pad/blob/main/docs/PRIVACY.md

Support and bug reports: https://github.com/LilaQ/keys2pad/issues

**RELEASE BLOCKER:** The responsible legal or natural person, serviceable contact details, and a separate support address have not yet been provided. This prerelease therefore makes no claim of complete release compliance.
