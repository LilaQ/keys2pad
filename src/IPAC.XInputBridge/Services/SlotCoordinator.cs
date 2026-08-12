using IPAC.XInputBridge.Models;

namespace IPAC.XInputBridge.Services;

public sealed record BridgeStatus(bool Enabled, int PhysicalControllers, int VirtualControllers, string Profile, string? Error = null);

public sealed class SlotCoordinator : IDisposable
{
    private readonly IPhysicalControllerCounter _physicalCounter;
    private readonly IKeyboardInput _keyboard;
    private readonly Func<AppConfig> _getConfig;
    private readonly object _sync = new();
    private VirtualControllerService? _virtualControllers;
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private int _physicalCount = -1;
    private bool _enabled;

    public SlotCoordinator(IPhysicalControllerCounter physicalCounter, IKeyboardInput keyboard, Func<AppConfig> getConfig)
    {
        _physicalCounter = physicalCounter;
        _keyboard = keyboard;
        _getConfig = getConfig;
    }

    public event EventHandler<BridgeStatus>? StatusChanged;

    public BridgeStatus Status { get; private set; } = new(false, 0, 0, "-");

    public void Start()
    {
        lock (_sync)
        {
            if (_enabled)
            {
                return;
            }

            _enabled = true;
            _physicalCount = -1;
            _cts = new CancellationTokenSource();
            _loop = Task.Run(() => RunAsync(_cts.Token));
        }
    }

    public void Stop()
    {
        Task? loop;
        lock (_sync)
        {
            _enabled = false;
            _cts?.Cancel();
            loop = _loop;
        }

        try { loop?.Wait(TimeSpan.FromSeconds(2)); } catch (AggregateException) { }

        lock (_sync)
        {
            _virtualControllers?.Dispose();
            _virtualControllers = null;
            Publish(new BridgeStatus(false, Math.Max(0, _physicalCount), 0, _getConfig().ActiveProfile));
        }
    }

    public void Rebuild() => _physicalCount = -1;

    private async Task RunAsync(CancellationToken token)
    {
        DateTime nextDeviceScan = DateTime.MinValue;
        try
        {
            while (!token.IsCancellationRequested)
            {
                AppConfig config = _getConfig();
                if (DateTime.UtcNow >= nextDeviceScan)
                {
                    int count = _physicalCounter.CountPhysicalXInputControllers();
                    if (count != _physicalCount)
                    {
                        _physicalCount = count;
                        _virtualControllers?.DisconnectAll();

                        // Give Windows enough time to release old XInput indices, then
                        // let already connected hardware claim the lowest free indices.
                        await Task.Delay(650, token).ConfigureAwait(false);
                        int stableCount = _physicalCounter.CountPhysicalXInputControllers();
                        _physicalCount = stableCount;
                        _virtualControllers ??= new VirtualControllerService();
                        _virtualControllers.Connect(4 - stableCount);
                    }

                    nextDeviceScan = DateTime.UtcNow.AddSeconds(1);
                }

                int physical = Math.Clamp(_physicalCount, 0, 4);
                List<PlayerConfig> virtualPlayers = config.CurrentProfile.Players.Skip(physical).Take(4 - physical).ToList();
                _virtualControllers?.Apply(virtualPlayers, _keyboard);
                Publish(new BridgeStatus(true, physical, _virtualControllers?.Count ?? 0, config.ActiveProfile));
                await Task.Delay(Math.Clamp(config.PollIntervalMs, 4, 50), token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            try { _virtualControllers?.Dispose(); } catch { }
            _virtualControllers = null;
            Publish(new BridgeStatus(false, Math.Max(0, _physicalCount), 0, _getConfig().ActiveProfile, ex.Message));
            lock (_sync) { _enabled = false; }
        }
    }

    private void Publish(BridgeStatus status)
    {
        if (Status == status)
        {
            return;
        }

        Status = status;
        StatusChanged?.Invoke(this, status);
    }

    public void Dispose() => Stop();
}
