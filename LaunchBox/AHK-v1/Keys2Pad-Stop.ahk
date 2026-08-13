#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

BridgeExe := A_ScriptDir . "\Keys2Pad.exe"
IfNotExist, %BridgeExe%
{
    PackageExe := A_ScriptDir . "\..\..\Keys2Pad.exe"
    IfExist, %PackageExe%
        BridgeExe := PackageExe
}
IfExist, %BridgeExe%
    RunWait, "%BridgeExe%" --stop,, Hide
