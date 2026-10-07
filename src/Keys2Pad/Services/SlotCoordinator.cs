using Keys2Pad.Models;

namespace Keys2Pad.Services;

public sealed record BridgeStatus(bool Enabled, int PhysicalControllers, int VirtualControllers, string Profile, string? Error = null)
{
    public IReadOnlyList<PlayerSlotStatus> Slots { get; init; } = Enumerable.Range(0, 4)
        .Select(_ => new PlayerSlotStatus("Stopped", "Bridge is off")).ToArray();
}

public sealed class SlotCoordinator : IDisposable
{
    private readonly Func<IPhysicalControllerSource> _createSource;
    private readonly Func<IVirtualControllerService> _createVirtual;
    private readonly IXInputSlots _xinput;
    private readonly IKeyboardInput _keyboard;
    private readonly Func<AppConfig> _getConfig;
    private readonly object _sync = new();
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private TaskCompletionSource<string?> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task<string?> WhenReady => _ready.Task;

    public SlotCoordinator(IKeyboardInput keyboard, Func<AppConfig> getConfig,
        Func<IPhysicalControllerSource>? createSource = null,
        Func<IVirtualControllerService>? createVirtual = null, IXInputSlots? xinput = null)
    {
        _createSource = createSource ?? (() => new GameInputControllerSource());
        _createVirtual = createVirtual ?? (() => new VirtualControllerService());
        _xinput = xinput ?? new XInputSlots();
        _keyboard = keyboard;
        _getConfig = getConfig;
    }

    public event EventHandler<BridgeStatus>? StatusChanged;
    public BridgeStatus Status { get; private set; } = new(false, 0, 0, "-");

    public void Start()
    {
        lock (_sync)
        {
            if (_loop is { IsCompleted: false }) return;
            _cts?.Dispose();
            _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            RuntimeLog.Write("Bridge start requested.");
            Publish(new BridgeStatus(true, 0, 0, _getConfig().ActiveProfile)
            {
                Slots = Enumerable.Range(0, 4).Select(_ => new PlayerSlotStatus("Starting", "Preparing controller input")).ToArray()
            });
            _loop = Task.Run(() => RunAsync(token));
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            RuntimeLog.Write("Bridge stop requested.");
            _cts?.Cancel();
            // The loop owns its devices and disposes them before completing. Never
            // dispose underneath a still-running input/report or connect operation.
            _loop?.GetAwaiter().GetResult();
            _loop = null;
            _cts?.Dispose();
            _cts = null;
            Publish(new BridgeStatus(false, Status.PhysicalControllers, 0, _getConfig().ActiveProfile));
        }
    }

    // Profiles and rescans must not tear down controllers seen by a running game.
    public void Rebuild() => RuntimeLog.Write("Controller rescan/profile refresh requested; preserving connected slots.");

    private async Task RunAsync(CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            using IPhysicalControllerSource source = _createSource();
            using IVirtualControllerService output = _createVirtual();
            SlotRouter router = new();
            IReadOnlyList<PhysicalGamepad> physical = source.Read();
            IReadOnlyList<int> connected = _xinput.ConnectedSlots();
            router.Route(physical, connected, []);
            // Existing physical slots keep native input. Empty slots become virtual
            // and stay connected through subsequent source changes.
            for (int i = connected.Count; i < 4; i++)
            {
                token.ThrowIfCancellationRequested();
                output.ConnectOne();
            }
            DateTime pendingSince = DateTime.UtcNow;
            while (!token.IsCancellationRequested)
            {
                AppConfig config = _getConfig();
                physical = source.Read();
                connected = _xinput.ConnectedSlots();
                IReadOnlyList<int> virtualSlots = output.Slots;
                if (output.PendingCount > 0)
                {
                    if (DateTime.UtcNow - pendingSince > TimeSpan.FromSeconds(5))
                        throw new InvalidOperationException("Windows did not assign all virtual controllers to XInput slots. Check other controller emulators and restart the bridge.");
                    Publish(new BridgeStatus(true, physical.Count, output.Count, config.ActiveProfile)
                    {
                        Slots = Enumerable.Range(0, 4).Select(_ => new PlayerSlotStatus("Connecting", "Waiting for Windows slots")).ToArray()
                    });
                }
                else
                {
                    if (virtualSlots.Distinct().Count() != output.Count)
                        throw new InvalidOperationException("Windows reported duplicate virtual player slots. Check other controller emulators and restart the bridge.");
                    int[] directSlots = connected.Except(virtualSlots).ToArray();
                    if (output.Count + directSlots.Length < 4)
                    {
                        output.ConnectOne();
                        pendingSince = DateTime.UtcNow;
                        continue;
                    }
                    IReadOnlyList<SlotRoute> routes = router.Route(physical, directSlots, virtualSlots);
                    output.Apply(routes, config.CurrentProfile.Players, _keyboard);
                    PlayerSlotStatus[] slots = Enumerable.Range(0, 4).Select(index =>
                    {
                        SlotRoute? route = routes.FirstOrDefault(r => r.Index == index);
                        if (route is null) return new PlayerSlotStatus("Disconnected", "Waiting for Windows");
                        if (route.Direct) return new PlayerSlotStatus("Gamepad", $"Direct · XInput {index + 1}");
                        if (route.Gamepad is { } pad) return new PlayerSlotStatus("Gamepad", pad.ReadingAvailable ? pad.Name : "Waiting for gamepad input");
                        PlayerConfig player = config.CurrentProfile.Players[index];
                        return player.Enabled
                            ? new PlayerSlotStatus("Cab keys", player.Bindings.Count == 0 ? "No keys assigned" : $"{player.Bindings.Count} bindings · XInput {index + 1}")
                            : new PlayerSlotStatus("Keys off", "Cabinet input disabled");
                    }).ToArray();
                    Publish(new BridgeStatus(true, physical.Count, output.Count, config.ActiveProfile) { Slots = slots });
                    _ready.TrySetResult(null);
                }
                await Task.Delay(Math.Clamp(config.PollIntervalMs, 4, 50), token).ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { _ready.TrySetResult("Bridge stopped while starting."); }
        catch (Exception ex)
        {
            _ready.TrySetResult(ex.Message);
            RuntimeLog.Write($"Bridge failure: {ex}");
            Publish(new BridgeStatus(false, Status.PhysicalControllers, 0, _getConfig().ActiveProfile, ex.Message));
        }
    }

    private void Publish(BridgeStatus status)
    {
        if (Status.Enabled == status.Enabled && Status.PhysicalControllers == status.PhysicalControllers
            && Status.VirtualControllers == status.VirtualControllers && Status.Profile == status.Profile
            && Status.Error == status.Error && Status.Slots.SequenceEqual(status.Slots)) return;
        RuntimeLog.Write($"Bridge status: enabled={status.Enabled}; physical={status.PhysicalControllers}; virtual={status.VirtualControllers}; profile={status.Profile}; slots={string.Join(" | ", status.Slots.Select((s, i) => $"P{i + 1}={s.Source} ({s.Detail})"))}; error={status.Error ?? "none"}");
        Status = status;
        StatusChanged?.Invoke(this, status);
    }

    public void Dispose() => Stop();
}
