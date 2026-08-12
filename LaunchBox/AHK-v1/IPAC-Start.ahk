#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

; BridgeExe anpassen oder EXE und Script in denselben Ordner legen.
BridgeExe := A_ScriptDir . "\IPAC.XInputBridge.exe"
ProfileName := "Arcade Standard"

IfNotExist, %BridgeExe%
{
    PackageExe := A_ScriptDir . "\..\..\IPAC.XInputBridge.exe"
    IfExist, %PackageExe%
        BridgeExe := PackageExe
}

IfNotExist, %BridgeExe%
{
    MsgBox, 16, LaunchBox / IPAC, IPAC.XInputBridge.exe nicht gefunden:`n%BridgeExe%
    ExitApp, 2
}

RunWait, "%BridgeExe%" --start,, Hide
RunWait, "%BridgeExe%" --profile "%ProfileName%",, Hide
