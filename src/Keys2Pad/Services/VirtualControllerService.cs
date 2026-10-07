using Keys2Pad.Models;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Keys2Pad.Services;

public interface IVirtualControllerService : IDisposable
{
    int Count { get; }
    int PendingCount { get; }
    IReadOnlyList<int> Slots { get; }
    void ConnectOne();
    void Apply(IReadOnlyList<SlotRoute> routes, IReadOnlyList<PlayerConfig> players, IKeyboardInput keyboard);
}

public sealed class VirtualControllerService : IVirtualControllerService
{
    private readonly ViGEmClient _client;
    private readonly List<IXbox360Controller> _controllers = [];

    public VirtualControllerService()
    {
        RuntimeLog.Write("Opening ViGEm client.");
        _client = new ViGEmClient();
        RuntimeLog.Write("ViGEm client connected.");
    }

    public int Count => _controllers.Count;

    public IReadOnlyList<int> Slots => _controllers.Select(Index).Where(i => i >= 0).ToArray();
    public int PendingCount => _controllers.Count(c => Index(c) < 0);

    private static int Index(IXbox360Controller controller)
    {
        try { return controller.UserIndex is >= 0 and < 4 ? controller.UserIndex : -1; }
        catch (Nefarius.ViGEm.Client.Targets.Xbox360.Exceptions.Xbox360UserIndexNotReportedException) { return -1; }
    }

    public void ConnectOne()
    {
        IXbox360Controller controller = _client.CreateXbox360Controller();
        controller.AutoSubmitReport = false;
        try { controller.Connect(); }
        catch
        {
            try { controller.Disconnect(); } catch { }
            throw;
        }
        _controllers.Add(controller);
        controller.ResetReport();
        controller.SubmitReport();
        RuntimeLog.Write($"Virtual controller connected: {_controllers.Count}.");
    }

    public void DisconnectAll()
    {
        Exception? failure = null;
        foreach (IXbox360Controller controller in _controllers)
        {
            try { controller.ResetReport(); controller.SubmitReport(); }
            catch (Exception ex) { failure ??= ex; }
            try { controller.Disconnect(); }
            catch (Exception ex) { failure ??= ex; }
        }
        _controllers.Clear();
        if (failure is not null) throw new InvalidOperationException("Could not release every virtual controller cleanly.", failure);
    }

    public void Apply(IReadOnlyList<SlotRoute> routes, IReadOnlyList<PlayerConfig> players, IKeyboardInput keyboard)
    {
        foreach (IXbox360Controller controller in _controllers)
        {
            int slot = Index(controller);
            SlotRoute? route = routes.FirstOrDefault(r => r.Index == slot && !r.Direct);
            if (route?.Gamepad is { } pad) ApplyGamepad(controller, pad.State);
            else if (route is not null) Apply(controller, players[slot], keyboard);
            else
            {
                controller.ResetReport();
                controller.SubmitReport();
            }
        }
    }

    internal static void ApplyGamepad(IXbox360Controller controller, GamepadState state) => Submit(controller, ControllerReport.FromGamepad(state));
    internal static void Apply(IXbox360Controller controller, PlayerConfig player, IKeyboardInput keyboard) => Submit(controller, ControllerReport.FromKeyboard(player, keyboard));

    private static void Submit(IXbox360Controller controller, ControllerReport report)
    {
        controller.ResetReport();
        controller.SetButtonsFull(report.Buttons);
        controller.SetAxisValue(Xbox360Axis.LeftThumbX, report.LeftX);
        controller.SetAxisValue(Xbox360Axis.LeftThumbY, report.LeftY);
        controller.SetAxisValue(Xbox360Axis.RightThumbX, report.RightX);
        controller.SetAxisValue(Xbox360Axis.RightThumbY, report.RightY);
        controller.SetSliderValue(Xbox360Slider.LeftTrigger, report.LeftTrigger);
        controller.SetSliderValue(Xbox360Slider.RightTrigger, report.RightTrigger);
        controller.SubmitReport();
    }

    public void Dispose()
    {
        try { DisconnectAll(); }
        finally { _client.Dispose(); }
    }
}
