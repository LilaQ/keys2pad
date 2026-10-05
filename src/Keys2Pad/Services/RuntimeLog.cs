namespace Keys2Pad.Services;

public static class RuntimeLog
{
    private static readonly object Sync = new();
    private static string _path = Path.Combine(AppContext.BaseDirectory, "Keys2Pad.log");

    public static void StartSession()
    {
        lock (Sync)
        {
            try { File.WriteAllText(_path, string.Empty); }
            catch (IOException) { UseFallback(); }
            catch (UnauthorizedAccessException) { UseFallback(); }
        }
        Write($"Session started; version={typeof(RuntimeLog).Assembly.GetName().Version}; pid={Environment.ProcessId}; cwd={Environment.CurrentDirectory}");
    }

    public static void Write(string message)
    {
        lock (Sync)
        {
            try { File.AppendAllText(_path, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}"); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static void UseFallback()
    {
        _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Keys2Pad", "Keys2Pad.log");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, string.Empty);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
