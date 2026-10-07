using System.Runtime.InteropServices;
using System.Text;

namespace Keys2Pad.Services;

/// <summary>Classifies actual device ancestry, including wireless/GIP devices.</summary>
public static class PhysicalControllerMonitor
{
    internal static bool IsViGEmDevicePath(string path) => ClassifyDevicePath(path).Virtual;

    internal static (bool Virtual, bool NativeXInput, bool Bluetooth) ClassifyDevicePath(string path)
    {
        string id = DeviceInstanceIdFromPath(path);
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidOperationException("GameInput did not provide a controller device path.");
        // Phantom nodes are usable while a device is arriving or being removed.
        if (CM_Locate_DevNode(out uint node, id, 1) != 0)
            throw new InvalidOperationException("Cannot identify the controller's device tree: " + id);
        bool nativeXInput = false, bluetooth = false;
        for (int depth = 0; depth < 32; depth++)
        {
            string instanceId = GetDeviceId(node), service = GetDeviceService(node);
            if (IsViGEmNode(instanceId, service)) return (true, false, false);
            nativeXInput |= service.StartsWith("xusb", StringComparison.OrdinalIgnoreCase)
                || service.StartsWith("xboxgip", StringComparison.OrdinalIgnoreCase)
                || instanceId.Contains("&IG_", StringComparison.OrdinalIgnoreCase);
            bluetooth |= instanceId.StartsWith("BTH", StringComparison.OrdinalIgnoreCase);
            if (CM_Get_Parent(out uint parent, node, 0) != 0) return (false, nativeXInput, bluetooth);
            node = parent;
        }
        throw new InvalidOperationException("Controller device ancestry exceeds the supported depth.");
    }

    internal static string DeviceInstanceIdFromPath(string path)
    {
        if (!path.StartsWith(@"\\?\", StringComparison.Ordinal)) return path;
        string id = path[4..];
        int guid = id.IndexOf("#{", StringComparison.Ordinal);
        if (guid >= 0) id = id[..guid];
        return id.Replace('#', '\\');
    }

    internal static bool IsViGEmNode(string instanceId, string service) =>
        instanceId.Contains("VIGEM", StringComparison.OrdinalIgnoreCase)
        || service.Equals("ViGEmBus", StringComparison.OrdinalIgnoreCase);

    private static string GetDeviceService(uint devInst)
    {
        byte[] buffer = new byte[512];
        uint length = (uint)buffer.Length;
        return CM_Get_DevNode_Registry_Property(devInst, 5 /* CM_DRP_SERVICE */,
            out _, buffer, ref length, 0) == 0
            ? Encoding.Unicode.GetString(buffer, 0, (int)length).TrimEnd('\0') : string.Empty;
    }

    private static string GetDeviceId(uint devInst)
    {
        StringBuilder buffer = new(512);
        if (CM_Get_Device_ID(devInst, buffer, buffer.Capacity, 0) != 0)
            throw new InvalidOperationException("Cannot read controller device identity.");
        return buffer.ToString();
    }

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Locate_DevNodeW")]
    private static extern uint CM_Locate_DevNode(out uint node, string instanceId, uint flags);
    [DllImport("cfgmgr32.dll")]
    private static extern int CM_Get_Parent(out uint parent, uint devInst, uint flags);
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Get_Device_IDW")]
    private static extern int CM_Get_Device_ID(uint devInst, StringBuilder buffer, int bufferLength, uint flags);
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Get_DevNode_Registry_PropertyW")]
    private static extern uint CM_Get_DevNode_Registry_Property(uint devInst, uint property,
        out uint regType, byte[] buffer, ref uint length, uint flags);
}
