# LaunchBox + XInput KeyBridge

Enthalten sind getrennte Start-/Stop-Skripte und ein kombiniertes `While-Game`-Skript für AutoHotkey v1 und v2. Die Skripte funktionieren unabhängig davon, ob die Eingaben von einer normalen Tastatur oder einem Arcade-Encoder kommen.

1. Den passenden AHK-Ordner wählen.
2. Im mitgelieferten Paket wird die EXE automatisch zwei Ebenen oberhalb gefunden. Nach dem Kopieren in einen anderen LaunchBox-Ordner `XInput.KeyBridge.exe` neben das Script legen oder `BridgeExe` auf den installierten Pfad ändern.
3. Falls gewünscht `ProfileName` anpassen.
4. `KeyBridge-Start.ahk` als Startup-Script und `KeyBridge-Stop.ahk` als Shutdown-Script eintragen. Alternativ `KeyBridge-While-Game.ahk` als laufendes Emulator-Script nutzen.

Wichtig: Manche LaunchBox-/Emulator-Setups beenden laufende AHK-Prozesse hart. In diesem Fall ist das getrennte Shutdown-Script zuverlässiger als `OnExit` im kombinierten Script.
