using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Map.Structures.Events.Scrolling;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace fluXis.Screens.Gameplay.Ruleset;

public partial class ScrollGroup : Component
{
    private readonly List<ScrollVelocity> velocities = new();
    private readonly Dictionary<ScrollMultiplierEvent, Transform> scrollMultiplierTransforms = new();
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
            int idx = boundaries.BinarySearch(new ScrollBoundary(time, 0), Comparer<ScrollBoundary>.Create((a, b) => a.Time.CompareTo(b.Time)));

            if (idx < 0) idx = ~idx;
            else
            {
                // exact match found; need to advance past any equal-Time entries
                while (idx < boundaries.Count && boundaries[idx].Time == time) idx++;
            }

            index = idx;
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
        if (!hasEvents)
            return;

        marks.Clear();
        boundaries.Clear();

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

    public void RemoveVelocity(ScrollVelocity sv) => velocities.Remove(sv);
    public void RemoveAdditiveVelocity(AdditiveVelocity av) => additiveVelocities.Remove(av);

    public void AddScrollMultiplier(ScrollMultiplierEvent sm)
    {
        if (HasEvent(sm))
            throw new Exception("Scroll multiplier already in scroll group"); // update existing instead of throwing?

        using (BeginAbsoluteSequence(sm.Time))
        {
            var transform = this.MakeTransform(nameof(ScrollMultiplier), sm.Multiplier, Math.Max(sm.Duration, 0), sm.Easing);
            this.TransformTo(transform);
            scrollMultiplierTransforms[sm] = transform;
        }

        // the thing above implicitly removed every transform passed the newly added one, so we need to reapply all the others
        foreach (var oldSm in scrollMultiplierTransforms.Keys.Where(e => e.Time >= sm.Time && e != sm).OrderBy(e => e.Time))
        {
            using (BeginAbsoluteSequence(oldSm.Time))
            {
                var transform = this.MakeTransform(nameof(ScrollMultiplier), oldSm.Multiplier, Math.Max(oldSm.Duration, 0), oldSm.Easing);
                this.TransformTo(transform);
                scrollMultiplierTransforms[oldSm] = transform;
            }
        }
    }

    public void RemoveScrollMultiplier(ScrollMultiplierEvent sm)
    {
        if (scrollMultiplierTransforms.TryGetValue(sm, out var transform))
        {
            RemoveTransform(transform);
            scrollMultiplierTransforms.Remove(sm);

            if (scrollMultiplierTransforms.Count == 0) ScrollMultiplier = 1;
        }
        else throw new Exception("Scroll multiplier transform not found, this might be because the ScrollMultiplierEvent wasn't added to the scroll group");
    }

    public void UpdateScrollMultiplier(ScrollMultiplierEvent sm)
    {
        RemoveScrollMultiplier(sm);
        AddScrollMultiplier(sm);
    }

    public void AddEvent(IHasGroups ev)
    {
        if (ev is AdditiveVelocity av) AddAdditiveVelocity(av);
        else if (ev is ScrollVelocity sv) AddVelocity(sv);
        else if (ev is ScrollMultiplierEvent sm) AddScrollMultiplier(sm);
        else throw new Exception("Unsupported event type");
    }

    public void RemoveEvent(IHasGroups ev)
    {
        if (ev is AdditiveVelocity av) RemoveAdditiveVelocity(av);
        else if (ev is ScrollVelocity sv) RemoveVelocity(sv);
        else if (ev is ScrollMultiplierEvent sm) RemoveScrollMultiplier(sm);
        else throw new Exception("Unsupported event type");
    }

    // only works for SVs and AVs
    public bool HasEvent(IHasGroups ev)
    {
        if (ev is AdditiveVelocity av) return additiveVelocities.Contains(av);
        if (ev is ScrollVelocity sv) return velocities.Contains(sv);
        if (ev is ScrollMultiplierEvent sm) return scrollMultiplierTransforms.ContainsKey(sm);

        return false;
    }

    public bool HasEvents() => velocities.Count > 0 || additiveVelocities.Count > 0 || scrollMultiplierTransforms.Count > 0;
}
