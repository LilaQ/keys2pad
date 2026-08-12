#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

; Als laufendes AutoHotkey-Script des Emulators verwenden.
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
OnExit, StopIPACBridge
return

StopIPACBridge:
IfExist, %BridgeExe%
    RunWait, "%BridgeExe%" --stop,, Hide
ExitApp
