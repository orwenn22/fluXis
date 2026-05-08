using System.Collections.Generic;
using System.Linq;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace fluXis.Screens.Gameplay.Ruleset;

public partial class ScrollGroup : Component
{
    private readonly List<ScrollVelocity> velocities = new();
    private readonly List<AdditiveVelocity> additiveVelocities = new();
    private readonly List<double> marks = new();

    private record ScrollBoundary(double Time, double EffectiveSpeed);

    private readonly List<ScrollBoundary> boundaries = new();

    public double CurrentTime { get; private set; }
    public float ScrollMultiplier { get; set; } = 1;

    public override bool RemoveCompletedTransforms => false;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Origin = Anchor.Centre;
    }

    protected override void Update()
    {
        base.Update();
        CurrentTime = PositionFromTime(Clock.CurrentTime);
    }

    public double PositionFromTime(double time, int index = -1)
    {
        if (boundaries.Count == 0)
            return time;

        if (index == -1)
        {
            index = boundaries.Count; // default: past all boundaries

            for (var i = 0; i < boundaries.Count; i++)
            {
                if (time < boundaries[i].Time)
                {
                    index = i;
                    break;
                }
            }
        }

        if (index == 0)
        {
            return time; // no events yet, assume speed is 1
        }

        var prev = boundaries[index - 1];
        return marks[index - 1] + (time - prev.Time) * prev.EffectiveSpeed;
    }

    public void InitMarkers()
    {
        var hasEvents = velocities.Count > 0 || additiveVelocities.Count > 0;
        if (!hasEvents || marks.Count > 0)
            return;

        velocities.Sort((a, b) => a.Time.CompareTo(b.Time));
        additiveVelocities.Sort((a, b) => a.Time.CompareTo(b.Time));

        var events = velocities.Cast<ITimedObject>()
                               .Concat(additiveVelocities.Cast<ITimedObject>())
                               .OrderBy(e => e.Time)
                               .ThenBy(e => e is ScrollVelocity ? 0 : 1)
                               .ToList();

        double baseSV = 1.0;
        var activeOffsets = new Dictionary<string, double>();

        foreach (var ev in events)
        {
            if (ev is ScrollVelocity sv)
                baseSV = sv.Multiplier;
            else if (ev is AdditiveVelocity av)
                activeOffsets[av.EffectName] = av.VelocityOffset;

            var effectiveSpeed = (baseSV + activeOffsets.Values.Sum()) * ScrollMultiplier;
            boundaries.Add(new ScrollBoundary(ev.Time, effectiveSpeed));
        }

        marks.Add(boundaries[0].Time);

        for (var i = 1; i < boundaries.Count; i++)
        {
            var prev = boundaries[i - 1];
            var curr = boundaries[i];
            marks.Add(marks[i - 1] + (curr.Time - prev.Time) * prev.EffectiveSpeed);
        }
    }

    public List<ScrollVelocity> FlattenToScrollVelocities(string groupName)
    {
        return boundaries.Select(b => new ScrollVelocity
        {
            Time = b.Time,
            Multiplier = b.EffectiveSpeed,
            Groups = groupName != null ? new List<string> { groupName } : new List<string>()
        }).ToList();
    }

    public void AddVelocity(ScrollVelocity sv) => velocities.Add(sv);

    public void AddAdditiveVelocity(AdditiveVelocity av) => additiveVelocities.Add(av);
}
