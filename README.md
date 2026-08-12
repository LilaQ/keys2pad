# IPAC XInput Bridge

Eine native Windows-App, die Keyboard-Eingaben (z. B. von einem Ultimarc I-PAC) auf bis zu vier virtuelle Xbox-360-Controller abbildet. Echte XInput-Controller erhalten automatisch Vorrang.

## Verhalten der Controller-Slots

| Echte Controller | Virtuelle Controller | Belegung |
|---:|---:|---|
| 0 | 4 | P1–P4 virtuell |
| 1 | 3 | P1 echt, P2–P4 virtuell |
| 2 | 2 | P1–P2 echt, P3–P4 virtuell |
| 3 | 1 | P1–P3 echt, P4 virtuell |
| 4 | 0 | P1–P4 echt |

Windows weist XInput-Indizes selbst zu und stellt keine API zum direkten Setzen eines Slots bereit. Bei einer Änderung der echten Controller trennt die Bridge deshalb alle virtuellen Pads für etwa 650 ms und meldet nur die noch benötigten Geräte wieder an. So können bereits verbundene echte Controller zuerst die niedrigen Indizes belegen. Ein laufendes Spiel muss Hotplug unterstützen; für maximale Zuverlässigkeit Controller vor dem Spielstart einschalten.

## Funktionen

- vier unabhängig konfigurierbare P1–P4-Mappings
- Profile erstellen, duplizieren, löschen und per CLI wählen
- digitale Keyboard-Eingaben für beide Sticks, D-Pad, ABXY, Schultertasten, Trigger, Start/Back und Stick-Klicks
- automatische Erkennung echter Xbox/XInput-Pads, ViGEm-Geräte werden im Windows-Gerätebaum ausgeschlossen
- Tray-Icon, Minimieren ins Tray und optionaler Autostart pro Windows-Benutzer
- lokale JSON-Konfiguration unter `%LOCALAPPDATA%\IPAC XInput Bridge\config.json`
- AutoHotkey-v1- und -v2-Skripte für LaunchBox
- keine Konten, Telemetrie, Werbung oder Netzwerkkommunikation der App

## Voraussetzungen

- Windows 10 oder 11
- x64-Build (ARM64 kann über `scripts/build.ps1 -Runtime win-arm64` erstellt werden)
- [ViGEmBus 1.22.0 aus dem offiziellen, archivierten Projekt](https://github.com/nefarius/ViGEmBus/releases/tag/v1.22.0)

ViGEmBus wurde 2023 eingestellt und erhält keine Updates mehr. Die App installiert oder lädt den Kernel-Treiber absichtlich nicht automatisch. Nutze ausschließlich den signierten offiziellen Release und bewerte den Einsatz für dein System selbst.

## Installation

1. ViGEmBus aus dem offiziellen Release installieren (Administratorrechte werden nur hierfür benötigt).
2. Den Release-ZIP der Bridge entpacken.
3. `IPAC.XInputBridge.exe` starten.
4. In P1–P4 die jeweilige Aktion anklicken und anschließend die gewünschte IPAC-Taste drücken. `Entf`, `Backspace` oder `Esc` löscht eine Belegung.
5. Mit `Win+R` → `joy.cpl` die vier Xbox-360-Controller prüfen.

Die Standardbelegung nutzt gebräuchliche I-PAC-Tasten für P1/P2. Weil I-PAC-Konfigurationen variieren, sollte jede Installation die Belegung einmal prüfen. P3/P4 sind standardmäßig leer.

## CLI

```text
IPAC.XInputBridge.exe --start
IPAC.XInputBridge.exe --stop
IPAC.XInputBridge.exe --toggle
IPAC.XInputBridge.exe --status
IPAC.XInputBridge.exe --profile "Arcade Standard"
IPAC.XInputBridge.exe --show
IPAC.XInputBridge.exe --hide
IPAC.XInputBridge.exe --tray
IPAC.XInputBridge.exe --exit
```

Die erste Instanz stellt einen nur für den aktuellen Windows-Benutzer zugänglichen Named-Pipe-Endpunkt bereit. Weitere CLI-Aufrufe steuern diese Instanz. `--start` startet die GUI bei Bedarf; Befehle wie `--stop` liefern Exitcode 2, wenn die App nicht läuft.

## LaunchBox / AutoHotkey

Fertige Skripte liegen unter [`LaunchBox/AHK-v2`](LaunchBox/AHK-v2) und [`LaunchBox/AHK-v1`](LaunchBox/AHK-v1). Details stehen in [`LaunchBox/README.md`](LaunchBox/README.md).

## Bauen

Mit .NET 8 SDK auf Windows:

```powershell
./scripts/build.ps1
```

Das selbstenthaltende Paket landet unter `artifacts/IPAC-XInput-Bridge-win-x64`. Alternativ baut der enthaltene GitHub-Actions-Workflow auf einem Windows-Runner.

## Bekannte Grenzen

- XInput-Slotnummern sind laut Microsoft automatisch und nicht direkt änderbar. Das Trennen/Neuverbinden ist daher eine Best-Effort-Strategie des Betriebssystems.
- Spiele ohne Controller-Hotplug sollten erst nach der endgültigen Controller-Auswahl gestartet werden.
- Die Eingabe ist digital: Keyboard-Tasten erzeugen 0/100 % Stick- bzw. Triggerwerte.
- Die Erkennung zählt aktive Windows-XUSB-Geräteknoten. Exotische XInput-Wrapper oder Remote-/Streaming-Treiber können eine manuelle Anpassung erfordern.

## Datenschutz und Veröffentlichung

Die ausführlichen lokalen Hinweise stehen in der App unter **Hilfe → Rechtliches & Compliance** sowie unter [`docs/PRIVACY.de.md`](docs/PRIVACY.de.md) und [`docs/THIRD-PARTY-NOTICES.md`](docs/THIRD-PARTY-NOTICES.md). Vor öffentlicher Distribution sind die in [`docs/RELEASE-COMPLIANCE.md`](docs/RELEASE-COMPLIANCE.md) gekennzeichneten Eigentümerentscheidungen und Release-Blocker zu schließen.
