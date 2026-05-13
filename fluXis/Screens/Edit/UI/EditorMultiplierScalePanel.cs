using System;
using System.Globalization;
using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Panel;
using fluXis.Graphics.UserInterface.Text;
using fluXis.Overlay.Auth.UI;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace fluXis.Screens.Edit.UI;

public partial class EditorMultiplierScalePanel : Panel
{
    private FluXisTextBox multiplierTextBox;

    public Action<double> OnScale { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Width = 420;
        AutoSizeAxes = Axes.Y;
        Content.RelativeSizeAxes = Axes.X;
        Content.AutoSizeAxes = Axes.Y;

        Content.Child = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding(20),
            Spacing = new Vector2(10),
            Children = new Drawable[]
            {
                new FluXisSpriteText
                {
                    Text = "Scale SV/AV",
                    WebFontSize = 32,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                multiplierTextBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Multiplier"
                },
                new AuthOverlayButton("Scale") { Action = scale },
                new AuthOverlayButton("Cancel") { Action = Hide }
            }
        };
    }

    private void scale()
    {
        if (!double.TryParse(multiplierTextBox.Text, CultureInfo.InvariantCulture, out double multiplier))
        {
            return;
        }

        OnScale?.Invoke(multiplier);
        Hide();
    }
}
