using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Audio.Transforms;
using fluXis.Map;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Map.Structures.Events.Scrolling;
using fluXis.Scoring;
using fluXis.Screens.Edit;
using fluXis.Screens.Gameplay.Input;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Screens.Gameplay.Ruleset;

public partial class RulesetData : CompositeDrawable
{
    public event Action<ScrollVelocity> OnSvAdd;
    public event Action<ScrollVelocity> OnSvRemove;
    public event Action<ScrollVelocity> OnSvUpdate;

    public event Action<AdditiveVelocity> OnAvAdd;
    public event Action<AdditiveVelocity> OnAvRemove;
    public event Action<AdditiveVelocity> OnAvUpdate;

    public event Action<HitObject> OnHitObjectAddAction;
    public event Action<HitObject> OnHitObjectRemoveAction;
    public event Action<HitObject> OnHitObjectUpdateAction;

    public event Action<TimingPoint> OnTimingPointChangeAction;

    public MapInfo MapInfo { get; init; }
    public MapEvents MapEvents { get; init; }

    public TransformableClock ParentClock { get; set; }
    public bool AllowReverting { get; set; }

    public Bindable<float> ScrollSpeed { get; set; } = new(3);
    public float Rate { get; set; } = 1;
    public bool CatchingUp { get; set; }
    public BindableBool IsPaused { get; } = new();
    public GameplayInput Input { get; set; }
    public bool AlwaysShowKeys { get; set; }

    public HitWindows HitWindows { get; set; }
    public ReleaseWindows ReleaseWindows { get; set; }
    public LandmineWindows LandmineWindows { get; set; }

    private readonly Dictionary<string, ScrollGroup> scrolls = new();
    public IReadOnlyDictionary<string, ScrollGroup> ScrollGroups => scrolls;

    public Drawable ShakeTarget { get; set; }

    private readonly Container scrollGroupsContainer;

    public RulesetData()
    {
        scrollGroupsContainer = new Container();
        InternalChild = scrollGroupsContainer;
        ClearTransforms();
    }

    // the editor's OnEntering method calls FinishTransforms(true), but we want to keep the transforms on the scroll groups
    public override void FinishTransforms(bool propagateChildren = false, string targetMember = null)
    {
        base.FinishTransforms(false, targetMember);
    }

    public void RegisterEditorMapListeners(EditorMap editorMap)
    {
        editorMap.RegisterAddListener<ScrollVelocity>(onHasGroupEventAdd);
        editorMap.RegisterRemoveListener<ScrollVelocity>(onHasGroupEventRemove);
        editorMap.RegisterUpdateListener<ScrollVelocity>(onHasGroupEventUpdate);
        editorMap.RegisterAddListener<ScrollMultiplierEvent>(onHasGroupEventAdd);
        editorMap.RegisterRemoveListener<ScrollMultiplierEvent>(onHasGroupEventRemove);
        editorMap.RegisterUpdateListener<ScrollMultiplierEvent>(onHasGroupEventUpdate);
        editorMap.RegisterAddListener<AdditiveVelocity>(onHasGroupEventAdd);
        editorMap.RegisterRemoveListener<AdditiveVelocity>(onHasGroupEventRemove);
        editorMap.RegisterUpdateListener<AdditiveVelocity>(onHasGroupEventUpdate);
        editorMap.RegisterAddListener<HitObject>(onHitObjectAdd);
        editorMap.RegisterRemoveListener<HitObject>(onHitObjectRemove);
        editorMap.RegisterUpdateListener<HitObject>(onHitObjectUpdate);
        editorMap.RegisterAddListener<TimingPoint>(onTimingPointChage);
        editorMap.RegisterRemoveListener<TimingPoint>(onTimingPointChage);
        editorMap.RegisterUpdateListener<TimingPoint>(onTimingPointChage);
    }

    // the RulesetData should be loaded before calling this
    public void CreateScrollGroups(bool forceRebuild = false)
    {
        if (scrolls.Count > 0 && !forceRebuild)
            return;

        scrolls.Clear();
        scrollGroupsContainer.Clear();

        // creating groups
        for (int i = 0; i < MapInfo.RealmEntry!.KeyCount; i++)
            scrolls[$"${i + 1}"] = new ScrollGroup { Name = $"${i + 1}" };

        var events = MapInfo.ScrollVelocities.Cast<IHasGroups>().Concat(MapEvents.ScrollMultiplyEvents).Concat(MapInfo.AdditiveVelocities).ToList();
        var groups = events.SelectMany(x => x.Groups).Distinct().Order().ToList();

        foreach (var group in groups)
        {
            if (group.StartsWith('$'))
                continue;

            if (!scrolls.ContainsKey(group))
                scrolls[group] = new ScrollGroup { Name = group };
        }

        scrolls.ForEach(x => LoadComponent(x.Value));

        //TODO: fix scroll multipliers not working
        // populating groups
        foreach (var ev in events)
        {
            if (ev.Groups.Count == 0)
            {
                foreach (var (_, group) in scrolls.Where(x => x.Key.StartsWith('$')))
                    ev.Apply(group);
            }
            else
            {
                foreach (var group in ev.Groups)
                {
                    if (scrolls.TryGetValue(group, out var scroll))
                        ev.Apply(scroll);
                }
            }
        }

        scrolls.ForEach(x => x.Value.InitMarkers());
        scrolls.ForEach(x =>
        {
            scrollGroupsContainer.Add(x.Value);
            x.Value.Clock = ParentClock; //assuming ParentClock is valid?
        });
    }

