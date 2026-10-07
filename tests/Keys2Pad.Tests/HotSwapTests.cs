using System.Reflection;
using System.Runtime.InteropServices;
using Keys2Pad.Models;
using Keys2Pad.Services;

internal static class HotSwapTests
{
    public static void Run()
    {
        SlotRouter router = new();
        int[] allSlots = [0, 1, 2, 3];
        Check(router.Route([], [], allSlots).All(r => r.Gamepad is null), "Cab keys on all four slots");
        PhysicalGamepad first = new("first:1", "Xbox Wireless", new GamepadState { Buttons = 4, LeftX = .75f });
        var routes = router.Route([first], [], allSlots);
        Check(routes[0].Gamepad == first && routes.Skip(1).All(r => r.Gamepad is null), "Late pad replaces P1 keys");
        PhysicalGamepad second = new("second:1", "Second Xbox", default);
        routes = router.Route([first, second], [], allSlots);
        Check(routes[0].Gamepad == first && routes[1].Gamepad == second, "Second pad becomes P2");
        routes = router.Route([second], [], allSlots);
        Check(routes[0].Gamepad == second && routes[1].Gamepad is null, "Disconnect promotes remaining pad and restores P2 keys");
        Check(router.Route([], [], allSlots).All(r => r.Gamepad is null), "Disconnect restores all cabinet inputs");
        for (int cycle = 0; cycle < 200; cycle++)
        {
            var replacement = first with { Id = $"first:{cycle + 2}" };
            Check(router.Route([replacement], [], allSlots)[0].Gamepad == replacement, "Repeated reconnect works");
            var swapped = second with { Id = $"second:{cycle + 2}" };
            Check(router.Route([swapped], [], allSlots)[0].Gamepad == swapped, "Equal count device replacement works");
            router.Route([], [], allSlots);
        }

        SlotRouter native = new();
        routes = native.Route([first], [0], [1, 2, 3]);
        Check(routes[0].Direct && routes.Skip(1).All(r => r.Gamepad is null), "Preconnected physical P1 is not duplicated");
        routes = native.Route([first, second], [0], [1, 2, 3]);
        Check(routes[0].Direct && routes[1].Gamepad == second, "Late second pad proxies into virtual P2");
        routes = native.Route([second], [], allSlots);
        Check(routes[0].Gamepad == second, "After direct P1 leaves, its replacement virtual P1 works");
        routes = native.Route([second, first with { Id = "first:2" }], [], allSlots);
        Check(routes[1].Gamepad?.Id == "first:2", "Returning original pad does not remain classified as direct");

        string hid = @"\\?\HID#VID_045E&PID_028E&IG_00#7&123&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
        Check(PhysicalControllerMonitor.DeviceInstanceIdFromPath(hid) == @"HID\VID_045E&PID_028E&IG_00\7&123&0&0000", "PnP interface path normalization");
        Check(PhysicalControllerMonitor.DeviceInstanceIdFromPath(@"USB\VID_045E&PID_028E\123") == @"USB\VID_045E&PID_028E\123", "PnP instance IDs preserved");
        Type info = typeof(GameInputControllerSource).GetNestedType("DeviceInfo", BindingFlags.NonPublic)!;
        Check(Marshal.OffsetOf(info, "DeviceId").ToInt32() == 26, "GameInput device ID ABI offset");
        Check(Marshal.OffsetOf(info, "DisplayName").ToInt32() == 128, "GameInput display name ABI offset");
        Check(Marshal.OffsetOf(info, "PnpPath").ToInt32() == 136, "GameInput PnP ABI offset");
        Check(Marshal.SizeOf<GamepadState>() == 28, "GameInput gamepad reading ABI size");

        ControllerReport report = ControllerReport.FromGamepad(first.State);
        Check(report.Buttons == 0x1000 && report.LeftX > 24000, "Physical button bit mask and analog stick");
        report = ControllerReport.FromGamepad(new GamepadState { LeftX = -1, LeftTrigger = 1, RightY = 1 });
        Check(report.Buttons == 0 && report.LeftX == short.MinValue, "New report releases held buttons and preserves negative range");
        Check(report.LeftTrigger == 255 && report.RightY == short.MaxValue, "Full trigger and positive analog range");
        report = ControllerReport.FromGamepad(new GamepadState { Buttons = 0x3FFF });
        Check(report.Buttons == 0xF3FF, "Every supported GameInput button maps to the corresponding XInput button");
        PlayerConfig player = new() { Enabled = true, Bindings = new() { [VirtualInput.B] = 1 } };
        report = ControllerReport.FromKeyboard(player, new TestKeyboard());
        Check(report == new ControllerReport(0x2000, 0, 0, 0, 0, 0, 0), "Keyboard fallback replaces entire physical report");
        player.Enabled = false;
        Check(ControllerReport.FromKeyboard(player, new TestKeyboard()) == default, "Disabled keys produce a neutral report");
        Check(ControllerReport.FromGamepad(new GamepadState { LeftX = float.NaN, RightTrigger = float.PositiveInfinity }) == default, "Invalid analog values remain neutral");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("HotSwap test failed: " + message);
    }

    private sealed class TestKeyboard : IKeyboardInput
    {
        public bool IsPressed(int key) => true;
    }
}
