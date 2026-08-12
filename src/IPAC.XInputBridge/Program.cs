using System.Diagnostics;
using IPAC.XInputBridge.Models;
using IPAC.XInputBridge.Services;
using IPAC.XInputBridge.UI;

namespace IPAC.XInputBridge;

internal static class Program
{
    private const string MutexName = "Local\\IPAC.XInputBridge.SingleInstance.v1";

    [STAThread]
    private static void Main(string[] args)
    {
        string? command = ParseCommand(args);
        if (command is not null)
        {
            string? response = CommandServer.TrySendAsync(command).GetAwaiter().GetResult();
            if (response is not null)
            {
                WriteConsole(response);
                return;
            }

            if (command is "stop" or "exit" or "hide" or "status")
            {
                WriteConsole("IPAC XInput Bridge läuft nicht.");
                Environment.ExitCode = 2;
                return;
            }

            string startupCommand = command == "toggle" ? "start" : command;
            response = StartBackgroundAndSend(startupCommand);
            if (response is not null)
            {
                WriteConsole(response);
                return;
            }

            WriteConsole("IPAC XInput Bridge konnte nicht gestartet werden.");
            Environment.ExitCode = 4;
            return;
        }

        using Mutex mutex = new(initiallyOwned: true, MutexName, out bool firstInstance);
        if (!firstInstance)
        {
            WriteConsole("IPAC XInput Bridge läuft bereits.");
            return;
        }

        ApplicationConfiguration.Initialize();
        ConfigStore store = new();
        AppConfig config = store.Load();
        using SlotCoordinator coordinator = new(new PhysicalControllerMonitor(), new KeyboardInput(), () => config);
        using MainForm form = new(config, store, coordinator);
        using CommandServer server = new(cmd => form.ExecuteCommandAsync(cmd));

        if (config.StartEnabled)
        {
            coordinator.Start();
        }

        bool tray = args.Any(a => a.Equals("--tray", StringComparison.OrdinalIgnoreCase));
        if (tray)
        {
            form.BeginHidden = true;
        }

        Application.Run(form);
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
            string? response = CommandServer.TrySendAsync(command, timeoutMs: 150).GetAwaiter().GetResult();
            if (response is not null)
            {
                return response;
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
        string executable = Path.GetFileName(Environment.ProcessPath) ?? "IPAC.XInputBridge.exe";
        WriteConsole($"""
            {executable} [Option]

              --start             Bridge aktivieren (startet die App bei Bedarf)
              --stop              Bridge deaktivieren
              --toggle            Bridge umschalten
              --status            Status als Text ausgeben
              --profile "Name"    Profil aktivieren
              --show / --hide     Fenster zeigen/verbergen
              --tray              neue Instanz im Tray starten
              --exit              App vollständig beenden
              --help              diese Hilfe
            """);
    }

    private static void WriteConsole(string text)
    {
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
