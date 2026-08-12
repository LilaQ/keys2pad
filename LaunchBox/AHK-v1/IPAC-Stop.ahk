#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

BridgeExe := A_ScriptDir . "\IPAC.XInputBridge.exe"
IfNotExist, %BridgeExe%
{
    PackageExe := A_ScriptDir . "\..\..\IPAC.XInputBridge.exe"
    IfExist, %PackageExe%
        BridgeExe := PackageExe
}
IfExist, %BridgeExe%
    RunWait, "%BridgeExe%" --stop,, Hide
