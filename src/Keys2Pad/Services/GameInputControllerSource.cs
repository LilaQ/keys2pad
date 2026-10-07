using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Keys2Pad.Services;

/// <summary>
/// Slim binding to Microsoft's versioned GameInput v2 ABI. Unlike XInput and
/// Windows.Gaming.Input, it supports background reads beyond the four game slots.
/// ABI: Microsoft.GameInput 3.5.283, native/include/v2/GameInput.h.
/// </summary>
public sealed class GameInputControllerSource : IPhysicalControllerSource
{
    private const uint GamepadKind = 0x40000;
    private readonly object _sync = new();
    private readonly Dictionary<IntPtr, Device> _devices = [];
    private readonly DeviceCallback _callback;
    private IntPtr _library, _input;
    private ulong _callbackToken;
    private long _connection;
    private string? _error;
    private bool _disposed;

    private sealed class Device(IntPtr handle, string id, string name, Guid container, string path, int family, ushort vendor)
    {
        public IntPtr Handle { get; } = handle;
        public string Id { get; } = id;
        public string Name { get; } = name;
        public Guid Container { get; } = container;
        public string Path { get; } = path;
        public DateTime FirstSeen { get; } = DateTime.UtcNow;
        public bool? Virtual { get; set; }
        public int Family { get; } = family;
        public ushort Vendor { get; } = vendor;
        public bool NativeXInputEligible { get; set; }
    }

