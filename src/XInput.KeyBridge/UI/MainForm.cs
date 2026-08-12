using System.Diagnostics;
using XInput.KeyBridge.Models;
using XInput.KeyBridge.Services;

namespace XInput.KeyBridge.UI;

public sealed class MainForm : Form
{
    private static readonly (VirtualInput Input, string Label)[] Inputs =
    [
        (VirtualInput.LeftStickUp, "Linker Stick hoch"),
        (VirtualInput.LeftStickDown, "Linker Stick runter"),
        (VirtualInput.LeftStickLeft, "Linker Stick links"),
        (VirtualInput.LeftStickRight, "Linker Stick rechts"),
        (VirtualInput.RightStickUp, "Rechter Stick hoch"),
        (VirtualInput.RightStickDown, "Rechter Stick runter"),
        (VirtualInput.RightStickLeft, "Rechter Stick links"),
        (VirtualInput.RightStickRight, "Rechter Stick rechts"),
        (VirtualInput.DPadUp, "Steuerkreuz hoch"),
        (VirtualInput.DPadDown, "Steuerkreuz runter"),
        (VirtualInput.DPadLeft, "Steuerkreuz links"),
        (VirtualInput.DPadRight, "Steuerkreuz rechts"),
        (VirtualInput.A, "A"), (VirtualInput.B, "B"),
        (VirtualInput.X, "X"), (VirtualInput.Y, "Y"),
        (VirtualInput.LeftShoulder, "LB"),
        (VirtualInput.RightShoulder, "RB"),
        (VirtualInput.LeftTrigger, "LT"),
        (VirtualInput.RightTrigger, "RT"),
        (VirtualInput.Back, "Back / View"),
        (VirtualInput.Start, "Start / Menu"),
        (VirtualInput.Guide, "Xbox / Guide"),
        (VirtualInput.LeftThumb, "Linken Stick drücken"),
        (VirtualInput.RightThumb, "Rechten Stick drücken")
    ];

    private readonly AppConfig _config;
    private readonly ConfigStore _store;
    private readonly SlotCoordinator _coordinator;
    private readonly Label _statusLabel = new();
    private readonly Label _slotLabel = new();
    private readonly Button _toggleButton = new();
    private readonly ComboBox _profiles = new();
    private readonly TabControl _playerTabs = new();
    private readonly CheckBox _startEnabled = new();
    private readonly CheckBox _autostart = new();
    private readonly CheckBox _minimizeToTray = new();
    private readonly NumericUpDown _pollInterval = new();
    private readonly NotifyIcon _trayIcon;
    private readonly Dictionary<(int Player, VirtualInput Input), Button> _mappingButtons = [];
    private (int Player, VirtualInput Input, Button Button)? _capture;
    private bool _reallyClose;
    private bool _updatingAutostart;

    public MainForm(AppConfig config, ConfigStore store, SlotCoordinator coordinator)
    {
        _config = config;
        _store = store;
        _coordinator = coordinator;

        Text = "XInput KeyBridge";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(780, 620);
        Size = new Size(900, 760);
        KeyPreview = true;

        MainMenuStrip = BuildMenu();
        Controls.Add(MainMenuStrip);
        Controls.Add(BuildMainLayout());

        ContextMenuStrip trayMenu = new();
        trayMenu.Items.Add("Öffnen", null, (_, _) => ShowWindow());
        trayMenu.Items.Add("Bridge starten", null, (_, _) => _coordinator.Start());
        trayMenu.Items.Add("Bridge stoppen", null, (_, _) => _coordinator.Stop());
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Beenden", null, (_, _) => ExitApplication());
        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "XInput KeyBridge",
            ContextMenuStrip = trayMenu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ShowWindow();

        _coordinator.StatusChanged += CoordinatorOnStatusChanged;
        FormClosing += OnFormClosing;
        Resize += (_, _) =>
        {
            if (WindowState == FormWindowState.Minimized && _config.MinimizeToTray)
            {
                Hide();
            }
        };
        KeyDown += CaptureKeyDown;

        RefreshProfiles();
        LoadSettings();
        BuildPlayerTabs();
        UpdateStatus(_coordinator.Status);
    }

