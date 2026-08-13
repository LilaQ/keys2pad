#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

; Als laufendes AutoHotkey-Script des Emulators verwenden.
BridgeExe := A_ScriptDir . "\Keys2Pad.exe"
ProfileName := "Standard"

IfNotExist, %BridgeExe%
{
    PackageExe := A_ScriptDir . "\..\..\Keys2Pad.exe"
    IfExist, %PackageExe%
        BridgeExe := PackageExe
}

IfNotExist, %BridgeExe%
{
    MsgBox, 16, LaunchBox / Keys2Pad, Keys2Pad.exe nicht gefunden:`n%BridgeExe%
    ExitApp, 2
}

RunWait, "%BridgeExe%" --start,, Hide
RunWait, "%BridgeExe%" --profile "%ProfileName%",, Hide
OnExit, StopKeyBridge
return

StopKeyBridge:
IfExist, %BridgeExe%
    RunWait, "%BridgeExe%" --stop,, Hide
ExitApp
