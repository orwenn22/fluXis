using System.Collections.Generic;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Events.Scrolling;
using fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.TimingTags;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags;

public partial class TimingTagContainer : EditorTagContainer
{
    protected override void LoadComplete()
    {
        AddTag(new PreviewPointTag(this));

        foreach (var timingPoint in Map.MapInfo.TimingPoints)
            addTimingPoint(timingPoint);

        foreach (var sv in Map.MapInfo.ScrollVelocities)
            addScrollVelocity(sv);

        foreach (var av in Map.MapInfo.AdditiveVelocities)
            addAdditiveVelocity(av);

        foreach (var sm in Map.MapInfo.MapEvents.ScrollMultiplyEvents)
            addScrollMultiplier(sm);

        Map.RegisterAddListener<TimingPoint>(addTimingPoint);
        Map.RegisterRemoveListener<TimingPoint>(RemoveTag);
        Map.RegisterAddRangeListener<TimingPoint>(addTimingPointRange);
        Map.RegisterClearListener<TimingPoint>(ClearTags<TimingPoint>);

        Map.RegisterAddListener<ScrollVelocity>(addScrollVelocity);
        Map.RegisterRemoveListener<ScrollVelocity>(RemoveTag);
        Map.RegisterAddRangeListener<ScrollVelocity>(addScrollVelocityRange);
        Map.RegisterClearListener<ScrollVelocity>(ClearTags<ScrollVelocity>);

        Map.RegisterAddListener<AdditiveVelocity>(addAdditiveVelocity);
        Map.RegisterRemoveListener<AdditiveVelocity>(RemoveTag);
        Map.RegisterAddRangeListener<AdditiveVelocity>(addAdditiveVelocityRange);
        Map.RegisterClearListener<AdditiveVelocity>(ClearTags<AdditiveVelocity>);

        Map.RegisterAddListener<ScrollMultiplierEvent>(addScrollMultiplier);
        Map.RegisterRemoveListener<ScrollMultiplierEvent>(RemoveTag);
        Map.RegisterAddRangeListener<ScrollMultiplierEvent>(addScrollMultiplierRange);
        Map.RegisterClearListener<ScrollMultiplierEvent>(ClearTags<ScrollMultiplierEvent>);
    }

    private void addTimingPoint(TimingPoint tp) => AddTag(new TimingPointTag(this, tp));
    private void addScrollVelocity(ScrollVelocity sv) => AddTag(new ScrollVelocityTag(this, sv));
    private void addAdditiveVelocity(AdditiveVelocity av) => AddTag(new AdditiveVelocityTag(this, av));
    private void addScrollMultiplier(ScrollMultiplierEvent sm) => AddTag(new ScrollMultiplierTag(this, sm));

    private void addTimingPointRange(IEnumerable<TimingPoint> timpingPoints)
    {
        foreach (var tp in timpingPoints) addTimingPoint(tp);
    }

    private void addScrollVelocityRange(IEnumerable<ScrollVelocity> scrollVelocities)
    {
        foreach (var sv in scrollVelocities) addScrollVelocity(sv);
    }

    private void addAdditiveVelocityRange(IEnumerable<AdditiveVelocity> additiveVelocities)
    {
        foreach (var av in additiveVelocities) addAdditiveVelocity(av);
    }

    private void addScrollMultiplierRange(IEnumerable<ScrollMultiplierEvent> scrollMutipliers)
    {
        foreach (var sm in scrollMutipliers) addScrollMultiplier(sm);
    }
}
