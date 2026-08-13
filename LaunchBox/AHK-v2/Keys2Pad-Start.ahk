#Requires AutoHotkey v2.0
#SingleInstance Force

; Change BridgeExe or place the EXE and script in the same folder.
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
