using System;
using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Panel;
using fluXis.Graphics.UserInterface.Text;
using fluXis.Overlay.Auth.UI;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace fluXis.Screens.Edit.UI;

public partial class EditorAddNotesToGroupPanel : Panel
{
    private FluXisTextBox textBox;

    public Action<string> OnSetGroup { get; set; }

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
                    Text = "Add notes to group",
                    WebFontSize = 32,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                textBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Group name"
                },
                new AuthOverlayButton("Set group") { Action = setGroup },
                new AuthOverlayButton("Cancel") { Action = Hide }
            }
        };
    }

    private void setGroup()
    {
        OnSetGroup?.Invoke(textBox.Text);
        Hide();
    }
}
