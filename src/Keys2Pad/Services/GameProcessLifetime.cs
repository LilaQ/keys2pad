using System.Diagnostics;

namespace Keys2Pad.Services;

// Runs in the bridge itself, independently of LaunchBox's script lifetime.
internal sealed class GameProcessLifetime : IDisposable
{
    private readonly CancellationTokenSource _cancel = new();

    public GameProcessLifetime(string processName, Action ended, TimeSpan? startupTimeout = null)
    {
        _ = WatchAsync(Path.GetFileNameWithoutExtension(processName), ended,
            startupTimeout ?? TimeSpan.FromSeconds(60), _cancel.Token);
    }

    private static async Task WatchAsync(string name, Action ended, TimeSpan timeout, CancellationToken cancel)
    {
        List<Process> attached = new();
        try
        {
            Stopwatch startup = Stopwatch.StartNew();
            while (attached.Count == 0 && startup.Elapsed < timeout)
            {
                cancel.ThrowIfCancellationRequested();
                attached.AddRange(Process.GetProcessesByName(name));
                if (attached.Count == 0) await Task.Delay(250, cancel);
            }
            RuntimeLog.Write(attached.Count == 0
                ? $"Game watch: {name} did not start within {timeout.TotalSeconds}s."
                : $"Game watch: attached to {name}; PIDs={string.Join(",", attached.Select(p => p.Id))}.");
            while (attached.Any(IsAlive)) await Task.Delay(250, cancel);
            cancel.ThrowIfCancellationRequested();
            RuntimeLog.Write($"Game watch: {name} ended; exiting bridge.");
            ended();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            RuntimeLog.Write($"Game watch failed: {ex}");
            if (!cancel.IsCancellationRequested) ended();
        }
        finally { foreach (Process process in attached) process.Dispose(); }
    }

    private static bool IsAlive(Process process)
    {
        try { return !process.HasExited; }
        catch (InvalidOperationException) { return false; }
    }

    public void Dispose() => _cancel.Cancel();
}
