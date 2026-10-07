using System.Diagnostics;
using Keys2Pad.Models;
using Keys2Pad.Services;

internal static class CoordinatorTests
{
    public static void Run() => RunAsync().GetAwaiter().GetResult();

    private static async Task RunAsync()
    {
        AppConfig config = AppConfig.CreateDefault();
        config.CurrentProfile.Players[0].Bindings.Clear();
        config.CurrentProfile.Players[0].Bindings[VirtualInput.A] = 1;
        Input input = new();
        Outputs output = new();
        using SlotCoordinator bridge = new(new Keyboard(), () => config, () => input, () => output, output);
        bridge.Start();
        Check(await bridge.WhenReady.WaitAsync(TimeSpan.FromSeconds(2)) is null, "Start waits until slots are ready");
        Check(output.Count == 4 && output.Report(0).Buttons == 0x1000, "Initial cabinet P1 report");
        input.Set(new PhysicalGamepad("pad:1", "Xbox", new GamepadState { Buttons = 8, LeftX = .5f }));
        await Until(() => output.Report(0).Buttons == 0x2000 && bridge.Status.Slots[0].Source == "Gamepad");
        Check(output.Report(0).LeftX > 16000 && output.Connections == 4, "Late controller takes P1 without reconnecting outputs");
        input.Set(new PhysicalGamepad("pad:2", "Replacement", new GamepadState { Buttons = 16 }));
        await Until(() => output.Report(0).Buttons == 0x4000);
        Check(output.Connections == 4, "Same-count replacement preserves outputs");
        input.Set();
        await Until(() => output.Report(0).Buttons == 0x1000 && bridge.Status.Slots[0].Source == "Cab keys");
        bridge.Rebuild();
        await Task.Delay(20);
        Check(output.Connections == 4, "Rescan/profile change preserves slots");
        config.CurrentProfile.Players[0].Enabled = false;
        await Until(() => output.Report(0) == default && bridge.Status.Slots[0].Source == "Keys off");
        bridge.Stop();
        Check(output.Disposed && input.Disposed && !bridge.Status.Enabled, "Stop releases devices and updates all player status");

        Input nativeInput = new();
        nativeInput.Set(new PhysicalGamepad("native:1", "Xbox", default));
        Outputs nativeOutput = new();
        nativeOutput.DirectSlot = 0;
        using SlotCoordinator native = new(new Keyboard(), () => config, () => nativeInput, () => nativeOutput, nativeOutput);
        native.Start();
        Check(await native.WhenReady.WaitAsync(TimeSpan.FromSeconds(2)) is null && nativeOutput.Count == 3,
            "Preconnected native P1 only needs three virtual controllers");
        nativeInput.Set();
        nativeOutput.DirectSlot = -1;
        await Until(() => nativeOutput.Count == 4 && native.Status.Slots[0].Source == "Keys off");
        nativeInput.Set(new PhysicalGamepad("native:2", "Xbox", new GamepadState { Buttons = 4 }));
        await Until(() => nativeOutput.Report(0).Buttons == 0x1000);
        Check(nativeOutput.Connections == 4, "Native controller reconnect proxies through its filled P1 slot");
        native.Stop();

        using SlotCoordinator failing = new(new Keyboard(), () => config,
            () => throw new InvalidOperationException("Runtime unavailable"), () => new Outputs(), new Outputs());
        failing.Start();
        Check(await failing.WhenReady.WaitAsync(TimeSpan.FromSeconds(2)) == "Runtime unavailable", "Startup failures reach the command client");
        await Until(() => failing.Status.Error == "Runtime unavailable" && !failing.Status.Enabled);
    }

    private static async Task Until(Func<bool> ready)
    {
        Stopwatch timeout = Stopwatch.StartNew();
        while (!ready())
        {
            if (timeout.Elapsed > TimeSpan.FromSeconds(2)) throw new InvalidOperationException("Coordinator condition timed out.");
            await Task.Delay(4);
        }
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Coordinator test failed: " + message);
    }
    private sealed class Keyboard : IKeyboardInput { public bool IsPressed(int key) => true; }
    private sealed class Input : IPhysicalControllerSource
    {
        private PhysicalGamepad[] _pads = [];
        public bool Disposed;
        public void Set(params PhysicalGamepad[] pads) => Volatile.Write(ref _pads, pads);
        public IReadOnlyList<PhysicalGamepad> Read() => Volatile.Read(ref _pads);
        public void Dispose() => Disposed = true;
    }
    private sealed class Outputs : IVirtualControllerService, IXInputSlots
    {
        private readonly object _sync = new();
        private readonly List<int> _slots = [];
        private readonly Dictionary<int, ControllerReport> _reports = [];
        public volatile int DirectSlot = -1;
        public int Connections;
        public bool Disposed;
        public int Count { get { lock (_sync) return _slots.Count; } }
        public int PendingCount => 0;
        public IReadOnlyList<int> Slots { get { lock (_sync) return _slots.ToArray(); } }
        public IReadOnlyList<int> ConnectedSlots() => Slots.Concat(DirectSlot >= 0 ? new[] { DirectSlot } : []).ToArray();
        public void ConnectOne()
        {
            lock (_sync)
            {
                // Deliberately differs from connection/list order.
                _slots.Add(new[] { 2, 0, 3, 1 }.First(i => !_slots.Contains(i) && i != DirectSlot));
                Connections++;
            }
        }
        public void Apply(IReadOnlyList<SlotRoute> routes, IReadOnlyList<PlayerConfig> players, IKeyboardInput keyboard)
        {
            lock (_sync)
                foreach (SlotRoute route in routes.Where(r => !r.Direct))
                    _reports[route.Index] = route.Gamepad is { } pad
                        ? ControllerReport.FromGamepad(pad.State) : ControllerReport.FromKeyboard(players[route.Index], keyboard);
        }
        public ControllerReport Report(int slot) { lock (_sync) return _reports.GetValueOrDefault(slot); }
        public void Dispose() { lock (_sync) { Disposed = true; _slots.Clear(); } }
    }
}
