#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

; Als laufendes AutoHotkey-Script des Emulators verwenden.
BridgeExe := A_ScriptDir . "\XInput.KeyBridge.exe"
ProfileName := "Standard"

IfNotExist, %BridgeExe%
{
    PackageExe := A_ScriptDir . "\..\..\XInput.KeyBridge.exe"
    IfExist, %PackageExe%
        BridgeExe := PackageExe
}

IfNotExist, %BridgeExe%
{
    MsgBox, 16, LaunchBox / KeyBridge, XInput.KeyBridge.exe nicht gefunden:`n%BridgeExe%
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
