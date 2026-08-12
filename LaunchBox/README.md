# LaunchBox + IPAC XInput Bridge

Enthalten sind getrennte Start-/Stop-Skripte und ein kombiniertes `While-Game`-Skript für AutoHotkey v1 und v2.

1. Den passenden AHK-Ordner wählen.
2. Im mitgelieferten Paket wird die EXE automatisch zwei Ebenen oberhalb gefunden. Nach dem Kopieren in einen anderen LaunchBox-Ordner `IPAC.XInputBridge.exe` neben das Script legen oder `BridgeExe` auf den installierten Pfad ändern.
3. Falls gewünscht `ProfileName` anpassen.
4. `IPAC-Start.ahk` als Startup-Script und `IPAC-Stop.ahk` als Shutdown-Script eintragen. Alternativ `IPAC-While-Game.ahk` als laufendes Emulator-Script nutzen.

Wichtig: Manche LaunchBox-/Emulator-Setups beenden laufende AHK-Prozesse hart. In diesem Fall ist das getrennte Shutdown-Script zuverlässiger als `OnExit` im kombinierten Script.