    public bool BeginHidden { get; set; }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (BeginHidden)
        {
            Hide();
        }
    }

    private MenuStrip BuildMenu()
    {
        MenuStrip menu = new();
        ToolStripMenuItem file = new("Datei");
        file.DropDownItems.Add("Konfigurationsordner öffnen", null, (_, _) =>
            Process.Start(new ProcessStartInfo("explorer.exe", _store.DirectoryPath) { UseShellExecute = true }));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("Beenden", null, (_, _) => ExitApplication());

        ToolStripMenuItem help = new("Hilfe");
        help.DropDownItems.Add("Rechtliches & Compliance", null, (_, _) => new LegalForm().ShowDialog(this));
        help.DropDownItems.Add("Über", null, (_, _) => MessageBox.Show(
            this,
            $"XInput KeyBridge\nVersion {Application.ProductVersion}\n\nKeyboard zu XInput für Windows 10/11.",
            "Über", MessageBoxButtons.OK, MessageBoxIcon.Information));

        menu.Items.Add(file);
        menu.Items.Add(help);
        return menu;
    }

    private Control BuildMainLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 38, 12, 12),
            ColumnCount = 1,
            RowCount = 4
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        TableLayoutPanel status = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        FlowLayoutPanel statusText = new() { AutoSize = true, FlowDirection = FlowDirection.TopDown };
        _statusLabel.Font = new Font(Font, FontStyle.Bold);
        _statusLabel.AutoSize = true;
        _slotLabel.AutoSize = true;
        statusText.Controls.Add(_statusLabel);
        statusText.Controls.Add(_slotLabel);
        _toggleButton.AutoSize = true;
        _toggleButton.Click += (_, _) => ToggleBridge();
        Button refresh = new() { Text = "Geräte neu erkennen", AutoSize = true, Margin = new Padding(8, 3, 0, 3) };
        refresh.Click += (_, _) => _coordinator.Rebuild();
        status.Controls.Add(statusText, 0, 0);
        status.Controls.Add(_toggleButton, 1, 0);
        status.Controls.Add(refresh, 2, 0);
        root.Controls.Add(status, 0, 0);

        FlowLayoutPanel profileBar = new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 12, 0, 8) };
        profileBar.Controls.Add(new Label { Text = "Profil:", AutoSize = true, Margin = new Padding(0, 7, 4, 0) });
        _profiles.DropDownStyle = ComboBoxStyle.DropDownList;
        _profiles.Width = 220;
        _profiles.SelectedIndexChanged += (_, _) => SelectProfile();
        profileBar.Controls.Add(_profiles);
        profileBar.Controls.Add(MakeButton("Neu", (_, _) => NewProfile()));
        profileBar.Controls.Add(MakeButton("Duplizieren", (_, _) => DuplicateProfile()));
        profileBar.Controls.Add(MakeButton("Löschen", (_, _) => DeleteProfile()));
        root.Controls.Add(profileBar, 0, 1);

        _playerTabs.Dock = DockStyle.Fill;
        root.Controls.Add(_playerTabs, 0, 2);

        GroupBox settings = new() { Text = "Verhalten", Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(10) };
        FlowLayoutPanel settingsFlow = new() { Dock = DockStyle.Fill, AutoSize = true };
        _startEnabled.Text = "Bridge beim App-Start aktivieren";
        _startEnabled.AutoSize = true;
        _startEnabled.CheckedChanged += (_, _) => { _config.StartEnabled = _startEnabled.Checked; Save(); };
        _autostart.Text = "Mit Windows starten";
        _autostart.AutoSize = true;
        _autostart.CheckedChanged += (_, _) => SetAutostart();
        _minimizeToTray.Text = "Schließen/Minimieren ins Tray";
        _minimizeToTray.AutoSize = true;
        _pollInterval.Minimum = 4;
        _pollInterval.Maximum = 50;
        _pollInterval.Width = 55;
        _pollInterval.ValueChanged += (_, _) => { _config.PollIntervalMs = (int)_pollInterval.Value; Save(); };
        settingsFlow.Controls.Add(_startEnabled);
        settingsFlow.Controls.Add(_autostart);
        settingsFlow.Controls.Add(_minimizeToTray);
        settingsFlow.Controls.Add(new Label { Text = "Polling (ms):", AutoSize = true, Margin = new Padding(16, 7, 2, 0) });
        settingsFlow.Controls.Add(_pollInterval);
        settings.Controls.Add(settingsFlow);
        root.Controls.Add(settings, 0, 3);
        return root;
    }

    private void BuildPlayerTabs()
    {
        _capture = null;
        _mappingButtons.Clear();
        _playerTabs.TabPages.Clear();
        ProfileConfig profile = _config.CurrentProfile;

        for (int playerIndex = 0; playerIndex < 4; playerIndex++)
        {
            int capturedPlayer = playerIndex;
            PlayerConfig player = profile.Players[playerIndex];
            TabPage page = new($"P{playerIndex + 1}");
            TableLayoutPanel table = new()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 2,
                Padding = new Padding(12)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            CheckBox enabled = new() { Text = $"Keyboard-Spieler P{playerIndex + 1} aktiv", Checked = player.Enabled, AutoSize = true };
            enabled.CheckedChanged += (_, _) => { player.Enabled = enabled.Checked; Save(); };
            table.Controls.Add(enabled, 0, 0);
            table.SetColumnSpan(enabled, 2);

            Label instructions = new()
            {
                Text = "Linksklick: Taste neu zuweisen · Rechtsklick: Zuordnung löschen",
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(3, 3, 3, 10)
            };
            table.Controls.Add(instructions, 0, 1);
            table.SetColumnSpan(instructions, 2);

            int row = 2;
            foreach ((VirtualInput input, string label) in Inputs)
            {
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) }, 0, row);
                Button button = new() { Dock = DockStyle.Top, Height = 28, Tag = input, Text = KeyName(player, input) };
                button.Click += (_, _) => BeginCapture(capturedPlayer, input, button);
                button.MouseUp += (_, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        ClearBinding(capturedPlayer, input, button);
                    }
                };
                _mappingButtons[(capturedPlayer, input)] = button;
                table.Controls.Add(button, 1, row++);
            }

            FlowLayoutPanel actions = new() { AutoSize = true, Margin = new Padding(0, 9, 0, 6) };
            Button clear = new() { Text = "Alle Belegungen löschen", AutoSize = true };
            clear.Click += (_, _) => ClearPlayer(capturedPlayer);
            Button reset = new() { Text = "Standard wiederherstellen", AutoSize = true };
            reset.Click += (_, _) => ResetPlayer(capturedPlayer);
            actions.Controls.Add(clear);
            actions.Controls.Add(reset);
            table.Controls.Add(actions, 0, row);
            table.SetColumnSpan(actions, 2);
            page.Controls.Add(table);
            _playerTabs.TabPages.Add(page);
        }
    }

    private static Button MakeButton(string text, EventHandler click)
    {
        Button button = new() { Text = text, AutoSize = true };
        button.Click += click;
        return button;
    }

    private void BeginCapture(int player, VirtualInput input, Button button)
    {
        if (_capture is { } old)
        {
            old.Button.Text = KeyName(_config.CurrentProfile.Players[old.Player], old.Input);
        }

        _capture = (player, input, button);
        button.Text = "Beliebige Taste drücken …";
        button.Focus();
    }

    private void CaptureKeyDown(object? sender, KeyEventArgs e)
    {
        if (_capture is not { } capture)
        {
            return;
        }

        PlayerConfig player = _config.CurrentProfile.Players[capture.Player];
        player.Bindings[capture.Input] = (int)e.KeyCode;

        capture.Button.Text = KeyName(player, capture.Input);
        _capture = null;
        e.Handled = true;
        e.SuppressKeyPress = true;
        Save();
    }

    private static string KeyName(PlayerConfig player, VirtualInput input) =>
        player.Bindings.TryGetValue(input, out int key) ? ((Keys)key).ToString() : "— nicht belegt —";

    private void ClearBinding(int playerIndex, VirtualInput input, Button button)
    {
        if (_capture is { } capture && capture.Player == playerIndex && capture.Input == input)
        {
            _capture = null;
        }

        PlayerConfig player = _config.CurrentProfile.Players[playerIndex];
        player.Bindings.Remove(input);
        button.Text = KeyName(player, input);
        Save();
    }

    private void ClearPlayer(int playerIndex)
    {
        if (MessageBox.Show(this, $"Wirklich alle Belegungen für P{playerIndex + 1} löschen?", "Belegungen löschen",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _config.CurrentProfile.Players[playerIndex].Bindings.Clear();
        Save();
        BuildPlayerTabs();
    }

    private void LoadSettings()
    {
        _startEnabled.Checked = _config.StartEnabled;
        _minimizeToTray.Checked = _config.MinimizeToTray;
        _pollInterval.Value = Math.Clamp(_config.PollIntervalMs, 4, 50);
        try { _autostart.Checked = AutostartService.IsEnabled(); } catch { _autostart.Enabled = false; }
    }

    private void SelectProfile()
    {
        if (_profiles.SelectedItem is not string selected || selected == _config.ActiveProfile)
        {
            return;
        }

        _config.ActiveProfile = selected;
        Save();
        BuildPlayerTabs();
        _coordinator.Rebuild();
    }

    private void RefreshProfiles()
    {
        _profiles.BeginUpdate();
        _profiles.Items.Clear();
        _profiles.Items.AddRange(_config.Profiles.Select(p => p.Name).Cast<object>().ToArray());
        _profiles.SelectedItem = _config.ActiveProfile;
        _profiles.EndUpdate();
    }

    private void NewProfile()
    {
        string? name = PromptDialog.Ask(this, "Neues Profil", "Profilname:", "Neues Profil");
        if (!TryUseNewName(name)) return;
        _config.Profiles.Add(ProfileConfig.CreateDefault(name!));
        _config.ActiveProfile = name!;
        Save();
        RefreshProfiles();
        BuildPlayerTabs();
        _coordinator.Rebuild();
    }

    private void DuplicateProfile()
    {
        string? name = PromptDialog.Ask(this, "Profil duplizieren", "Name der Kopie:", _config.ActiveProfile + " Kopie");
        if (!TryUseNewName(name)) return;
        _config.Profiles.Add(_config.CurrentProfile.Clone(name!));
        _config.ActiveProfile = name!;
        Save();
        RefreshProfiles();
        BuildPlayerTabs();
        _coordinator.Rebuild();
    }

    private bool TryUseNewName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (_config.Profiles.Any(p => p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(this, "Dieser Profilname existiert bereits.", "Profil", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private void DeleteProfile()
    {
        if (_config.Profiles.Count == 1)
        {
            MessageBox.Show(this, "Das letzte Profil kann nicht gelöscht werden.");
            return;
        }

        if (MessageBox.Show(this, $"Profil „{_config.ActiveProfile}“ löschen?", "Profil löschen",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _config.Profiles.Remove(_config.CurrentProfile);
        _config.ActiveProfile = _config.Profiles[0].Name;
        Save();
        RefreshProfiles();
        BuildPlayerTabs();
        _coordinator.Rebuild();
    }

    private void ResetPlayer(int playerIndex)
    {
        if (MessageBox.Show(this, $"Belegung für P{playerIndex + 1} zurücksetzen?", "Zurücksetzen",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _config.CurrentProfile.Players[playerIndex] = PlayerConfig.CreateDefault(playerIndex);
        Save();
        BuildPlayerTabs();
    }

    private void SetAutostart()
    {
        if (_updatingAutostart) return;
        try
        {
            AutostartService.SetEnabled(_autostart.Checked);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Autostart", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _updatingAutostart = true;
            try { _autostart.Checked = !_autostart.Checked; }
            finally { _updatingAutostart = false; }
        }
    }

    private void ToggleBridge()
    {
        if (_coordinator.Status.Enabled) _coordinator.Stop(); else _coordinator.Start();
    }

    private void CoordinatorOnStatusChanged(object? sender, BridgeStatus status)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(() => UpdateStatus(status)); else UpdateStatus(status);
    }

    private void UpdateStatus(BridgeStatus status)
    {
        _statusLabel.Text = status.Error is not null
            ? "Fehler: " + status.Error
            : status.Enabled ? "Bridge aktiv" : "Bridge gestoppt";
        _statusLabel.ForeColor = status.Error is not null ? Color.Firebrick : status.Enabled ? Color.DarkGreen : SystemColors.ControlText;
        _slotLabel.Text = status.Enabled
            ? $"Echt: {status.PhysicalControllers} · Virtuell: {status.VirtualControllers} · Profil: {status.Profile}"
            : "Keine virtuellen Controller verbunden";
        _toggleButton.Text = status.Enabled ? "Bridge stoppen" : "Bridge starten";
        _trayIcon.Text = status.Enabled
            ? $"XInput KeyBridge: {status.PhysicalControllers} echt, {status.VirtualControllers} virtuell"
            : "XInput KeyBridge: gestoppt";
    }

    public Task<string> ExecuteCommandAsync(string command)
    {
        TaskCompletionSource<string> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void Execute()
        {
            try
            {
                string result = ExecuteCommand(command.Trim());
                completion.SetResult(result);
            }
            catch (Exception ex)
            {
                completion.SetResult("Fehler: " + ex.Message);
            }
        }

        if (InvokeRequired) BeginInvoke(Execute); else Execute();
        return completion.Task;
    }

    private string ExecuteCommand(string command)
    {
        switch (command.ToLowerInvariant())
        {
            case "start": _coordinator.Start(); return "OK: Bridge gestartet.";
            case "stop": _coordinator.Stop(); return "OK: Bridge gestoppt.";
            case "toggle": ToggleBridge(); return "OK: Bridge umgeschaltet.";
            case "show": ShowWindow(); return "OK: Fenster geöffnet.";
            case "hide": Hide(); return "OK: Fenster verborgen.";
            case "status":
                BridgeStatus s = _coordinator.Status;
                return $"{(s.Enabled ? "aktiv" : "gestoppt")}; physisch={s.PhysicalControllers}; virtuell={s.VirtualControllers}; profil={s.Profile}";
            case "exit":
                BeginInvoke(ExitApplication);
                return "OK: App wird beendet.";
        }

        if (command.StartsWith("profile ", StringComparison.OrdinalIgnoreCase))
        {
            string requested = command[8..].Trim().Trim('"');
            ProfileConfig? profile = _config.Profiles.FirstOrDefault(p => p.Name.Equals(requested, StringComparison.OrdinalIgnoreCase));
            if (profile is null) return $"Fehler: Profil „{requested}“ nicht gefunden.";
            _config.ActiveProfile = profile.Name;
            Save();
            RefreshProfiles();
            BuildPlayerTabs();
            _coordinator.Rebuild();
            return $"OK: Profil „{profile.Name}“ aktiv.";
        }

        return "Fehler: unbekannter Befehl.";
    }

    private void ShowWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_reallyClose && _config.MinimizeToTray && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
    }

    private void ExitApplication()
    {
        _reallyClose = true;
        _coordinator.Stop();
        Close();
    }

    private void Save() => _store.Save(_config);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _coordinator.StatusChanged -= CoordinatorOnStatusChanged;
            _trayIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
