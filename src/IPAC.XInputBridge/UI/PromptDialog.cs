namespace IPAC.XInputBridge.UI;

internal static class PromptDialog
{
    public static string? Ask(IWin32Window owner, string title, string label, string initial)
    {
        using Form form = new()
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(390, 125),
            MaximizeBox = false,
            MinimizeBox = false
        };
        Label prompt = new() { Text = label, Left = 12, Top = 14, AutoSize = true };
        TextBox text = new() { Text = initial, Left = 12, Top = 38, Width = 365 };
        Button ok = new() { Text = "OK", DialogResult = DialogResult.OK, Left = 220, Top = 82, Width = 75 };
        Button cancel = new() { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Left = 302, Top = 82, Width = 75 };
        form.Controls.AddRange([prompt, text, ok, cancel]);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        text.SelectAll();
        return form.ShowDialog(owner) == DialogResult.OK ? text.Text.Trim() : null;
    }
}
