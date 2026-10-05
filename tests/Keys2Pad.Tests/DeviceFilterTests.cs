using Keys2Pad.Services;

internal static class DeviceFilterTests
{
    public static void Run()
    {
        if (!PhysicalControllerMonitor.IsViGEmNode(@"ROOT\SYSTEM\0001", "ViGEmBus")
            || !PhysicalControllerMonitor.IsViGEmNode(@"ROOT\SYSTEM\0001", "vigembus")
            || !PhysicalControllerMonitor.IsViGEmNode(@"ROOT\VIGEMBUS\0000", "")
            || PhysicalControllerMonitor.IsViGEmNode(@"USB\VID_045E&PID_0B12", "xusb22"))
            throw new InvalidOperationException("Virtual parent service classification failed.");
    }
}
