using System;
using System.Linq;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace fluXis.Screens.Edit.UI.BottomBar.Timeline;

public partial class TimelineSVDensity : FillFlowContainer
{
    [Resolved]
    private EditorClock clock { get; set; }

    [Resolved]
    private EditorMap map { get; set; }

    private DrawableTrack track => clock.Track.Value;

    private const int sections = 200;
    private const float section_width = 1f / sections;

    private float sectionLength => (float)(track.Length / sections);

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        Size = new Vector2(1, 6);
        Anchor = Origin = Anchor.Centre;
        CornerRadius = 3;
        Masking = true;
        Y = 16;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        clock.TrackChanged += trackChanged;
        map.RegisterAddListener<ScrollVelocity>(_ => updateDensity());
        map.RegisterUpdateListener<ScrollVelocity>(_ => updateDensity());
        map.RegisterRemoveListener<ScrollVelocity>(_ => updateDensity());

        trackChanged(track);
    }

    private void trackChanged(DrawableTrack track)
    {
        Clear();

        for (var i = 0; i < sections; i++)
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Width = section_width,
                Colour = Theme.ScrollVelocity,
                AlwaysPresent = true,
                Alpha = 0
            });
        }

        updateDensity();
    }

    private void updateDensity()
    {
        var counts = new float[sections];

        for (int i = 0; i < sections; i++)
        {
            var start = sectionLength * i;
            var end = start + sectionLength;

            var objects = map.MapInfo.ScrollVelocities.Where(h => h.Time >= start && h.Time < end);
            counts[i] = (float)Math.Log2(objects.Sum(getValue) + 1);
        }

        var highest = counts.Max();
        var percentages = counts.Select(c => c / highest).ToArray();

        for (var i = 0; i < sections; i++)
        {
            var box = this[i];
            box.Alpha = percentages[i];
        }
    }

    private float getValue(ScrollVelocity hit)
    {
        return 1f;
    }

    protected override bool OnHover(HoverEvent e)
    {
        this.ResizeHeightTo(12, 300, Easing.OutQuint);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        this.ResizeHeightTo(6, 600, Easing.OutQuint);
    }
}