    // should be called right after an SV or SM is added to the map to update scroll groups
    private void onHasGroupEventAdd(IHasGroups ev)
    {
        //CreateScrollGroups(true);

        foreach (var scrollGroup in ev.Groups)
        {
            if (!scrolls.TryGetValue(scrollGroup, out var scroll))
            {
                var newGroup = new ScrollGroup { Name = $"{scrollGroup}" };
                scrolls[scrollGroup] = newGroup;
                scrollGroupsContainer.Add(newGroup);
                newGroup.Clock = ParentClock;
            }
        }

        if (ev.Groups.Count == 0)
        {
            foreach (var (_, group) in scrolls.Where(x => x.Key.StartsWith('$')))
            {
                ev.Apply(group);
                group.InitMarkers();
            }
        }
        else
        {
            foreach (var group in ev.Groups)
            {
                if (scrolls.TryGetValue(group, out var scroll))
                {
                    ev.Apply(scroll);
                    scroll.InitMarkers();
                }
            }
        }

        if (ev is ScrollVelocity sv) OnSvAdd?.Invoke(sv);
        else if (ev is AdditiveVelocity av) OnAvAdd?.Invoke(av);
    }

    // should be called after an SV or SM is removed from the map to update scroll groups
    private void onHasGroupEventRemove(IHasGroups ev)
    {
        // CreateScrollGroups(true);

        foreach (var g in ev.Groups)
        {
            if (scrolls.TryGetValue(g, out var scroll))
            {
                scroll.RemoveEvent(ev);
                scroll.InitMarkers();

                if (!scroll.HasEvents() && !scroll.Name.StartsWith('$'))
                {
                    scrolls.Remove(g);
                    scrollGroupsContainer.Remove(scroll, true);
                }
            }
        }

        if (ev is ScrollVelocity sv) OnSvRemove?.Invoke(sv);
        else if (ev is AdditiveVelocity av) OnAvRemove?.Invoke(av);
    }

    private void onHasGroupEventUpdate(IHasGroups ev)
    {
        // if (ev is ScrollMultiplierEvent) throw new Exception("ScrollMultiplier events are not supported");

        // the user might have removed some groups from the event, if so we need to remove the event from those groups
        foreach (var scroll in scrolls.Values)
        {
            if (!ev.Groups.Contains(scroll.Name) && scroll.HasEvent(ev))
            {
                scroll.RemoveEvent(ev);
                scroll.InitMarkers();

                if (!scroll.HasEvents() && !scroll.Name.StartsWith('$'))
                {
                    scrolls.Remove(scroll.Name);
                    scrollGroupsContainer.Remove(scroll, true);
                }
            }
        }

        // the event might have been added to other groups, if so we need to add the event to those groups
        foreach (var g in ev.Groups)
        {
            // check if group already exists, if not create it
            if (!scrolls.TryGetValue(g, out var scroll))
            {
                scroll = new ScrollGroup { Name = $"{g}" };
                scrolls[g] = scroll;
                scrollGroupsContainer.Add(scroll);
                scroll.Clock = ParentClock;
            }

            // if the group already exits, add the event to it
            if (!scroll.HasEvent(ev))
                scroll.AddEvent(ev);
            else
            {
                if (ev is ScrollMultiplierEvent sm) scroll.UpdateScrollMultiplier(sm);
            }

            if (!(ev is ScrollMultiplierEvent)) scroll.InitMarkers();
        }

        if (ev is ScrollVelocity sv) OnSvUpdate?.Invoke(sv);
        else if (ev is AdditiveVelocity av) OnAvUpdate?.Invoke(av);
    }

    private void onHitObjectAdd(HitObject hit)
    {
        OnHitObjectAddAction?.Invoke(hit);
    }

    private void onHitObjectRemove(HitObject hit)
    {
        OnHitObjectRemoveAction?.Invoke(hit);
    }

    private void onHitObjectUpdate(HitObject hit)
    {
        OnHitObjectUpdateAction?.Invoke(hit);
    }

    private void onTimingPointChage(TimingPoint timingPoint)
    {
        OnTimingPointChangeAction?.Invoke(timingPoint);
    }

    // protected virtual GameplayInput CreateInput() => new(IsPaused.GetBoundCopy(), MapInfo.RealmEntry!.KeyCount, MapInfo.IsDual);

    // protected override GameplayInput CreateInput() => new ReplayInput(RulesetData.IsPaused.GetBoundCopy(), RulesetData.MapInfo.RealmEntry!.KeyCount, RulesetData.MapInfo.IsDual);
}
