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

public partial class EditorScrollGroupChoicePanel : Panel
{
    private readonly string title;
    private FluXisTextBox textBox;

    public Action<string> OnConfirm { get; set; }

    public EditorScrollGroupChoicePanel(string title = "Add notes to group")
    {
        this.title = title;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Width = 580;
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
                    Text = title,
                    WebFontSize = 32,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                textBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Group name"
                },
                new AuthOverlayButton("Confirm") { Action = confirm },
                new AuthOverlayButton("Cancel") { Action = Hide }
            }
        };
    }

    private void confirm()
    {
        OnConfirm?.Invoke(textBox.Text);
        Hide();
    }
}
