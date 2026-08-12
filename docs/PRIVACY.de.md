# Datenschutzhinweis – XInput KeyBridge

Stand: 12. August 2026 · Entwicklungsfassung 0.2.0

## Verarbeitung

XInput KeyBridge arbeitet lokal auf dem Windows-PC. Die App verarbeitet:

- den momentanen gedrückt/nicht-gedrückt-Zustand der vom Benutzer konfigurierten Tastaturtasten;
- Windows-Geräteinformationen, die zur Zählung echter XInput-Controller und zum Ausschluss eigener ViGEm-Geräte nötig sind;
- vom Benutzer gewählte Profile, Tastenbelegungen und App-Einstellungen.

Konfigurationen werden im Benutzerprofil unter `%LOCALAPPDATA%\XInput KeyBridge\config.json` gespeichert. Die App enthält keine Konten, Cloud-Synchronisierung, Telemetrie, Werbung, Analyse-SDKs oder eigene Netzwerkkommunikation. Daten werden nicht von der App an Dritte übertragen.

## Berechtigungen und Löschung

Die App selbst läuft ohne Administratorrechte. Optional wird unter `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` ein Autostart-Eintrag angelegt. Er kann in der GUI wieder entfernt werden. Konfigurationsdaten können durch Löschen des oben genannten Ordners entfernt werden.

Der separat vom Benutzer installierte ViGEmBus-Treiber benötigt bei der Installation Administratorrechte. Dessen Installation und Lebenszyklus liegen außerhalb der App.

## Verantwortlicher und Kontakt

**RELEASE-BLOCKER:** Verantwortliche juristische/natürliche Person, ladungsfähige Kontaktangaben, Supportadresse und eigentümerfreigegebene öffentliche Datenschutz-URL wurden noch nicht bereitgestellt. Diese Entwicklungsfassung behauptet deshalb keinen Veröffentlichungsstatus.
