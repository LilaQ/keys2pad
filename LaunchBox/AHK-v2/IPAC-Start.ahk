#Requires AutoHotkey v2.0
#SingleInstance Force

; BridgeExe anpassen oder EXE und Script in denselben Ordner legen.
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
