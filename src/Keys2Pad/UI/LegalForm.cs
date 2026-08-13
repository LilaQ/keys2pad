using System.Diagnostics;

namespace Keys2Pad.UI;

public sealed class LegalForm : Form
{
    public LegalForm()
    {
        Text = "Legal & Compliance";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(720, 610);
        MinimumSize = new Size(560, 440);

        RichTextBox text = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            DetectUrls = true,
            BackColor = SystemColors.Window,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(12),
            Text = $"""
                Keys2Pad — Legal & Compliance
                Version {Application.ProductVersion}

                Privacy and data handling
                The app works locally. It reads the current state of configured keyboard keys and Windows device information for connected controllers. Configurations are stored under %LOCALAPPDATA%\Keys2Pad. The app has no accounts, cloud sync, telemetry, or advertising, and it does not send data over the network.

                Permissions
                The app does not require administrator privileges. It can optionally create a startup entry for the current user. The separately installed ViGEmBus driver is a kernel driver and requires administrator privileges during installation.

                Terms and warranty
                This prerelease is provided without warranty. Publisher identity, terms or EULA, and a separate support address still require decisions from the owner.

                Third-party software
                Nefarius.ViGEm.Client 1.21.256 (MIT) and ViGEmBus (BSD-3-Clause). ViGEmBus has been discontinued by its manufacturer. Project and binary releases:
                https://github.com/nefarius/ViGEmBus

                Microsoft .NET 8 Runtime — MIT and additional notices:
                https://github.com/dotnet/runtime

                Privacy and support
                https://github.com/LilaQ/keys2pad/blob/main/docs/PRIVACY.md
                https://github.com/LilaQ/keys2pad/issues
                Local documentation is also included in the package under docs.

                Safety and limitations
                Windows assigns XInput slots P1–P4 automatically. When physical controllers change, the app briefly disconnects its virtual devices and reconnects the required ones. Running games handle this change differently depending on their implementation.
                """
        };
        text.LinkClicked += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.LinkText))
            {
                Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
            }
        };

        Button close = new() { Text = "Close", Dock = DockStyle.Bottom, Height = 38, DialogResult = DialogResult.OK };
        Controls.Add(text);
        Controls.Add(close);
        AcceptButton = close;
        CancelButton = close;
    }
}
