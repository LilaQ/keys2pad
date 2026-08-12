#Requires AutoHotkey v2.0
#SingleInstance Force

; Dieses Script als laufendes AutoHotkey-Script des Emulators verwenden.
; Es startet die Bridge sofort und stoppt sie beim normalen Script-Ende.
BridgeExe := A_ScriptDir "\XInput.KeyBridge.exe"
ProfileName := "Standard"

if !FileExist(BridgeExe) && FileExist(A_ScriptDir "\..\..\XInput.KeyBridge.exe")
    BridgeExe := A_ScriptDir "\..\..\XInput.KeyBridge.exe"

if !FileExist(BridgeExe) {
    MsgBox "XInput.KeyBridge.exe nicht gefunden:`n" BridgeExe, "LaunchBox / KeyBridge", 16
    ExitApp 2
}

RunWait '"' BridgeExe '" --start', , "Hide"
RunWait '"' BridgeExe '" --profile "' ProfileName '"', , "Hide"
OnExit StopKeyBridge

StopKeyBridge(*) {
    global BridgeExe
    if FileExist(BridgeExe)
        RunWait '"' BridgeExe '" --stop', , "Hide"
}
