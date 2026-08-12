#Requires AutoHotkey v2.0
#SingleInstance Force

; Dieses Script als laufendes AutoHotkey-Script des Emulators verwenden.
; Es startet die Bridge sofort und stoppt sie beim normalen Script-Ende.
BridgeExe := A_ScriptDir "\IPAC.XInputBridge.exe"
ProfileName := "Arcade Standard"

if !FileExist(BridgeExe) && FileExist(A_ScriptDir "\..\..\IPAC.XInputBridge.exe")
    BridgeExe := A_ScriptDir "\..\..\IPAC.XInputBridge.exe"

if !FileExist(BridgeExe) {
    MsgBox "IPAC.XInputBridge.exe nicht gefunden:`n" BridgeExe, "LaunchBox / IPAC", 16
    ExitApp 2
}

RunWait '"' BridgeExe '" --start', , "Hide"
RunWait '"' BridgeExe '" --profile "' ProfileName '"', , "Hide"
OnExit StopIPACBridge

StopIPACBridge(*) {
    global BridgeExe
    if FileExist(BridgeExe)
        RunWait '"' BridgeExe '" --stop', , "Hide"
}
