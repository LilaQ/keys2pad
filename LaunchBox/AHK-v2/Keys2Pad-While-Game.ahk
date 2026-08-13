#Requires AutoHotkey v2.0
#SingleInstance Force

; Use this as the emulator's running AutoHotkey script.
; It starts the bridge immediately and stops it when the script exits normally.
BridgeExe := A_ScriptDir "\Keys2Pad.exe"
ProfileName := "Default"

if !FileExist(BridgeExe) && FileExist(A_ScriptDir "\..\..\Keys2Pad.exe")
    BridgeExe := A_ScriptDir "\..\..\Keys2Pad.exe"

if !FileExist(BridgeExe) {
    MsgBox "Keys2Pad.exe was not found:`n" BridgeExe, "LaunchBox / Keys2Pad", 16
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
