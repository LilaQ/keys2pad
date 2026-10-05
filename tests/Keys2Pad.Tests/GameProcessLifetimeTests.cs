using System.Diagnostics;
using Keys2Pad.Services;

internal static class GameProcessLifetimeTests
{
    public static void Run()
    {
        string exe = OperatingSystem.IsWindows() ? "ping" : "sleep";
        using Process child = Process.Start(new ProcessStartInfo(exe,
            OperatingSystem.IsWindows() ? "-n 4 127.0.0.1" : "3") { UseShellExecute = false, CreateNoWindow = true })!;
        using ManualResetEventSlim ended = new();
        using (GameProcessLifetime watch = new(exe, ended.Set))
        {
            Thread.Sleep(500);
            if (ended.IsSet) throw new Exception("Live game must retain bridge.");
            child.Kill();
            child.WaitForExit();
            if (!ended.Wait(5000)) throw new Exception("Exited game must close bridge.");
        }
        using ManualResetEventSlim timedOut = new();
        using (GameProcessLifetime watch = new("k2p_missing_" + Guid.NewGuid().ToString("N"), timedOut.Set, TimeSpan.FromMilliseconds(100)))
            if (!timedOut.Wait(5000)) throw new Exception("Failed startup must close bridge.");
        using ManualResetEventSlim canceled = new();
        GameProcessLifetime canceledWatch = new("k2p_missing_" + Guid.NewGuid().ToString("N"), canceled.Set, TimeSpan.FromMilliseconds(500));
        canceledWatch.Dispose();
        if (canceled.Wait(1000)) throw new Exception("Replacing watch must not exit bridge.");
    }
}
