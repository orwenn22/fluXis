using System;
using fluXis.Input;
using fluXis.Map.Structures;
using fluXis.Screens.Gameplay.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace fluXis.Screens.Gameplay.Ruleset.HitObjects;

public partial class DrawableTickNote : DrawableHitObject
{
    public override bool CanBeRemoved => Judged || wouldMiss;

    private bool wouldMiss => Time.Current - Data.Time > HitWindows.TimingFor(HitWindows.LowestHitable);

    [Resolved]
    private GameplayInput input { get; set; }

    private Circle followLine;

    public DrawableTickNote(HitObject data)
        : base(data)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new[]
        {
            followLine = new Circle
            {
                BypassAutoSizeAxes = Axes.Both,
                Colour = Colour4.FromHex("#F2C979").Opacity(.4f),
                Size = new Vector2(8),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            },
            Skin.GetTaikoTickNote().With(x => x.RelativeSizeAxes = Axes.X)
        };
    }

    protected override void Update()
    {
        base.Update();

        if (Data.VisualLane > 1)
            X = ObjectManager.PositionAtLane(Data.VisualLane);

        var next = Data.NextObject;

        if (next?.Type == HitObjectType.Tick && next.Lane == Data.Lane && (Data.VisualLane != 0 || next.VisualLane != 0))
        {
            var l = next.VisualLane == 0 ? next.Lane : next.VisualLane;
            var pos = Column.FullPositionAt(next.Time, l, next.ScrollGroup, next.StartEasing);
            var delta = pos - Position;
            var distance = delta.Length;

            followLine.Alpha = 1;
            followLine.Position = delta / 2;
            followLine.Height = distance;
            followLine.Rotation = -(float)(Math.Atan2(delta.X, delta.Y) * (180 / Math.PI));
        }
        else
            followLine.Alpha = 0;
    }

    protected override void CheckJudgement(bool byUser, double offset)
    {
        if (!byUser)
        {
            ApplyResult(HitWindows.TimingFor(HitWindows.Lowest));
            return;
        }

        if (HitWindows.CanBeHit(offset))
            ApplyResult(offset);
    }

    public override void OnPressed(FluXisGameplayKeybind key)
    {
        if (!Column.IsFirst(this))
            return;

        UpdateJudgement(true);
    }
}
