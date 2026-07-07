using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Map;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Scoring.Enums;
using fluXis.Scoring.Processing;
using fluXis.Scoring.Processing.Health;
using fluXis.Scoring.Structs;
using fluXis.Screens.Gameplay.Ruleset.Playfields;
using fluXis.Utils;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osuTK;

namespace fluXis.Screens.Gameplay.Ruleset.HitObjects;

public partial class HitObjectColumn : Container<DrawableHitObject>
{
    private const float minimum_loaded_hit_objects = 3;

    [Resolved]
    public Playfield Playfield { get; private set; }

    [Resolved]
    private JudgementProcessor judgementProcessor { get; set; }

    [CanBeNull]
    [Resolved(CanBeNull = true)]
    private HealthProcessor healthProcessor { get; set; }

    [Resolved]
    private RulesetData rulesetData { get; set; }

    public List<HitObject> PastHitObjects { get; } = new();
    public List<HitObject> FutureHitObjects { get; } = new();
    public List<DrawableHitObject> VisibleHitObjects { get; } = new();

    public bool Finished => VisibleHitObjects.Count == 0 && FutureHitObjects.Count == 0;

    [CanBeNull]
    public HitObject NextUp
    {
        get
        {
            if (VisibleHitObjects.Count > 0)
                return VisibleHitObjects[0].Data;

            return FutureHitObjects.Count > 0 ? FutureHitObjects[0] : null;
        }
    }

    public MapInfo Map { get; }
    public HitObjectManager HitManager { get; }
    public int Lane { get; }
    public int Index { get; }

    public SnapIndices SnapIndices;

    public ScrollGroup DefaultScrollGroup => rulesetData.ScrollGroups[$"${Index}"]; // if we truly want to cache it like before, don't do it in the constructor

    private DependencyContainer dependencies;

    public HitObjectColumn(MapInfo map, RulesetData rulesetData, HitObjectManager hitManager, int lane)
    {
        Map = map;
        Lane = lane;
        HitManager = hitManager;
        SnapIndices = new SnapIndices(map);

        var idx = Lane;

        if (map.IsSplit && idx > map.RealmEntry!.KeyCount)
            idx -= map.RealmEntry!.KeyCount;

        Index = idx;

        var objects = Map.HitObjects.Where(h => h.Lane == Lane).ToList();
        objects.Sort((a, b) => a.Time.CompareTo(b.Time));
        objects.ForEach(FutureHitObjects.Add);

        HitObject last = null;

        foreach (var hit in FutureHitObjects)
        {
            if (last != null)
                last.NextObject = hit;

            hit.StartEasing = HitManager.EasingAtTime(hit.Time);
            hit.EndEasing = HitManager.EasingAtTime(hit.EndTime);
            last = hit;

            updateHitScrollGroup(hit, rulesetData);
        }

        SnapIndices.InitSnapIndices();
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        dependencies.CacheAs(this);

        rulesetData.OnSvAdd += onEventChange;
        rulesetData.OnSvRemove += onEventChange;
        rulesetData.OnSvUpdate += onEventChange;
        rulesetData.OnAvAdd += onEventChange;
        rulesetData.OnAvRemove += onEventChange;
        rulesetData.OnAvUpdate += onEventChange;
        rulesetData.OnHitObjectAddAction += addHitObject;
        rulesetData.OnHitObjectRemoveAction += removeHitObject;
        rulesetData.OnHitObjectUpdateAction += moveHitObject;
        rulesetData.OnTimingPointChangeAction += onTimingPointChange;
    }

    protected override void Dispose(bool isDisposing)
    {
        if (IsDisposed) return;

        rulesetData.OnSvAdd -= onEventChange;
        rulesetData.OnSvRemove -= onEventChange;
        rulesetData.OnSvUpdate -= onEventChange;
        rulesetData.OnAvAdd -= onEventChange;
        rulesetData.OnAvRemove -= onEventChange;
        rulesetData.OnAvUpdate -= onEventChange;
        rulesetData.OnHitObjectAddAction -= addHitObject;
        rulesetData.OnHitObjectRemoveAction -= removeHitObject;
        rulesetData.OnHitObjectUpdateAction -= moveHitObject;
        rulesetData.OnTimingPointChangeAction -= onTimingPointChange;

        base.Dispose(isDisposing);
    }

