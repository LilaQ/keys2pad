#Requires AutoHotkey v2.0
#SingleInstance Force

BridgeExe := A_ScriptDir "\XInput.KeyBridge.exe"
if !FileExist(BridgeExe) && FileExist(A_ScriptDir "\..\..\XInput.KeyBridge.exe")
    BridgeExe := A_ScriptDir "\..\..\XInput.KeyBridge.exe"
if FileExist(BridgeExe)
    RunWait '"' BridgeExe '" --stop', , "Hide"
