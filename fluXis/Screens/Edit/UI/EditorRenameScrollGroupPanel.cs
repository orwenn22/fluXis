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

public partial class EditorRenameScrollGroupPanel : Panel
{
    private FluXisTextBox oldNameTextBox;
    private FluXisTextBox newNameTextBox;

    public Action<string, string> OnRename { get; set; }

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
                    Text = "Rename scroll group",
                    WebFontSize = 32,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                oldNameTextBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Old name"
                },
                newNameTextBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "New name"
                },
                new AuthOverlayButton("Rename group") { Action = setGroup },
                new AuthOverlayButton("Cancel") { Action = Hide }
            }
        };
    }

    private void setGroup()
    {
        OnRename?.Invoke(oldNameTextBox.Text, newNameTextBox.Text);
        Hide();
    }
}
