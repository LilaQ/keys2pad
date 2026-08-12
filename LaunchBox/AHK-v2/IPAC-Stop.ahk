#Requires AutoHotkey v2.0
#SingleInstance Force

BridgeExe := A_ScriptDir "\IPAC.XInputBridge.exe"
if !FileExist(BridgeExe) && FileExist(A_ScriptDir "\..\..\IPAC.XInputBridge.exe")
    BridgeExe := A_ScriptDir "\..\..\IPAC.XInputBridge.exe"
if FileExist(BridgeExe)
    RunWait '"' BridgeExe '" --stop', , "Hide"