    protected override void Update()
    {
        base.Update();
        int i;

        // spawn newly visible notes
        for (i = 0; i < FutureHitObjects.Count; ++i)
        {
            var hit = FutureHitObjects[i];

            if (VisibleHitObjects.Count < minimum_loaded_hit_objects || ShouldDisplay(hit.Time, hit.ScrollGroup))
            {
                visibleHitObjectSortedAdd(hit);
                FutureHitObjects.RemoveAt(i);
                --i;
            }
        }

        // ensure the notes with the 3 lowest times are loaded
        if (VisibleHitObjects.Count > 0)
        {
            var earliestVisible = VisibleHitObjects[VisibleHitObjects.Count >= 3 ? 2 : 0].Data; // trust

            for (i = 0; i < minimum_loaded_hit_objects && i < FutureHitObjects.Count; ++i)
            {
                var hit = FutureHitObjects[i];

                if (hit.Time <= earliestVisible.Time)
                {
                    visibleHitObjectSortedAdd(hit);
                    FutureHitObjects.RemoveAt(i);
                    --i;
                }
            }
        }

        i = -1;

        foreach (var hitObject in VisibleHitObjects.ToList())
        {
            ++i;

            if (hitObject.CanBeRemoved)
            {
                removeVisibleHitObject(hitObject);
                continue;
            }

            if (i < minimum_loaded_hit_objects) // make sure we don't unload the earliest next notes
                continue;

            if (!ShouldDisplay(VisibleHitObjects.Last().Data.Time, VisibleHitObjects.Last().Data.ScrollGroup))
                removeVisibleHitObject(hitObject, true);
        }

        // reverting
        while (rulesetData.AllowReverting && PastHitObjects.Count > 0)
        {
            var lastObject = PastHitObjects.Last();
            var result = lastObject.Result;

            // TODO: make it so objects get appropriate results in charting tab, and revert this
            if (result is null)
            {
                if (Clock.CurrentTime >= lastObject.Time)
                    break;
            }
            else
            {
                if (Clock.CurrentTime >= result.Value.Time)
                    break;
            }

            PastHitObjects.Remove(lastObject);
            revertHitObject(lastObject);
        }
    }

    public bool ShouldDisplay(double time, [CanBeNull] ScrollGroup group = null)
    {
        group ??= DefaultScrollGroup;

        var svTime = group.PositionFromTime(time);
        var y = PositionAtTime(svTime, group);
        return y >= 0 && y < DrawHeight * 2;
    }

    public Vector2 FullPositionAt(double time, float lane, [CanBeNull] ScrollGroup group = null, Easing ease = Easing.None)
        => new(HitManager.PositionAtLane(lane), PositionAtTime(time, group, ease));

    public float PositionAtTime(double time, [CanBeNull] ScrollGroup group = null, Easing ease = Easing.None)
    {
        group ??= DefaultScrollGroup;

        var pos = HitManager.HitPosition;
        var current = group.CurrentTime + HitManager.VisualTimeOffset;
        var y = (float)(pos - .5f * ((time - (float)current) * (HitManager.ScrollSpeed * group.ScrollMultiplier)));

        if (ease <= Easing.None || y < 0 || y > pos)
            return y;

        var progress = y / pos;
        y = Interpolation.ValueAt(progress, 0, pos, 0, 1, ease);
        return float.IsFinite(y) ? y : 0;
    }

    public bool IsFirst(DrawableHitObject hitObject) => VisibleHitObjects.FirstOrDefault(h => h.Data.Lane == hitObject.Data.Lane && h.Data.Time < hitObject.Data.Time) == null;

    private void visibleHitObjectSortedAdd(HitObject hit)
    {
        var d = createHitObject(hit);
        int index = VisibleHitObjects.BinarySearch(d, Comparer<DrawableHitObject>.Create((a, b) => a.Data.Time.CompareTo(b.Data.Time)));
        if (index < 0) index = ~index;

        VisibleHitObjects.Insert(index, d);
        AddInternal(d);
    }

    private void revertHitObject(HitObject hit)
    {
        if (!Playfield.IsSubPlayfield)
        {
            if (hit.HoldEndResult is not null)
                judgementProcessor.RevertResult(hit.HoldEndResult.Value);

            if (hit.Result is not null)
                judgementProcessor.RevertResult(hit.Result.Value);
        }

        var draw = createHitObject(hit);
        VisibleHitObjects.Insert(0, draw);
        AddInternal(draw);

        // if (ShouldDisplay(hit.Time, hit.ScrollGroup))
        //    visibleHitObjectSortedAdd(hit);
        // else
        // {
        //    int index = FutureHitObjects.BinarySearch(hit, Comparer<HitObject>.Create((a, b) => a.Time.CompareTo(b.Time)));
        //    if (index < 0) index = ~index;
        //    FutureHitObjects.Insert(index, hit);
        // }
    }

    private DrawableHitObject createHitObject(HitObject data)
    {
        var draw = HitManager.CreateHitObject(data);
        draw.OnHit += hit;
        return draw;
    }

    private void removeVisibleHitObject(DrawableHitObject hitObject, bool addToFuture = false)
    {
        if (!addToFuture)
        {
            hitObject.OnKill();
        }

        hitObject.OnHit -= hit;

        VisibleHitObjects.Remove(hitObject);

        if (addToFuture)
        {
            int index = FutureHitObjects.BinarySearch(hitObject.Data, Comparer<HitObject>.Create((a, b) => a.Time.CompareTo(b.Time)));
            if (index < 0) index = ~index;
            FutureHitObjects.Insert(index, hitObject.Data);
        }
        else
        {
            int index = PastHitObjects.BinarySearch(hitObject.Data, Comparer<HitObject>.Create((a, b) => a.Time.CompareTo(b.Time)));
            if (index < 0) index = ~index;
            PastHitObjects.Insert(index, hitObject.Data);
        }

        RemoveInternal(hitObject, true);
    }

