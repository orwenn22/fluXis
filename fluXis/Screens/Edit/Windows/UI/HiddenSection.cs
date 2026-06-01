using fluXis.Graphics.Sprites.Icons;
using fluXis.Graphics.Sprites.Text;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace fluXis.Screens.Edit.Windows.UI;

public partial class HiddenSection : CompositeDrawable
{
    public Drawable Content { get; init; }
    public string HeaderText { get; init; } = "Section";

    public bool Collapsed
    {
        get => collapsed;
        set
        {
            if (collapsed == value) return;

            collapsed = value;

            if (InternalChildren.Count == 1 && InternalChild is GridContainer gridContainer)
            {
                gridContainer.RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize), // header
                    new Dimension(collapsed ? GridSizeMode.Absolute : GridSizeMode.AutoSize, 0f), // content
                };
            }
        }
    }

    private bool collapsed = true;

    private HiddenSectionHeader header;

    public HiddenSection()
    {
        Masking = true;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        AutoSizeDuration = 300;
        AutoSizeEasing = Easing.Out;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChild = new GridContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Distributed),
            },
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize), // header
                new Dimension(collapsed ? GridSizeMode.Absolute : GridSizeMode.AutoSize, 0f), // content
            },
            Content = new[]
            {
                new Drawable[] { header = new HiddenSectionHeader(this, HeaderText) },
                new Drawable[] { Content }
            }
        };
    }

    public partial class HiddenSectionHeader : CompositeDrawable
    {
        private readonly HiddenSection section;
        private readonly FluXisSpriteIcon icon;

        public HiddenSectionHeader(HiddenSection section, string headerText)
        {
            this.section = section;

            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(4, 0),
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(16, 16),
                        Child = icon = new FluXisSpriteIcon()
                        {
                            Icon = FontAwesome6.Solid.AngleRight,
                            Rotation = section.Collapsed ? 0 : 90,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(14, 14),
                        },
                    },
                    new FluXisSpriteText()
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = headerText,
                        WebFontSize = 16,
                    }
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button != MouseButton.Left) return true; // consume anyway

            section.Collapsed = !section.Collapsed;

            icon.RotateTo(section.Collapsed ? 0 : 90, 300, Easing.Out);
            return true;
        }
    }
}
