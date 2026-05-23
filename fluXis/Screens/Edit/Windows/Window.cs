using fluXis.Graphics.Sprites.Icons;
using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Buttons;
using fluXis.Graphics.UserInterface.Color;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace fluXis.Screens.Edit.Windows;

public partial class Window : CompositeDrawable
{
    private const float titlebar_height = 28f;
    private const float content_margin = 3f;
    private Container contentContainer;

    public Drawable Content
    {
        set
        {
            content = value;
            if (contentContainer != null) contentContainer.Child = content;
        }
        get => content;
    }

    private Drawable content;

    public bool Minimized
    {
        get => minimized;
        set
        {
            if (minimized == value) return;

            minimized = value;

            if (InternalChild is GridContainer gridContainer)
            {
                gridContainer.RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, titlebar_height + content_margin), // title bar
                    new Dimension(minimized ? GridSizeMode.Absolute : GridSizeMode.AutoSize, 0f), // main content
                };
            }
        }
    }

    private bool minimized = false;

    public Colour4 OutlineColour
    {
        get => outlineColour;
        set
        {
            outlineColour = value;
            titleBar?.SetBackgroundColour(outlineColour);
            if (backgroundBox != null) backgroundBox.Colour = outlineColour;
        }
    }

    private Colour4 outlineColour = Theme.Background1;

    public string Title { get; set; }

    protected WindowContainer ParentContainer => Parent as WindowContainer;

    private TitleBar titleBar;
    private Box backgroundBox;

    [BackgroundDependencyLoader]
    void load()
    {
        AutoSizeAxes = Axes.Both;
        AutoSizeDuration = 300;
        AutoSizeEasing = Easing.Out;
        Masking = true;
        CornerRadius = 10 + content_margin;

        InternalChild = new GridContainer
        {
            AutoSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
            },
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.Absolute, titlebar_height + content_margin), // title bar
                new Dimension(minimized ? GridSizeMode.Absolute : GridSizeMode.AutoSize, 0f), // main content
            },
            Content = new[]
            {
                new Drawable[]
                {
                    titleBar = new TitleBar(this)
                },
                new Drawable[]
                {
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            backgroundBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = outlineColour
                            },
                            new Container
                            {
                                Masking = true,
                                CornerRadius = 10,
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Top = 0, Left = content_margin, Right = content_margin, Bottom = content_margin }, // top margin is part of title bar
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Theme.Background2
                                    },
                                    contentContainer = new Container
                                    {
                                        AutoSizeAxes = Axes.Both,
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        if (content != null)
            contentContainer.Child = content;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        ParentContainer.BringToFront(this);
        return true;
    }

    private partial class TitleBar : Container
    {
        private readonly Window window;

        private readonly Box backgroundBox;
        private readonly IconButton minimizeButton;
        private readonly FluXisSpriteText titleText;
        private readonly IconButton closeButton;

        public TitleBar(Window window)
        {
            this.window = window;
            Colour4 foregroundColour = Theme.IsBright(window.outlineColour) ? Theme.TextDark : Theme.Text;
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                backgroundBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = window.OutlineColour
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Width = titlebar_height + content_margin,
                            Height = titlebar_height + content_margin,
                            Child = minimizeButton = new IconButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Icon = FontAwesome6.Solid.AngleRight,
                                ButtonSize = 24,
                                IconSize = 13,
                                Rotation = window.Minimized ? 0f : 90f,
                                Colour = foregroundColour,
                                Action = () =>
                                {
                                    window.Minimized = !window.Minimized;
                                    minimizeButton.RotateTo(window.Minimized ? 0f : 90f, 300, Easing.Out);
                                }
                            }
                        },
                        titleText = new FluXisSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = window.Title,
                            WebFontSize = 18,
                            Colour = foregroundColour
                        },
                    }
                },
                closeButton = new IconButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Icon = FontAwesome6.Solid.XMark,
                    ButtonSize = 24,
                    IconSize = 13,
                    X = -4,
                    Colour = foregroundColour,
                    Action = () => window.ParentContainer.Close(window)
                }
            };
        }

        public void SetBackgroundColour(Colour4 colour)
        {
            backgroundBox.Colour = colour;
            Colour4 foregroundColour = Theme.IsBright(colour) ? Theme.TextDark : Theme.Text;
            minimizeButton.Colour = foregroundColour;
            titleText.Colour = foregroundColour;
            closeButton.Colour = foregroundColour;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            window.ParentContainer.SetDraggedWindow(window, e.MouseDownPosition);
            window.ParentContainer.BringToFront(window);
            return true;
        }
    }
}
