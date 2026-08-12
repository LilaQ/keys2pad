#Requires AutoHotkey v2.0
#SingleInstance Force

; BridgeExe anpassen oder EXE und Script in denselben Ordner legen.
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