    public GameInputControllerSource()
    {
        _callback = OnDevice;
        try
        {
            _library = LoadRuntime();
            Guid iid = new("BBAA66D2-837A-40F7-A303-917D500955F4");
            Initialize initialize = Marshal.GetDelegateForFunctionPointer<Initialize>(NativeLibrary.GetExport(_library, "GameInputInitialize"));
            Marshal.ThrowExceptionForHR(initialize(ref iid, out _input));
            Method<FocusPolicy>(_input, 16)(_input, 0x40); // GameInputEnableBackgroundInput
            Marshal.ThrowExceptionForHR(Method<RegisterDevices>(_input, 8)(_input, IntPtr.Zero,
                GamepadKind, 1, 2 /* blocking initial enumeration */, IntPtr.Zero, _callback, out _callbackToken));
            RuntimeLog.Write("GameInput initialized; background gamepad input enabled.");
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private static IntPtr LoadRuntime()
    {
        List<string> paths = [Path.Combine(Environment.SystemDirectory, "GameInputRedist.dll"),
            Path.Combine(Environment.SystemDirectory, "GameInput.dll")];
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        using RegistryKey? key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\GameInput");
        if (key?.GetValue("RedistDir") is string installPath)
            paths.Add(Path.Combine(installPath, "GameInputRedist.dll"));
        paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft GameInput", "x64", "GameInputRedist.dll"));
        foreach (string path in paths.Where(File.Exists)
            .OrderByDescending(p => FileVersionInfo.GetVersionInfo(p).FileVersion is string v && Version.TryParse(v, out Version? version) ? version : new Version()))
        {
            if (!NativeLibrary.TryLoad(path, out IntPtr library)) continue;
            if (NativeLibrary.TryGetExport(library, "GameInputInitialize", out _)) return library;
            NativeLibrary.Free(library);
        }
        throw new InvalidOperationException("Install Microsoft GameInput using GameInputRedist.msi in the package, then restart Keys2Pad.");
    }

    private void OnDevice(ulong token, IntPtr context, IntPtr handle, ulong timestamp, uint current, uint previous)
    {
        try
        {
            lock (_sync)
            {
                if (_disposed) return;
                if ((current & 1) == 0)
                {
                    if (_devices.Remove(handle, out Device? removed))
                    {
                        if (removed.Virtual == false) RuntimeLog.Write($"Physical gamepad disconnected: {removed.Name}.");
                        Marshal.Release(removed.Handle);
                    }
                    return;
                }
                if (_devices.ContainsKey(handle)) return;
                Marshal.ThrowExceptionForHR(Method<DeviceInfoMethod>(handle, 3)(handle, out IntPtr data));
                DeviceInfo info = Marshal.PtrToStructure<DeviceInfo>(data);
                string path = Marshal.PtrToStringUTF8(info.PnpPath) ?? string.Empty;
                string name = Marshal.PtrToStringUTF8(info.DisplayName) ?? "Gamepad";
                Marshal.AddRef(handle);
                _devices.Add(handle, new Device(handle, $"{Convert.ToHexString(info.DeviceId)}:{++_connection}", name, info.Container, path, info.Family, info.Vendor));
            }
        }
        catch (Exception ex)
        {
            // Never unwind a managed exception through a native callback.
            lock (_sync) _error = ex.Message;
            RuntimeLog.Write($"GameInput device callback failed: {ex}");
        }
    }

    public IReadOnlyList<PhysicalGamepad> Read()
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_error is not null) throw new InvalidOperationException(_error);
            List<PhysicalGamepad> result = [];
            HashSet<Guid> containers = [];
            foreach (Device device in _devices.Values)
            {
                if ((Method<DeviceStatus>(device.Handle, 5)(device.Handle) & 1) == 0) continue;
                if (device.Virtual is null)
                {
                    try
                    {
                        var classification = PhysicalControllerMonitor.ClassifyDevicePath(device.Path);
                        device.Virtual = classification.Virtual;
                        device.NativeXInputEligible = device.Family is 1 or 2 || classification.NativeXInput
                            || (device.Vendor == 0x045E && classification.Bluetooth);
                        if (!device.Virtual.Value) RuntimeLog.Write($"Physical gamepad connected: {device.Name}.");
                    }
                    catch (InvalidOperationException) when (DateTime.UtcNow - device.FirstSeen < TimeSpan.FromSeconds(2))
                    {
                        // Arrival notifications can precede a queryable PnP tree.
                        continue;
                    }
                }
                if (device.Virtual == true) continue;
                // Some hardware exposes several input collections in one container.
                if (device.Container != Guid.Empty && !containers.Add(device.Container)) continue;
                GamepadState state = default;
                bool available = false;
                int hr = Method<CurrentReading>(_input, 4)(_input, GamepadKind, device.Handle, out IntPtr reading);
                if (hr >= 0 && reading != IntPtr.Zero)
                {
                    try { available = Method<ReadGamepad>(reading, 18)(reading, out state); }
                    finally { Marshal.Release(reading); }
                }
                result.Add(new PhysicalGamepad(device.Id, device.Name, available ? state : default, available, device.NativeXInputEligible));
            }
            return result;
        }
    }

    private static T Method<T>(IntPtr instance, int index) where T : Delegate =>
        Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(instance), index * IntPtr.Size));

    public void Dispose()
    {
        lock (_sync) _disposed = true;
        if (_input != IntPtr.Zero && _callbackToken != 0)
        {
            Method<StopCallback>(_input, 11)(_input, _callbackToken);
            // Called outside _sync: unregister waits for any in-flight callback.
            Method<UnregisterCallback>(_input, 12)(_input, _callbackToken);
            _callbackToken = 0;
        }
        lock (_sync)
        {
            foreach (Device device in _devices.Values) Marshal.Release(device.Handle);
            _devices.Clear();
        }
        if (_input != IntPtr.Zero) { Marshal.Release(_input); _input = IntPtr.Zero; }
        if (_library != IntPtr.Zero) { NativeLibrary.Free(_library); _library = IntPtr.Zero; }
        GC.KeepAlive(_callback);
    }

    // Only the prefix up to pnpPath is read; the remaining native fields stay native.
    [StructLayout(LayoutKind.Sequential)]
    private struct DeviceInfo
    {
        public ushort Vendor, Product, Revision, UsagePage, UsageId;
        public ushort HardwareMajor, HardwareMinor, HardwareBuild, HardwareRevision;
        public ushort FirmwareMajor, FirmwareMinor, FirmwareBuild, FirmwareRevision;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] DeviceId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] RootId;
        public int Family;
        public uint SupportedInput, RumbleMotors, SystemButtons;
        public Guid Container;
        public IntPtr DisplayName, PnpPath;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Initialize(ref Guid iid, out IntPtr input);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void FocusPolicy(IntPtr input, uint policy);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int RegisterDevices(IntPtr input, IntPtr device, uint kind, uint status, uint enumeration, IntPtr context, DeviceCallback callback, out ulong token);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void DeviceCallback(ulong token, IntPtr context, IntPtr device, ulong time, uint current, uint previous);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int DeviceInfoMethod(IntPtr device, out IntPtr info);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate uint DeviceStatus(IntPtr device);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int CurrentReading(IntPtr input, uint kind, IntPtr device, out IntPtr reading);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool ReadGamepad(IntPtr reading, out GamepadState state);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void StopCallback(IntPtr input, ulong token);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool UnregisterCallback(IntPtr input, ulong token);
}
