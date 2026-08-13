#Requires AutoHotkey v2.0
#SingleInstance Force

BridgeExe := A_ScriptDir "\Keys2Pad.exe"
if !FileExist(BridgeExe) && FileExist(A_ScriptDir "\..\..\Keys2Pad.exe")
    BridgeExe := A_ScriptDir "\..\..\Keys2Pad.exe"
if FileExist(BridgeExe)
    RunWait '"' BridgeExe '" --stop', , "Hide"
