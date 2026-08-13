#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

; Use as the emulator's running AutoHotkey script.
BridgeExe := A_ScriptDir . "\Keys2Pad.exe"
ProfileName := "Default"

IfNotExist, %BridgeExe%
{
    PackageExe := A_ScriptDir . "\..\..\Keys2Pad.exe"
    IfExist, %PackageExe%
        BridgeExe := PackageExe
}

IfNotExist, %BridgeExe%
{
    MsgBox, 16, LaunchBox / Keys2Pad, Keys2Pad.exe was not found:`n%BridgeExe%
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
