namespace Keys2Pad.Services;

public sealed record PlayerSlotStatus(string Source, string Detail);
public sealed record SlotRoute(int Index, bool Direct, PhysicalGamepad? Gamepad);

/// <summary>
/// Preserve physical devices already assigned by Windows, and proxy later arrivals
/// through the remaining, persistent virtual slots. Membership is per connection,
/// so a controller reconnecting after a direct slot was filled becomes a proxy.
/// </summary>
public sealed class SlotRouter
{
    private readonly List<string> _directIds = [];
    private bool _initialized;

    public IReadOnlyList<SlotRoute> Route(IReadOnlyList<PhysicalGamepad> physical, IReadOnlyList<int> directSlots, IReadOnlyList<int> virtualSlots)
    {
        HashSet<string> present = physical.Select(p => p.Id).ToHashSet();
        _directIds.RemoveAll(id => !present.Contains(id));
        if (_directIds.Count > directSlots.Count)
            _directIds.RemoveRange(directSlots.Count, _directIds.Count - directSlots.Count);
        if (!_initialized || _directIds.Count < directSlots.Count)
        {
            foreach (PhysicalGamepad pad in physical.Where(p => p.NativeXInputEligible && !_directIds.Contains(p.Id)))
            {
                if (_directIds.Count >= directSlots.Count) break;
                _directIds.Add(pad.Id);
            }
            _initialized = true;
        }
        Queue<PhysicalGamepad> proxies = new(physical.Where(p => !_directIds.Contains(p.Id)));
        List<SlotRoute> routes = [];
        foreach (int slot in Enumerable.Range(0, 4))
        {
            if (directSlots.Contains(slot)) routes.Add(new SlotRoute(slot, true, null));
            else if (virtualSlots.Contains(slot)) routes.Add(new SlotRoute(slot, false, proxies.TryDequeue(out PhysicalGamepad? pad) ? pad : null));
        }
        return routes;
    }
}
