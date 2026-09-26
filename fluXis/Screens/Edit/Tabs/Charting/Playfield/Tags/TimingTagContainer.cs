using System.Collections.Generic;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Map.Structures.Events.Scrolling;
using fluXis.Screens.Edit.Blueprints.Selection;
using fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.TimingTags;
using osu.Framework.Allocation;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags;

public partial class TimingTagContainer : EditorTagContainer
{
    [Resolved]
    private ChartingContainer chartingContainer { get; set; }

    private SelectionHandler<HitObject> selectionHandler => chartingContainer.BlueprintContainer.SelectionHandler;

    protected override void LoadComplete()
    {
        AddTag(new PreviewPointTag(this));

        foreach (var timingPoint in Map.MapInfo.TimingPoints)
            addTimingPoint(timingPoint);

        foreach (var sm in Map.MapInfo.MapEvents.ScrollMultiplyEvents)
            addScrollMultiplier(sm);

        Map.RegisterAddListener<TimingPoint>(addTimingPoint);
        Map.RegisterRemoveListener<TimingPoint>(RemoveTag);
        Map.RegisterAddRangeListener<TimingPoint>(addTimingPointRange);
        Map.RegisterClearListener<TimingPoint>(ClearTags<TimingPoint>);
        Map.RegisterUpdateListener<TimingPoint>(UpdateTag);

        Map.RegisterAddListener<ScrollMultiplierEvent>(addScrollMultiplier);
        Map.RegisterRemoveListener<ScrollMultiplierEvent>(RemoveTag);
        Map.RegisterAddRangeListener<ScrollMultiplierEvent>(addScrollMultiplierRange);
        Map.RegisterClearListener<ScrollMultiplierEvent>(ClearTags<ScrollMultiplierEvent>);
        Map.RegisterUpdateListener<TimingPoint>(UpdateTag);

        selectionHandler.SelectedObjects.CollectionChanged += (_, _) => updateSelection();
    }

    private void addTimingPoint(TimingPoint tp) => AddTag(new TimingPointTag(this, tp));
    private void addScrollMultiplier(ScrollMultiplierEvent sm) => AddTag(new ScrollMultiplierTag(this, sm));

    private void addTimingPointRange(IEnumerable<TimingPoint> timpingPoints)
    {
        foreach (var tp in timpingPoints) addTimingPoint(tp);
    }

    private void addScrollMultiplierRange(IEnumerable<ScrollMultiplierEvent> scrollMutipliers)
    {
        foreach (var sm in scrollMutipliers) addScrollMultiplier(sm);
    }

    //TODO: handle empty/null arrays properly?
    private void updateSelection()
    {
        if (selectionHandler.SelectedObjects.Count == 1)
        {
            HitObject hit = selectionHandler.SelectedObjects[0];
            string scrollGroup = hit.Group;
            if (string.IsNullOrEmpty(scrollGroup)) scrollGroup = $"${hit.Lane}";
            SetHighlightFilter(tag =>
            {
                if (tag.TimedObject is not IHasGroups groups)
                {
                    return true;
                }

                return groups.Groups.Contains(scrollGroup);
            });
        }
        else
        {
            SetHighlightFilter(null);
        }
    }
}
