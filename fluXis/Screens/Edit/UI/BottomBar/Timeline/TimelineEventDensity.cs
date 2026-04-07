using System;
using System.Linq;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Map.Structures.Events;
using fluXis.Map.Structures.Events.Camera;
using fluXis.Map.Structures.Events.Playfields;
using fluXis.Map.Structures.Events.Scrolling;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace fluXis.Screens.Edit.UI.BottomBar.Timeline;


public partial class TimelineEventDensity : FillFlowContainer
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
        Y = 24;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        clock.TrackChanged += trackChanged;
        map.RegisterAddListener<FlashEvent>(_ => updateDensity());
        map.RegisterUpdateListener<FlashEvent>(_ => updateDensity());
        map.RegisterRemoveListener<FlashEvent>(_ => updateDensity());

        map.RegisterAddListener<ColorFadeEvent>(_ => updateDensity());
        map.RegisterUpdateListener<ColorFadeEvent>(_ => updateDensity());
        map.RegisterRemoveListener<ColorFadeEvent>(_ => updateDensity());

        map.RegisterAddListener<PulseEvent>(_ => updateDensity());
        map.RegisterUpdateListener<PulseEvent>(_ => updateDensity());
        map.RegisterRemoveListener<PulseEvent>(_ => updateDensity());

        map.RegisterAddListener<PlayfieldMoveEvent>(_ => updateDensity());
        map.RegisterUpdateListener<PlayfieldMoveEvent>(_ => updateDensity());
        map.RegisterRemoveListener<PlayfieldMoveEvent>(_ => updateDensity());

        map.RegisterAddListener<PlayfieldScaleEvent>(_ => updateDensity());
        map.RegisterUpdateListener<PlayfieldScaleEvent>(_ => updateDensity());
        map.RegisterRemoveListener<PlayfieldScaleEvent>(_ => updateDensity());

        map.RegisterAddListener<PlayfieldRotateEvent>(_ => updateDensity());
        map.RegisterUpdateListener<PlayfieldRotateEvent>(_ => updateDensity());
        map.RegisterRemoveListener<PlayfieldRotateEvent>(_ => updateDensity());

        map.RegisterAddListener<LayerFadeEvent>(_ => updateDensity());
        map.RegisterUpdateListener<LayerFadeEvent>(_ => updateDensity());
        map.RegisterRemoveListener<LayerFadeEvent>(_ => updateDensity());

        map.RegisterAddListener<ShakeEvent>(_ => updateDensity());
        map.RegisterUpdateListener<ShakeEvent>(_ => updateDensity());
        map.RegisterRemoveListener<ShakeEvent>(_ => updateDensity());

        map.RegisterAddListener<ShaderEvent>(_ => updateDensity());
        map.RegisterUpdateListener<ShaderEvent>(_ => updateDensity());
        map.RegisterRemoveListener<ShaderEvent>(_ => updateDensity());

        map.RegisterAddListener<TimeOffsetEvent>(_ => updateDensity());
        map.RegisterUpdateListener<TimeOffsetEvent>(_ => updateDensity());
        map.RegisterRemoveListener<TimeOffsetEvent>(_ => updateDensity());

        map.RegisterAddListener<CameraMoveEvent>(_ => updateDensity());
        map.RegisterUpdateListener<CameraMoveEvent>(_ => updateDensity());
        map.RegisterRemoveListener<CameraMoveEvent>(_ => updateDensity());

        map.RegisterAddListener<CameraScaleEvent>(_ => updateDensity());
        map.RegisterUpdateListener<CameraScaleEvent>(_ => updateDensity());
        map.RegisterRemoveListener<CameraScaleEvent>(_ => updateDensity());

        map.RegisterAddListener<CameraRotateEvent>(_ => updateDensity());
        map.RegisterUpdateListener<CameraRotateEvent>(_ => updateDensity());
        map.RegisterRemoveListener<CameraRotateEvent>(_ => updateDensity());

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
                Colour = Colour4.FromHex("#FFFFFF"),
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

            var objects = map.MapEvents.Where(h => h.Time >= start && h.Time < end && h.GetType() != typeof(ScrollVelocity) && h.GetType() != typeof(NoteEvent)); // && h.GetType() != typeof(ScrollMultiplierEvent)
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

    private float getValue(ITimedObject hit)
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
