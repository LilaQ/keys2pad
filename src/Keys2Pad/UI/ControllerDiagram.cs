using System.Drawing.Drawing2D;
using Keys2Pad.Models;

namespace Keys2Pad.UI;

public sealed class VirtualInputEventArgs(VirtualInput input) : EventArgs
{
    public VirtualInput Input { get; } = input;
}

/// <summary>
/// Interactive vector controller based on the approved Xbox-style front view.
/// It is drawn entirely in code so it remains sharp at every Windows DPI scale.
/// </summary>
public sealed class ControllerDiagram : Control
{
    private const float DesignWidth = 1000f;
    private const float DesignHeight = 660f;
    private readonly ToolTip _toolTip = new();
    private readonly RegionInfo[] _regions = CreateRegions();
    private VirtualInput? _hoveredInput;
    private VirtualInput? _selectedInput;

    public ControllerDiagram()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        TabStop = false;
        Height = 410;
        MinimumSize = new Size(520, 260);
        BackColor = Color.FromArgb(9, 12, 16);
        AccessibleName = "Interaktive Controller-Übersicht";
        AccessibleDescription = "Controllerbereich anklicken, um eine Keyboard-Taste zuzuweisen. Rechtsklick löscht die Zuordnung.";
    }

    public event EventHandler<VirtualInputEventArgs>? BindingRequested;

    public event EventHandler<VirtualInputEventArgs>? BindingCleared;

    public Func<VirtualInput, string>? BindingTextProvider { get; set; }

    public VirtualInput? SelectedInput
    {
        get => _selectedInput;
        set
        {
            if (_selectedInput == value) return;
            _selectedInput = value;
            Invalidate();
        }
    }

    public void RefreshBindingDisplay()
    {
        if (_hoveredInput is { } hovered)
        {
            RegionInfo region = Find(hovered);
            _toolTip.SetToolTip(this,
                $"{region.ActionLabel}: {BindingTextProvider?.Invoke(region.Input) ?? "nicht belegt"}\nLinksklick: zuweisen · Rechtsklick: löschen");
        }
        else
        {
            _toolTip.SetToolTip(this, string.Empty);
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        (float scale, float offsetX, float offsetY) = GetTransform();
        GraphicsState state = e.Graphics.Save();
        e.Graphics.TranslateTransform(offsetX, offsetY);
        e.Graphics.ScaleTransform(scale, scale);

        DrawShell(e.Graphics);
        DrawShoulders(e.Graphics);
        DrawCenterPanel(e.Graphics);
        DrawSticks(e.Graphics);
        DrawDPad(e.Graphics);
        DrawFaceButtons(e.Graphics);
        DrawCenterButtons(e.Graphics);
        DrawFooter(e.Graphics);

        e.Graphics.Restore(state);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        RegionInfo? region = HitTest(e.Location);
        VirtualInput? hovered = region?.Input;
        if (_hoveredInput == hovered) return;

        _hoveredInput = hovered;
        Cursor = hovered.HasValue ? Cursors.Hand : Cursors.Default;
        string hint = region is null
            ? string.Empty
            : $"{region.ActionLabel}: {BindingTextProvider?.Invoke(region.Input) ?? "nicht belegt"}\nLinksklick: zuweisen · Rechtsklick: löschen";
        _toolTip.SetToolTip(this, hint);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoveredInput = null;
        Cursor = Cursors.Default;
        _toolTip.SetToolTip(this, string.Empty);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        RegionInfo? region = HitTest(e.Location);
        if (region is null) return;

        if (e.Button == MouseButtons.Left)
        {
            SelectedInput = region.Input;
            BindingRequested?.Invoke(this, new VirtualInputEventArgs(region.Input));
        }
        else if (e.Button == MouseButtons.Right)
        {
            BindingCleared?.Invoke(this, new VirtualInputEventArgs(region.Input));
        }
    }

    private void DrawShell(Graphics graphics)
    {
        using GraphicsPath shell = new();
        shell.StartFigure();
        shell.AddBezier(819, 148, 805, 132, 790, 124, 778, 105);
        shell.AddBezier(778, 105, 748, 82, 704, 66, 662, 60);
        shell.AddBezier(662, 60, 642, 59, 621, 73, 601, 83);
        shell.AddLine(601, 83, 389, 83);
        shell.AddBezier(389, 83, 370, 77, 351, 62, 334, 62);
        shell.AddBezier(334, 62, 291, 68, 247, 87, 215, 110);
        shell.AddBezier(215, 110, 208, 123, 194, 134, 184, 142);
        shell.AddBezier(184, 142, 167, 167, 149, 224, 132, 276);
        shell.AddBezier(132, 276, 112, 337, 96, 389, 87, 426);
        shell.AddBezier(87, 426, 80, 467, 77, 516, 83, 551);
        shell.AddBezier(83, 551, 88, 580, 104, 603, 119, 613);
        shell.AddBezier(119, 613, 134, 626, 157, 638, 173, 634);
        shell.AddBezier(173, 634, 198, 619, 226, 581, 250, 550);
        shell.AddLine(250, 550, 275, 521);
        shell.AddBezier(275, 521, 296, 497, 316, 480, 338, 473);
        shell.AddBezier(338, 473, 430, 470, 562, 470, 654, 472);
        shell.AddBezier(654, 472, 676, 478, 697, 493, 705, 500);
        shell.AddBezier(705, 500, 733, 536, 757, 569, 776, 590);
        shell.AddBezier(776, 590, 795, 611, 815, 632, 827, 634);
        shell.AddBezier(827, 634, 843, 636, 870, 619, 894, 597);
        shell.AddBezier(894, 597, 910, 578, 915, 550, 919, 523);
        shell.AddBezier(919, 523, 921, 482, 914, 436, 908, 405);
        shell.AddBezier(908, 405, 900, 369, 885, 320, 873, 283);
        shell.AddBezier(873, 283, 862, 246, 850, 210, 840, 181);
        shell.AddBezier(840, 181, 833, 164, 826, 154, 819, 148);
        shell.CloseFigure();

        using SolidBrush fill = new(Color.FromArgb(12, 16, 21));
        using Pen glow = new(Color.FromArgb(92, 218, 240, 235), 10f) { LineJoin = LineJoin.Round };
        using Pen outline = new(Color.FromArgb(238, 247, 248), 5f) { LineJoin = LineJoin.Round };
        graphics.DrawPath(glow, shell);
        graphics.FillPath(fill, shell);
        graphics.DrawPath(outline, shell);
    }

    private void DrawShoulders(Graphics graphics)
    {
        DrawRegion(graphics, Find(VirtualInput.LeftTrigger), Color.FromArgb(9, 13, 18), false);
        DrawRegion(graphics, Find(VirtualInput.LeftShoulder), Color.FromArgb(23, 29, 37), false);
        DrawRegion(graphics, Find(VirtualInput.RightTrigger), Color.FromArgb(9, 13, 18), false);
        DrawRegion(graphics, Find(VirtualInput.RightShoulder), Color.FromArgb(23, 29, 37), false);

        using Pen detail = DetailPen();
        using GraphicsPath leftEdge = new();
        leftEdge.AddBezier(215, 121, 245, 102, 281, 89, 317, 87);
        leftEdge.AddBezier(317, 87, 338, 87, 348, 95, 367, 115);
        graphics.DrawPath(detail, leftEdge);
        using GraphicsPath rightEdge = new();
        rightEdge.AddBezier(627, 114, 644, 98, 653, 83, 674, 84);
        rightEdge.AddBezier(674, 84, 707, 86, 748, 100, 780, 117);
        graphics.DrawPath(detail, rightEdge);
    }

    private static void DrawCenterPanel(Graphics graphics)
    {
        using GraphicsPath panel = new();
        panel.AddLine(369, 117, 385, 87);
        panel.AddBezier(385, 87, 440, 89, 554, 89, 608, 86);
        panel.AddLine(608, 86, 625, 115);
        panel.AddLine(625, 115, 576, 170);
        panel.AddBezier(576, 170, 571, 173, 567, 174, 563, 174);
        panel.AddLine(563, 174, 429, 175);
        panel.AddBezier(429, 175, 424, 175, 421, 173, 419, 172);
        panel.CloseFigure();
        using Pen detail = DetailPen();
        graphics.DrawPath(detail, panel);
    }

    private void DrawSticks(Graphics graphics)
    {
        DrawStickBase(graphics, 283, 231);
        DrawRegion(graphics, Find(VirtualInput.LeftStickUp), Color.FromArgb(18, 24, 31), false);
        DrawRegion(graphics, Find(VirtualInput.LeftStickRight), Color.FromArgb(18, 24, 31), false);
        DrawRegion(graphics, Find(VirtualInput.LeftStickDown), Color.FromArgb(18, 24, 31), false);
        DrawRegion(graphics, Find(VirtualInput.LeftStickLeft), Color.FromArgb(18, 24, 31), false);
        DrawStickRings(graphics, 283, 231);
        DrawRegion(graphics, Find(VirtualInput.LeftThumb), Color.FromArgb(16, 21, 27), true, "L3", 12f);

        DrawStickBase(graphics, 608, 354);
        DrawRegion(graphics, Find(VirtualInput.RightStickUp), Color.FromArgb(18, 24, 31), false);
        DrawRegion(graphics, Find(VirtualInput.RightStickRight), Color.FromArgb(18, 24, 31), false);
        DrawRegion(graphics, Find(VirtualInput.RightStickDown), Color.FromArgb(18, 24, 31), false);
        DrawRegion(graphics, Find(VirtualInput.RightStickLeft), Color.FromArgb(18, 24, 31), false);
        DrawStickRings(graphics, 608, 354);
        DrawRegion(graphics, Find(VirtualInput.RightThumb), Color.FromArgb(16, 21, 27), true, "R3", 12f);
    }

    private static void DrawStickBase(Graphics graphics, float centerX, float centerY)
    {
        using SolidBrush fill = new(Color.FromArgb(16, 21, 27));
        using Pen outline = FacePen();
        graphics.FillEllipse(fill, centerX - 64, centerY - 64, 128, 128);
        graphics.DrawEllipse(outline, centerX - 64, centerY - 64, 128, 128);
    }

    private static void DrawStickRings(Graphics graphics, float centerX, float centerY)
    {
        using Pen ring = new(Color.FromArgb(127, 139, 151), 2f);
        graphics.DrawEllipse(ring, centerX - 47, centerY - 47, 94, 94);
    }

    private void DrawDPad(Graphics graphics)
    {
        using GraphicsPath dpad = DPadPath();
        using SolidBrush fill = new(Color.FromArgb(16, 21, 27));
        using Pen outline = FacePen();
        graphics.FillPath(fill, dpad);
        graphics.DrawPath(outline, dpad);

        DrawRegion(graphics, Find(VirtualInput.DPadUp), Color.Transparent, false);
        DrawRegion(graphics, Find(VirtualInput.DPadRight), Color.Transparent, false);
        DrawRegion(graphics, Find(VirtualInput.DPadDown), Color.Transparent, false);
        DrawRegion(graphics, Find(VirtualInput.DPadLeft), Color.Transparent, false);
    }

    private void DrawFaceButtons(Graphics graphics)
    {
        DrawRegion(graphics, Find(VirtualInput.Y), Color.FromArgb(16, 21, 27), true, "Y", 20f);
        DrawRegion(graphics, Find(VirtualInput.X), Color.FromArgb(16, 21, 27), true, "X", 20f);
        DrawRegion(graphics, Find(VirtualInput.B), Color.FromArgb(16, 21, 27), true, "B", 20f);
        DrawRegion(graphics, Find(VirtualInput.A), Color.FromArgb(16, 21, 27), true, "A", 20f);
    }

    private void DrawCenterButtons(Graphics graphics)
    {
        DrawRegion(graphics, Find(VirtualInput.Guide), Color.FromArgb(16, 21, 27), true);
        DrawGuideGlyph(graphics);
        DrawRegion(graphics, Find(VirtualInput.Back), Color.FromArgb(16, 21, 27), true);
        using Pen detail = DetailPen(3f);
        graphics.DrawRectangle(detail, 424, 218, 13, 10);
        graphics.DrawRectangle(detail, 433, 225, 13, 10);
        DrawRegion(graphics, Find(VirtualInput.Start), Color.FromArgb(16, 21, 27), true);
        graphics.DrawLine(detail, 548, 219, 570, 219);
        graphics.DrawLine(detail, 548, 227, 570, 227);
        graphics.DrawLine(detail, 548, 235, 570, 235);
    }

    private static void DrawGuideGlyph(Graphics graphics)
    {
        using GraphicsPath glyph = new();
        glyph.AddBezier(473, 108, 484, 110, 492, 116, 498, 124);
        glyph.AddBezier(498, 124, 504, 116, 512, 110, 523, 108);
        glyph.AddBezier(523, 108, 514, 119, 506, 128, 501, 134);
        glyph.AddBezier(501, 134, 507, 143, 514, 151, 523, 158);
        glyph.AddBezier(523, 158, 512, 156, 504, 150, 498, 141);
        glyph.AddBezier(498, 141, 491, 150, 483, 156, 473, 158);
        glyph.AddBezier(473, 158, 482, 149, 490, 141, 495, 134);
        glyph.CloseFigure();
        using SolidBrush fill = new(Color.FromArgb(238, 247, 248));
        graphics.FillPath(fill, glyph);
    }

    private static void DrawFooter(Graphics graphics)
    {
        using Font font = new("Segoe UI", 12f, FontStyle.Bold);
        using SolidBrush brush = new(Color.FromArgb(135, 149, 164));
        using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        graphics.DrawString("25 EINZELN BELEGBARE XINPUT-AKTIONEN", font, brush, new RectangleF(250, 636, 500, 20), format);
    }

    private void DrawRegion(Graphics graphics, RegionInfo region, Color normalFill, bool outline, string? label = null, float labelSize = 12f)
    {
        bool selected = _selectedInput == region.Input;
        bool hovered = _hoveredInput == region.Input;
        Color fill = selected || hovered ? Color.FromArgb(37, 135, 255) : normalFill;

        if (fill.A > 0)
        {
            using SolidBrush brush = new(fill);
            graphics.FillPath(brush, region.Path);
        }

        if (outline)
        {
            using Pen pen = FacePen(selected || hovered ? Color.White : null);
            graphics.DrawPath(pen, region.Path);
        }

        if (!string.IsNullOrEmpty(label))
        {
            RectangleF bounds = region.Path.GetBounds();
            using Font font = new("Segoe UI", labelSize, FontStyle.Bold);
            using SolidBrush brush = new(Color.FromArgb(245, 248, 251));
            DrawCenteredText(graphics, label, font, brush, bounds);
        }
    }

    private RegionInfo Find(VirtualInput input) => _regions.First(region => region.Input == input);

    private RegionInfo? HitTest(Point location)
    {
        (float scale, float offsetX, float offsetY) = GetTransform();
        if (scale <= 0f) return null;
        PointF point = new((location.X - offsetX) / scale, (location.Y - offsetY) / scale);

        for (int i = _regions.Length - 1; i >= 0; i--)
        {
            if (_regions[i].Path.IsVisible(point)) return _regions[i];
        }

        return null;
    }

    private (float Scale, float OffsetX, float OffsetY) GetTransform()
    {
        float scale = Math.Min(ClientSize.Width / DesignWidth, ClientSize.Height / DesignHeight);
        float width = DesignWidth * scale;
        float height = DesignHeight * scale;
        return (scale, (ClientSize.Width - width) / 2f, (ClientSize.Height - height) / 2f);
    }

    private static RegionInfo[] CreateRegions() =>
    [
        new(VirtualInput.LeftTrigger, "Linker Trigger (LT)", LeftTriggerPath()),
        new(VirtualInput.LeftShoulder, "Linke Schultertaste (LB)", LeftBumperPath()),
        new(VirtualInput.RightTrigger, "Rechter Trigger (RT)", RightTriggerPath()),
        new(VirtualInput.RightShoulder, "Rechte Schultertaste (RB)", RightBumperPath()),
        new(VirtualInput.Guide, "Guide", EllipsePath(498, 132, 38)),
        new(VirtualInput.Back, "Back / View", EllipsePath(436, 227, 21)),
        new(VirtualInput.Start, "Start / Menu", EllipsePath(559, 227, 21)),
        new(VirtualInput.LeftStickUp, "Linker Stick hoch", StickSectorPath(283, 231, 270)),
        new(VirtualInput.LeftStickRight, "Linker Stick rechts", StickSectorPath(283, 231, 0)),
        new(VirtualInput.LeftStickDown, "Linker Stick runter", StickSectorPath(283, 231, 90)),
        new(VirtualInput.LeftStickLeft, "Linker Stick links", StickSectorPath(283, 231, 180)),
        new(VirtualInput.RightStickUp, "Rechter Stick hoch", StickSectorPath(608, 354, 270)),
        new(VirtualInput.RightStickRight, "Rechter Stick rechts", StickSectorPath(608, 354, 0)),
        new(VirtualInput.RightStickDown, "Rechter Stick runter", StickSectorPath(608, 354, 90)),
        new(VirtualInput.RightStickLeft, "Rechter Stick links", StickSectorPath(608, 354, 180)),
        new(VirtualInput.DPadUp, "Steuerkreuz hoch", DPadUpPath()),
        new(VirtualInput.DPadRight, "Steuerkreuz rechts", DPadRightPath()),
        new(VirtualInput.DPadDown, "Steuerkreuz runter", DPadDownPath()),
        new(VirtualInput.DPadLeft, "Steuerkreuz links", DPadLeftPath()),
        new(VirtualInput.Y, "Y-Taste", EllipsePath(711, 170, 31)),
        new(VirtualInput.X, "X-Taste", EllipsePath(647, 227, 31)),
        new(VirtualInput.B, "B-Taste", EllipsePath(769, 227, 31)),
        new(VirtualInput.A, "A-Taste", EllipsePath(712, 283, 31)),
        new(VirtualInput.LeftThumb, "Linken Stick drücken (L3)", EllipsePath(283, 231, 34)),
        new(VirtualInput.RightThumb, "Rechten Stick drücken (R3)", EllipsePath(608, 354, 34))
    ];

    private static GraphicsPath LeftTriggerPath() => Path(path =>
    {
        path.AddBezier(215, 121, 238, 96, 285, 76, 329, 68);
        path.AddBezier(329, 68, 344, 66, 360, 76, 382, 87);
        path.AddLine(382, 87, 372, 104);
        path.AddBezier(372, 104, 350, 84, 336, 77, 317, 79);
        path.AddBezier(317, 79, 278, 84, 241, 99, 215, 121);
    });

    private static GraphicsPath LeftBumperPath() => Path(path =>
    {
        path.AddBezier(215, 121, 245, 102, 281, 89, 317, 87);
        path.AddBezier(317, 87, 338, 87, 348, 95, 367, 115);
        path.AddLine(367, 115, 382, 87);
        path.AddLine(382, 87, 368, 118);
        path.AddBezier(368, 118, 350, 107, 331, 101, 306, 103);
        path.AddBezier(306, 103, 272, 106, 241, 115, 215, 121);
    });

    private static GraphicsPath RightTriggerPath() => Path(path =>
    {
        path.AddBezier(612, 85, 632, 75, 646, 64, 659, 65);
        path.AddBezier(659, 65, 703, 69, 758, 91, 780, 117);
        path.AddBezier(780, 117, 753, 95, 716, 80, 677, 77);
        path.AddBezier(677, 77, 658, 75, 644, 82, 624, 103);
    });

    private static GraphicsPath RightBumperPath() => Path(path =>
    {
        path.AddBezier(627, 114, 644, 98, 653, 83, 674, 84);
        path.AddBezier(674, 84, 707, 86, 748, 100, 780, 117);
        path.AddBezier(780, 117, 752, 111, 719, 103, 688, 101);
        path.AddBezier(688, 101, 663, 99, 645, 105, 627, 114);
    });

    private static GraphicsPath StickSectorPath(float centerX, float centerY, float rotation)
    {
        GraphicsPath path = new();
        path.AddArc(centerX - 64, centerY - 64, 128, 128, rotation - 45, 90);
        PointF innerEnd = PointOnCircle(centerX, centerY, 35, rotation + 45);
        PointF innerStart = PointOnCircle(centerX, centerY, 35, rotation - 45);
        path.AddLine(path.GetLastPoint(), innerEnd);
        path.AddArc(centerX - 35, centerY - 35, 70, 70, rotation + 45, -90);
        path.AddLine(innerStart, PointOnCircle(centerX, centerY, 64, rotation - 45));
        path.CloseFigure();
        return path;
    }

    private static PointF PointOnCircle(float centerX, float centerY, float radius, float angleDegrees)
    {
        double radians = angleDegrees * Math.PI / 180d;
        return new PointF(centerX + (float)(Math.Cos(radians) * radius), centerY + (float)(Math.Sin(radians) * radius));
    }

    private static GraphicsPath DPadPath() => Path(path =>
    {
        path.AddLine(370, 300, 408, 300);
        path.AddBezier(408, 300, 411, 300, 413, 302, 413, 305);
        path.AddLine(413, 340, 448, 340);
        path.AddBezier(448, 340, 451, 340, 453, 342, 453, 345);
        path.AddLine(453, 382, 453, 382);
        path.AddBezier(453, 382, 453, 385, 451, 387, 448, 387);
        path.AddLine(413, 387, 413, 422);
        path.AddBezier(413, 422, 413, 425, 411, 427, 408, 427);
        path.AddLine(370, 427, 370, 427);
        path.AddBezier(370, 427, 367, 427, 365, 425, 365, 422);
        path.AddLine(365, 387, 330, 387);
        path.AddBezier(330, 387, 327, 387, 325, 385, 325, 382);
        path.AddLine(325, 345, 325, 345);
        path.AddBezier(325, 345, 325, 342, 327, 340, 330, 340);
        path.AddLine(365, 340, 365, 305);
        path.AddBezier(365, 305, 365, 302, 367, 300, 370, 300);
    });

    private static GraphicsPath DPadUpPath() => PolygonPath((370, 300), (408, 300), (413, 305), (413, 340), (365, 340), (365, 305));
    private static GraphicsPath DPadRightPath() => PolygonPath((413, 340), (448, 340), (453, 345), (453, 382), (448, 387), (413, 387));
    private static GraphicsPath DPadDownPath() => PolygonPath((365, 387), (413, 387), (413, 422), (408, 427), (370, 427), (365, 422));
    private static GraphicsPath DPadLeftPath() => PolygonPath((330, 340), (365, 340), (365, 387), (330, 387), (325, 382), (325, 345));

    private static GraphicsPath PolygonPath(params (float X, float Y)[] points)
    {
        GraphicsPath path = new();
        path.AddPolygon(points.Select(point => new PointF(point.X, point.Y)).ToArray());
        return path;
    }

    private static GraphicsPath EllipsePath(float centerX, float centerY, float radius)
    {
        GraphicsPath path = new();
        path.AddEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);
        return path;
    }

    private static GraphicsPath Path(Action<GraphicsPath> build)
    {
        GraphicsPath path = new();
        path.StartFigure();
        build(path);
        path.CloseFigure();
        return path;
    }

    private static Pen FacePen(Color? color = null) => new(color ?? Color.FromArgb(238, 247, 248), 4f)
    {
        LineJoin = LineJoin.Round,
        StartCap = LineCap.Round,
        EndCap = LineCap.Round
    };

    private static Pen DetailPen(float width = 4f) => new(Color.FromArgb(223, 236, 239), width)
    {
        LineJoin = LineJoin.Round,
        StartCap = LineCap.Round,
        EndCap = LineCap.Round
    };

    private static void DrawCenteredText(Graphics graphics, string value, Font font, Brush brush, RectangleF bounds)
    {
        using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        graphics.DrawString(value, font, brush, bounds, format);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTip.Dispose();
            foreach (RegionInfo region in _regions) region.Path.Dispose();
        }
        base.Dispose(disposing);
    }

    private sealed record RegionInfo(VirtualInput Input, string ActionLabel, GraphicsPath Path);
}
