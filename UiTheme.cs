namespace train_automation;

/// <summary>Shared RailBot Pro palette from the Stitch design.</summary>
public static class UiTheme
{
    public static readonly Color Primary = Color.FromArgb(211, 47, 47);       // Red
    public static readonly Color PrimaryDark = Color.FromArgb(183, 28, 28);   // Dark Red
    public static readonly Color PrimaryContainer = Color.FromArgb(211, 47, 47);
    public static readonly Color OnPrimaryContainer = Color.White;
    public static readonly Color PageBg = Color.FromArgb(18, 18, 18);         // Dark gray/black
    public static readonly Color Surface = Color.FromArgb(30, 30, 30);
    public static readonly Color SurfaceLowest = Color.Black;
    public static readonly Color SurfaceLow = Color.FromArgb(24, 24, 24);
    public static readonly Color SurfaceContainer = Color.FromArgb(36, 36, 36);
    public static readonly Color SurfaceHigh = Color.FromArgb(44, 44, 44);
    public static readonly Color Border = Color.FromArgb(51, 51, 51);
    public static readonly Color OutlineVariant = Color.FromArgb(68, 68, 68);
    public static readonly Color Text = Color.White;
    public static readonly Color TextMuted = Color.FromArgb(170, 170, 170);
    public static readonly Color TextSecondary = Color.FromArgb(204, 204, 204);
    public static readonly Color Success = Color.FromArgb(39, 174, 96);
    public static readonly Color Warning = Color.FromArgb(242, 153, 74);
    public static readonly Color Danger = Color.FromArgb(235, 87, 87);
    public static readonly Color DangerDark = Color.FromArgb(186, 26, 26);

    public static Font HeadlineMd => new("Segoe UI", 10F, FontStyle.Bold);
    public static Font HeadlineLg => new("Segoe UI", 12F, FontStyle.Bold);
    public static Font BodySm => new("Segoe UI", 8.25F);
    public static Font LabelMd => new("Segoe UI", 8.25F, FontStyle.Bold);
    public static Font LabelSm => new("Segoe UI", 7.5F, FontStyle.Bold);

    public static Button CreatePrimaryButton(string text, int width = 140, int height = 36)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(width, height),
            BackColor = Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = LabelMd,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    public static Button CreateSecondaryButton(string text, int width = 120, int height = 36)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(width, height),
            BackColor = SurfaceLowest,
            ForeColor = Text,
            FlatStyle = FlatStyle.Flat,
            Font = LabelMd,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = Border;
        return btn;
    }

    public static Button CreateNavButton(string text, bool active)
    {
        var btn = new Button
        {
            Text = "  " + text,
            Height = 36,
            Width = 184,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = LabelMd,
            Cursor = Cursors.Hand,
            Padding = new Padding(8, 0, 0, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        ApplyNavStyle(btn, active);
        return btn;
    }

    public static void ApplyNavStyle(Button btn, bool active)
    {
        if (active)
        {
            btn.BackColor = PrimaryContainer;
            btn.ForeColor = OnPrimaryContainer;
        }
        else
        {
            btn.BackColor = SurfaceLow;
            btn.ForeColor = TextMuted;
        }
    }

    public static Label CreateCaption(string text) => new()
    {
        Text = text.ToUpperInvariant(),
        AutoSize = true,
        Font = LabelSm,
        ForeColor = TextMuted
    };

    public static Panel CreateCard() => new()
    {
        BackColor = SurfaceLowest,
        BorderStyle = BorderStyle.FixedSingle
    };
}
