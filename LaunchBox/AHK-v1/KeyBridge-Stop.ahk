#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

BridgeExe := A_ScriptDir . "\XInput.KeyBridge.exe"
IfNotExist, %BridgeExe%
{
    PackageExe := A_ScriptDir . "\..\..\XInput.KeyBridge.exe"
    IfExist, %PackageExe%
        BridgeExe := PackageExe
}
IfExist, %BridgeExe%
    RunWait, "%BridgeExe%" --stop,, Hide
