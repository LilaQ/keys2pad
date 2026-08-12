using System.Runtime.InteropServices;
using System.Text;

namespace IPAC.XInputBridge.Services;

public interface IPhysicalControllerCounter
{
    int CountPhysicalXInputControllers();
}

/// <summary>
/// Counts present XUSB device nodes and walks their parent tree so ViGEm children
/// are excluded. This avoids counting our own virtual Xbox 360 controllers.
/// </summary>
public sealed class PhysicalControllerMonitor : IPhysicalControllerCounter
{
    private const uint DigcfPresent = 0x00000002;
    private const uint DigcfAllClasses = 0x00000004;
    private const uint SpdrpHardwareId = 0x00000001;
    private const uint SpdrpService = 0x00000004;
    private const int ErrorNoMoreItems = 259;

    public int CountPhysicalXInputControllers()
    {
        IntPtr deviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, null, IntPtr.Zero, DigcfPresent | DigcfAllClasses);
        if (deviceInfoSet == new IntPtr(-1))
        {
            return 0;
        }

        try
        {
            int count = 0;
            HashSet<string> instanceIds = new(StringComparer.OrdinalIgnoreCase);
            SP_DEVINFO_DATA data = new() { cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>() };

            for (uint index = 0; SetupDiEnumDeviceInfo(deviceInfoSet, index, ref data); index++)
            {
                string hardwareIds = GetProperty(deviceInfoSet, ref data, SpdrpHardwareId);
                string service = GetProperty(deviceInfoSet, ref data, SpdrpService);
                bool isXInput = hardwareIds.Contains("IG_", StringComparison.OrdinalIgnoreCase)
                    || service.Equals("xusb22", StringComparison.OrdinalIgnoreCase)
                    || service.Equals("xusb21", StringComparison.OrdinalIgnoreCase)
                    || service.Equals("xusbhid", StringComparison.OrdinalIgnoreCase);

                if (!isXInput || IsViGEmDescendant(data.DevInst))
                {
                    data.cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>();
                    continue;
                }

                string instanceId = GetDeviceId(data.DevInst);
                if (instanceIds.Add(instanceId))
                {
                    count++;
                }

                data.cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>();
            }

            int error = Marshal.GetLastWin32Error();
            return error == ErrorNoMoreItems ? Math.Min(4, count) : Math.Min(4, count);
        }
        finally
        {
            SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }
    }

    private static bool IsViGEmDescendant(uint devInst)
    {
        uint current = devInst;
        for (int depth = 0; depth < 12; depth++)
        {
            string id = GetDeviceId(current);
            if (id.Contains("VIGEM", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (CM_Get_Parent(out uint parent, current, 0) != 0)
            {
                break;
            }

            current = parent;
        }

        return false;
    }

    private static string GetDeviceId(uint devInst)
    {
        StringBuilder buffer = new(512);
        return CM_Get_Device_ID(devInst, buffer, buffer.Capacity, 0) == 0 ? buffer.ToString() : string.Empty;
    }

    private static string GetProperty(IntPtr set, ref SP_DEVINFO_DATA data, uint property)
    {
        byte[] buffer = new byte[4096];
        if (!SetupDiGetDeviceRegistryProperty(set, ref data, property, out _, buffer, (uint)buffer.Length, out _))
        {
            return string.Empty;
        }

        return Encoding.Unicode.GetString(buffer).TrimEnd('\0').Replace('\0', ';');
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_DEVINFO_DATA
    {
        public uint cbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SetupDiGetClassDevs(IntPtr classGuid, string? enumerator, IntPtr parent, uint flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiEnumDeviceInfo(IntPtr set, uint index, ref SP_DEVINFO_DATA data);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetupDiGetDeviceRegistryProperty(
        IntPtr set, ref SP_DEVINFO_DATA data, uint property, out uint propertyType,
        byte[] buffer, uint bufferSize, out uint requiredSize);

    [DllImport("setupapi.dll")]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);

    [DllImport("cfgmgr32.dll")]
    private static extern int CM_Get_Parent(out uint parent, uint devInst, uint flags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern int CM_Get_Device_ID(uint devInst, StringBuilder buffer, int bufferLength, uint flags);
}