    private void hit(DrawableHitObject hitObject, double difference)
    {
        if (Playfield.IsSubPlayfield)
            return;

        // since judged is only set after hitting the tail this works
        var isHoldEnd = hitObject is DrawableLongNote { Judged: true };

        var isLandmine = hitObject is DrawableLandmine;

        var hitWindows =
            isHoldEnd ? rulesetData.ReleaseWindows :
            isLandmine ? rulesetData.LandmineWindows :
            rulesetData.HitWindows;

        var judgement = hitWindows.JudgementFor(difference);

        if (healthProcessor is { Failed: true })
            return;

        var resultType =
            isHoldEnd ? ResultType.HoldEnd :
            isLandmine ? ResultType.Landmine :
            ResultType.Hit;

        var result = new HitResult(Time.Current, difference, judgement, resultType);
        judgementProcessor.AddResult(result);

        if (isHoldEnd)
            hitObject.Data.HoldEndResult = result;
        else
            hitObject.Data.Result = result;
    }

    // called whenever an IHasGroups event is added, removed or modified (excluding ScrollMultiplierEvents)
    private void onEventChange(IHasGroups ev)
    {
        //TODO: put everything that is in HitObjects and past the event in FutureHitObject?

        foreach (var hitObject in VisibleHitObjects)
        {
            updateHitScrollGroup(hitObject.Data, rulesetData); // in case the object's group is created/deleted

            // all things considered that check might not even be worth it
            // if (ev.Groups.Contains(hitObject.Data.Group) || (string.IsNullOrEmpty(hitObject.Data.Group) && ev.Groups.Contains($"${Lane}")))
            // {
            hitObject.UpdateScrollVelocityTime();
            // }
        }

        foreach (var hitObject in FutureHitObjects)
            updateHitScrollGroup(hitObject, rulesetData);

        foreach (var hitObject in PastHitObjects)
            updateHitScrollGroup(hitObject, rulesetData);
    }

    private void addHitObject(HitObject hitObject)
    {
        if (hitObject.Lane != Lane) return; // object was added to another lane, ignore

        // just in case
        if (hasHitObject(hitObject)) removeHitObject(hitObject);

        var time = (int)hitObject.Time;
        var endTime = (int)hitObject.EndTime;
        SnapIndices.AddSnapAtTime(time);
        SnapIndices.AddSnapAtTime(endTime);

        updateHitScrollGroup(hitObject, rulesetData);
        visibleHitObjectSortedAdd(hitObject);
    }

    private void removeHitObject(HitObject hitObject)
    {
        if (FutureHitObjects.Remove(hitObject)) return;
        if (PastHitObjects.Remove(hitObject)) return;

        var drawableHitObject = VisibleHitObjects.Find(d => d.Data == hitObject);

        if (drawableHitObject != null)
        {
            VisibleHitObjects.Remove(drawableHitObject);
            RemoveInternal(drawableHitObject, true);
        }
    }

    private void moveHitObject(HitObject hitObject)
    {
        // object's lane changed, we need to move it to another column
        if (hitObject.Lane != Lane)
        {
            if (!hasHitObject(hitObject)) return;

            // if we get here it means the current column should send the object to the other column
            removeHitObject(hitObject);
            HitObjectColumn newColumn = HitManager.GetColumn(hitObject.Lane);
            if (newColumn == null) throw new Exception($"Column {hitObject.Lane} not found");

            newColumn.addHitObject(hitObject);
            return;
        }

        var time = (int)hitObject.Time;
        var endTime = (int)hitObject.EndTime;
        SnapIndices.AddSnapAtTime(time);
        SnapIndices.AddSnapAtTime(endTime);

        updateHitScrollGroup(hitObject, rulesetData);
        var drawable = VisibleHitObjects.Find(d => d.Data == hitObject);

        if (drawable != null)
        {
            drawable.UpdateScrollVelocityTime();
            drawable.UpdateSnapColor();
            VisibleHitObjects.Sort((a, b) => a.Data.Time.CompareTo(b.Data.Time));
        }
    }

    private void onTimingPointChange(TimingPoint timingPoint)
    {
        SnapIndices.InitSnapIndices();
        foreach (var hitObject in VisibleHitObjects) hitObject.UpdateSnapColor();
    }

    private bool hasHitObject(HitObject hitObject)
    {
        if (FutureHitObjects.Contains(hitObject)) return true;
        if (PastHitObjects.Contains(hitObject)) return true;
        if (VisibleHitObjects.Any(d => d.Data == hitObject)) return true;

        return false;
    }

    private static void updateHitScrollGroup(HitObject hit, RulesetData rulesetData)
    {
        if (!string.IsNullOrWhiteSpace(hit.Group) && rulesetData.ScrollGroups.TryGetValue(hit.Group, out var gr))
            hit.ScrollGroup = gr;
        else
            hit.ScrollGroup = null;
    }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
}
