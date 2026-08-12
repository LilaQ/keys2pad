using System.Runtime.InteropServices;

namespace IPAC.XInputBridge.Services;

public interface IKeyboardInput
{
    bool IsPressed(int virtualKey);
}

public sealed class KeyboardInput : IKeyboardInput
{
    public bool IsPressed(int virtualKey) => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);
}
