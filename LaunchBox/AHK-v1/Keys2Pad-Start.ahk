#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

; Change BridgeExe or place the EXE and script in the same folder.
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
