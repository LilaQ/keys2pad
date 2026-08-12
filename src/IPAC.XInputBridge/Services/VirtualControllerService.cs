using IPAC.XInputBridge.Models;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace IPAC.XInputBridge.Services;

public sealed class VirtualControllerService : IDisposable
{
    private readonly ViGEmClient _client;
    private readonly List<IXbox360Controller> _controllers = [];

    public VirtualControllerService()
    {
        _client = new ViGEmClient();
    }

    public int Count => _controllers.Count;

    public void Connect(int count)
    {
        DisconnectAll();
        for (int i = 0; i < Math.Clamp(count, 0, 4); i++)
        {
            IXbox360Controller controller = _client.CreateXbox360Controller();
            controller.AutoSubmitReport = false;
            controller.Connect();
            _controllers.Add(controller);
        }
    }

    public void DisconnectAll()
    {
        foreach (IXbox360Controller controller in _controllers)
        {
            controller.Disconnect();
        }

        _controllers.Clear();
    }

    public void Apply(IReadOnlyList<PlayerConfig> virtualPlayers, IKeyboardInput keyboard)
    {
        int count = Math.Min(_controllers.Count, virtualPlayers.Count);
        for (int i = 0; i < count; i++)
        {
            Apply(_controllers[i], virtualPlayers[i], keyboard);
        }
    }

    private static void Apply(IXbox360Controller controller, PlayerConfig player, IKeyboardInput keyboard)
    {
        bool Pressed(VirtualInput input) => player.Enabled
            && player.Bindings.TryGetValue(input, out int key)
            && keyboard.IsPressed(key);

        short Axis(VirtualInput negative, VirtualInput positive)
        {
            bool n = Pressed(negative);
            bool p = Pressed(positive);
            return n == p ? (short)0 : n ? short.MinValue : short.MaxValue;
        }

        controller.SetAxisValue(Xbox360Axis.LeftThumbX, Axis(VirtualInput.LeftStickLeft, VirtualInput.LeftStickRight));
        controller.SetAxisValue(Xbox360Axis.LeftThumbY, Axis(VirtualInput.LeftStickDown, VirtualInput.LeftStickUp));
        controller.SetAxisValue(Xbox360Axis.RightThumbX, Axis(VirtualInput.RightStickLeft, VirtualInput.RightStickRight));
        controller.SetAxisValue(Xbox360Axis.RightThumbY, Axis(VirtualInput.RightStickDown, VirtualInput.RightStickUp));
        controller.SetSliderValue(Xbox360Slider.LeftTrigger, Pressed(VirtualInput.LeftTrigger) ? byte.MaxValue : byte.MinValue);
        controller.SetSliderValue(Xbox360Slider.RightTrigger, Pressed(VirtualInput.RightTrigger) ? byte.MaxValue : byte.MinValue);

        Set(controller, Xbox360Button.Up, Pressed(VirtualInput.DPadUp));
        Set(controller, Xbox360Button.Down, Pressed(VirtualInput.DPadDown));
        Set(controller, Xbox360Button.Left, Pressed(VirtualInput.DPadLeft));
        Set(controller, Xbox360Button.Right, Pressed(VirtualInput.DPadRight));
        Set(controller, Xbox360Button.A, Pressed(VirtualInput.A));
        Set(controller, Xbox360Button.B, Pressed(VirtualInput.B));
        Set(controller, Xbox360Button.X, Pressed(VirtualInput.X));
        Set(controller, Xbox360Button.Y, Pressed(VirtualInput.Y));
        Set(controller, Xbox360Button.LeftShoulder, Pressed(VirtualInput.LeftShoulder));
        Set(controller, Xbox360Button.RightShoulder, Pressed(VirtualInput.RightShoulder));
        Set(controller, Xbox360Button.Back, Pressed(VirtualInput.Back));
        Set(controller, Xbox360Button.Start, Pressed(VirtualInput.Start));
        Set(controller, Xbox360Button.LeftThumb, Pressed(VirtualInput.LeftThumb));
        Set(controller, Xbox360Button.RightThumb, Pressed(VirtualInput.RightThumb));
        controller.SubmitReport();
    }

    private static void Set(IXbox360Controller controller, Xbox360Button button, bool pressed) =>
        controller.SetButtonState(button, pressed);

    public void Dispose()
    {
        DisconnectAll();
        _client.Dispose();
    }
}
