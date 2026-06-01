using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Graphics.UserInterface.Color;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace fluXis.Screens.Edit.Windows.UI;

public partial class PreviewGraph : CompositeDrawable
{
    private FillFlowContainer topContainer;
    private FillFlowContainer bottomContainer;

    public PreviewGraph()
    {
        RelativeSizeAxes = Axes.X;
    }

    [BackgroundDependencyLoader]
    void load()
    {
        AutoSizeAxes = Axes.Y; // TODO: we might not want to force this, but that would require changing more stuff

        InternalChild = new Container
        {
            Masking = true,
            CornerRadius = 10,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Theme.Background1,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        topContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 176,
                            Direction = FillDirection.Horizontal,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                            Colour = Theme.Text,
                        },
                        bottomContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 176,
                            Direction = FillDirection.Horizontal,
                        }
                    }
                }
            }
        };
    }

    public void UpdateContent(List<double> values, Colour4 colour)
    {
        var maxScale = Math.Max(Math.Abs(values.Min()), Math.Abs(values.Max()));

        if (maxScale == 0) maxScale = 1;

        topContainer.FadeColour(colour, 200, Easing.Out);
        bottomContainer.FadeColour(colour, 200, Easing.Out);

        if (topContainer.Count == values.Count)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var value = values.ElementAt(i);
                topContainer[i].ResizeHeightTo((value <= 0) ? 0f : (float)(value / maxScale), 200, Easing.Out);
                bottomContainer[i].ResizeHeightTo((value >= 0) ? 0f : (float)(-value / maxScale), 200, Easing.Out);
            }

            return;
        }

        topContainer.Clear();
        bottomContainer.Clear();

        foreach (var value in values)
        {
            topContainer.Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Width = 1f / values.Count,
                Height = (value <= 0) ? 0f : (float)(value / maxScale),
                Colour = Colour4.White
            });

            bottomContainer.Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Width = 1f / values.Count,
                Height = (value >= 0) ? 0f : (float)(-value / maxScale),
                Colour = Colour4.White
            });
        }
    }
}
