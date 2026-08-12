using System.Diagnostics;

namespace XInput.KeyBridge.UI;

public sealed class LegalForm : Form
{
    public LegalForm()
    {
        Text = "Rechtliches & Compliance";
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
                XInput KeyBridge — Rechtliches & Compliance
                Version {Application.ProductVersion}

                Datenschutz und Datenverarbeitung
                Die App arbeitet lokal. Sie liest den aktuellen Zustand der konfigurierten Tastaturtasten und die Windows-Geräteinformationen verbundener Controller. Konfigurationen werden unter %LOCALAPPDATA%\XInput KeyBridge gespeichert. Es gibt keine Konten, Cloud-Synchronisierung, Telemetrie, Werbung oder Netzwerkübertragung durch die App.

                Berechtigungen
                Die App benötigt keine Administratorrechte. Optional schreibt sie einen Autostart-Eintrag für den aktuellen Benutzer. Der separat zu installierende ViGEmBus-Treiber ist ein Kernel-Treiber und benötigt bei seiner Installation Administratorrechte.

                Nutzung / Gewährleistung
                Diese Entwicklungsfassung wird ohne Gewähr bereitgestellt. Vor einer öffentlichen Veröffentlichung müssen Herausgeberidentität, Nutzungsbedingungen bzw. EULA und Supportkontakt durch den Eigentümer festgelegt werden.

                Drittanbieter
                Nefarius.ViGEm.Client 1.21.256 (MIT) und ViGEmBus (BSD-3-Clause). ViGEmBus wurde vom Hersteller eingestellt. Projekt und Binär-Releases:
                https://github.com/nefarius/ViGEmBus

                Microsoft .NET 8 Runtime — MIT und weitere Hinweise:
                https://github.com/dotnet/runtime

                Support
                Noch kein eigentümerfreigegebener öffentlicher Support- oder Datenschutz-Link hinterlegt. Dies ist ein Release-Blocker, kein behaupteter Compliance-Status. Lokale Dokumentation liegt dem Paket im Ordner docs bei.

                Sicherheit und Einschränkungen
                XInput vergibt P1–P4 automatisch. Bei Änderungen echter Controller trennt die App virtuelle Geräte kurz und meldet sie danach erneut an. Laufende Spiele erkennen diese Änderung je nach Implementierung unterschiedlich.
                """
        };
        text.LinkClicked += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.LinkText))
            {
                Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
            }
        };

        Button close = new() { Text = "Schließen", Dock = DockStyle.Bottom, Height = 38, DialogResult = DialogResult.OK };
        Controls.Add(text);
        Controls.Add(close);
        AcceptButton = close;
        CancelButton = close;
    }
}
