#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

; BridgeExe anpassen oder EXE und Script in denselben Ordner legen.
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
