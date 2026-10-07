using System.Runtime.InteropServices;

namespace Keys2Pad.Services;

[StructLayout(LayoutKind.Sequential)]
public struct GamepadState
{
    // GameInput v2 button mask and normalized analog values.
    public uint Buttons;
    public float LeftTrigger, RightTrigger, LeftX, LeftY, RightX, RightY;
}

public sealed record PhysicalGamepad(string Id, string Name, GamepadState State, bool ReadingAvailable = true, bool NativeXInputEligible = true);

public interface IPhysicalControllerSource : IDisposable
{
    IReadOnlyList<PhysicalGamepad> Read();
}

public interface IXInputSlots
{
    IReadOnlyList<int> ConnectedSlots();
}

public sealed class XInputSlots : IXInputSlots
{
    public IReadOnlyList<int> ConnectedSlots() => Enumerable.Range(0, 4)
        .Where(i => XInputGetState((uint)i, out _) == 0).ToArray();

    [StructLayout(LayoutKind.Sequential)]
    private struct State
    {
        public uint Packet;
        public ushort Buttons;
        public byte LeftTrigger, RightTrigger;
        public short LeftX, LeftY, RightX, RightY;
    }

    [DllImport("xinput1_4.dll")]
    private static extern uint XInputGetState(uint index, out State state);
}
