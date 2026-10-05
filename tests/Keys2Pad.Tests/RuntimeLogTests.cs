using Keys2Pad.Services;

internal static class RuntimeLogTests
{
    public static void Run()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Keys2Pad.log");
        byte[]? original = File.Exists(path) ? File.ReadAllBytes(path) : null;
        try
        {
            File.WriteAllText(path, "previous-session-marker");
            RuntimeLog.StartSession();
            RuntimeLog.Write("cli-append-marker");
            Parallel.For(0, 16, i => RuntimeLog.Write($"concurrent-marker-{i}"));
            string contents = File.ReadAllText(path);
            if (contents.Contains("previous-session-marker") || !contents.Contains("cli-append-marker")
                || Enumerable.Range(0, 16).Any(i => !contents.Contains($"concurrent-marker-{i}")))
                throw new InvalidOperationException("Log reset, append or concurrent writes failed.");
            RuntimeLog.StartSession();
            if (File.ReadAllText(path).Contains("cli-append-marker"))
                throw new InvalidOperationException("New session did not replace previous log.");
        }
        finally
        {
            if (original is null) File.Delete(path);
            else File.WriteAllBytes(path, original);
        }
    }
}
