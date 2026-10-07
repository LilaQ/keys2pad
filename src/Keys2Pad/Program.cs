using System.Diagnostics;
using Keys2Pad.Models;
using Keys2Pad.Services;
using Keys2Pad.UI;

namespace Keys2Pad;

internal static class Program
{
    private const string MutexName = "Local\\Keys2Pad.SingleInstance.v1";

    [STAThread]
    private static void Main(string[] args)
    {
        string? command = ParseCommand(args);
        if (command is not null)
        {
            RuntimeLog.Write($"CLI command: {command}");
            string? response = CommandServer.TrySendAsync(command, responseTimeoutMs: 12000).GetAwaiter().GetResult();
            if (response is not null)
            {
                WriteConsole(response);
                if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)) Environment.ExitCode = 1;
                return;
            }

            if (command is "stop" or "exit" or "hide" or "status")
            {
                WriteConsole("Keys2Pad is not running.");
                Environment.ExitCode = 2;
                return;
            }

            string startupCommand = command == "toggle" ? "start" : command;
            response = StartBackgroundAndSend(startupCommand);
            if (response is not null)
            {
                WriteConsole(response);
                if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)) Environment.ExitCode = 1;
                return;
            }

            WriteConsole("Keys2Pad could not be started.");
            Environment.ExitCode = 4;
            return;
        }

        using Mutex mutex = new(initiallyOwned: true, MutexName, out bool firstInstance);
        if (!firstInstance)
        {
            WriteConsole("Keys2Pad is already running.");
            return;
        }

        RuntimeLog.StartSession();
        AppDomain.CurrentDomain.UnhandledException += (_, e) => RuntimeLog.Write($"Unhandled exception: {e.ExceptionObject}");
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        ApplicationConfiguration.Initialize();
        ConfigStore store = new();
        AppConfig config = store.Load();
        RuntimeLog.Write($"Config loaded: {store.FilePath}; profile={config.ActiveProfile}; startEnabled={config.StartEnabled}; players={config.CurrentProfile.Players.Count}");
        AppConfig runtimeConfig = config.RuntimeSnapshot();
        using SlotCoordinator coordinator = new(new KeyboardInput(), () => Volatile.Read(ref runtimeConfig));
        using MainForm form = new(config, store, coordinator, snapshot => Volatile.Write(ref runtimeConfig, snapshot));
        form.BeginHidden = args.Any(a => a.Equals("--tray", StringComparison.OrdinalIgnoreCase));
        // Create the dispatch handle on the STA thread before accepting pipe commands.
        _ = form.Handle;
        using CommandServer server = new(cmd => form.ExecuteCommandAsync(cmd));
        if (config.StartEnabled) coordinator.Start();

        try { Application.Run(form); }
        finally { coordinator.Stop(); RuntimeLog.Write("Session stopped."); }
    }

    private static string? StartBackgroundAndSend(string command)
    {
        string? executable = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executable))
        {
            return null;
        }

        ProcessStartInfo startInfo = new(executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("--tray");
        startInfo.ArgumentList.Add("--server");
        Process.Start(startInfo);

        for (int attempt = 0; attempt < 40; attempt++)
        {
            Thread.Sleep(100);
            string? response = CommandServer.TrySendAsync("status", timeoutMs: 150).GetAwaiter().GetResult();
            if (response is not null)
            {
                // Readiness probes must not replay a mutating command after a timeout.
                return CommandServer.TrySendAsync(command, responseTimeoutMs: 12000).GetAwaiter().GetResult();
            }
        }

        return null;
    }

    private static string? ParseCommand(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLowerInvariant();
            switch (arg)
            {
                case "--start": return "start";
                case "--stop": return "stop";
                case "--toggle": return "toggle";
                case "--status": return "status";
                case "--show": return "show";
                case "--hide": return "hide";
                case "--exit": return "exit";
                case "--watch-process" when i + 1 < args.Length: return "watch " + args[i + 1];
                case "--profile" when i + 1 < args.Length: return "profile " + args[i + 1];
                case "--help":
                case "-h":
                case "/?":
                    ShowHelp();
                    Environment.Exit(0);
                    break;
            }
        }

        return null;
    }

    private static void ShowHelp()
    {
        string executable = Path.GetFileName(Environment.ProcessPath) ?? "Keys2Pad.exe";
        WriteConsole($"""
            {executable} [option]

              --start             Enable the bridge (starts the app if needed)
              --stop              Disable the bridge
              --toggle            Toggle the bridge
              --status            Print status as text
              --profile "Name"    Activate a profile
              --show / --hide     Show or hide the window
              --tray              Start a new instance in the tray
              --watch-process EXE Exit when the game process ends (60s startup limit)
              --exit              Exit the app completely
              --help              Show this help
            """);
    }

    private static void WriteConsole(string text)
    {
        RuntimeLog.Write($"CLI response: {text}");
        try
        {
            if (!AttachConsole(AttachParentProcess))
            {
                return;
            }

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.WriteLine(text);
        }
        catch (IOException)
        {
            Debug.WriteLine(text);
        }
    }

    private const uint AttachParentProcess = 0xFFFFFFFF;

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(uint processId);

}
