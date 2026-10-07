using Keys2Pad.Models;

namespace Keys2Pad.Services;

public readonly record struct ControllerReport(ushort Buttons, byte LeftTrigger, byte RightTrigger,
    short LeftX, short LeftY, short RightX, short RightY)
{
    public static ControllerReport FromGamepad(GamepadState state)
    {
        // GameInput v2 and XInput use different button positions.
        uint buttons = ((state.Buttons & 0x03) << 4) | ((state.Buttons & 0x3C) << 10)
            | ((state.Buttons & 0x3C0) >> 6) | ((state.Buttons & 0xC00) >> 2) | ((state.Buttons & 0x3000) >> 6);
        return new((ushort)buttons, Trigger(state.LeftTrigger), Trigger(state.RightTrigger),
            Axis(state.LeftX), Axis(state.LeftY), Axis(state.RightX), Axis(state.RightY));
    }

    public static ControllerReport FromKeyboard(PlayerConfig player, IKeyboardInput keyboard)
    {
        bool Pressed(VirtualInput input) => player.Enabled && player.Bindings.TryGetValue(input, out int key) && keyboard.IsPressed(key);
        short Stick(VirtualInput negative, VirtualInput positive)
        {
            bool n = Pressed(negative), p = Pressed(positive);
            return n == p ? (short)0 : n ? short.MinValue : short.MaxValue;
        }
        ushort buttons = 0;
        void Button(VirtualInput input, ushort mask) { if (Pressed(input)) buttons |= mask; }
        Button(VirtualInput.DPadUp, 1); Button(VirtualInput.DPadDown, 2);
        Button(VirtualInput.DPadLeft, 4); Button(VirtualInput.DPadRight, 8);
        Button(VirtualInput.Start, 0x10); Button(VirtualInput.Back, 0x20);
        Button(VirtualInput.LeftThumb, 0x40); Button(VirtualInput.RightThumb, 0x80);
        Button(VirtualInput.LeftShoulder, 0x100); Button(VirtualInput.RightShoulder, 0x200);
        Button(VirtualInput.Guide, 0x400);
        Button(VirtualInput.A, 0x1000); Button(VirtualInput.B, 0x2000);
        Button(VirtualInput.X, 0x4000); Button(VirtualInput.Y, 0x8000);
        return new(buttons, Pressed(VirtualInput.LeftTrigger) ? (byte)255 : (byte)0,
            Pressed(VirtualInput.RightTrigger) ? (byte)255 : (byte)0,
            Stick(VirtualInput.LeftStickLeft, VirtualInput.LeftStickRight), Stick(VirtualInput.LeftStickDown, VirtualInput.LeftStickUp),
            Stick(VirtualInput.RightStickLeft, VirtualInput.RightStickRight), Stick(VirtualInput.RightStickDown, VirtualInput.RightStickUp));
    }

    private static short Axis(float value) => float.IsFinite(value)
        ? (short)Math.Round(Math.Clamp(value, -1f, 1f) * (value < 0 ? 32768f : 32767f)) : (short)0;
    private static byte Trigger(float value) => float.IsFinite(value)
        ? (byte)Math.Round(Math.Clamp(value, 0f, 1f) * 255f) : (byte)0;
}
