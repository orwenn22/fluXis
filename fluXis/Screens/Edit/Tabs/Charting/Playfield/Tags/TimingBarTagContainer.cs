using System.Collections.Generic;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Screens.Edit.Blueprints.Selection;
using fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.BarTags;
using osu.Framework.Allocation;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags;

public partial class TimingBarTagContainer : BarTagContainer
{
    [Resolved]
    private ChartingContainer chartingContainer { get; set; }

    private SelectionHandler<HitObject> selectionHandler => chartingContainer.BlueprintContainer.SelectionHandler;

    protected override void LoadComplete()
    {
        foreach (var sv in Map.MapInfo.ScrollVelocities)
            addScrollVelocity(sv);

        foreach (var av in Map.MapInfo.AdditiveVelocities)
            addAdditiveVelocity(av);

        Map.RegisterAddListener<ScrollVelocity>(addScrollVelocity);
        Map.RegisterRemoveListener<ScrollVelocity>(RemoveTag);
        Map.RegisterAddRangeListener<ScrollVelocity>(addScrollVelocityRange);
        Map.RegisterClearListener<ScrollVelocity>(ClearTags<ScrollVelocity>);
        Map.RegisterUpdateListener<TimingPoint>(UpdateTag);

        Map.RegisterAddListener<AdditiveVelocity>(addAdditiveVelocity);
        Map.RegisterRemoveListener<AdditiveVelocity>(RemoveTag);
        Map.RegisterAddRangeListener<AdditiveVelocity>(addAdditiveVelocityRange);
        Map.RegisterClearListener<AdditiveVelocity>(ClearTags<AdditiveVelocity>);
        Map.RegisterUpdateListener<TimingPoint>(UpdateTag);

        selectionHandler.SelectedObjects.CollectionChanged += (_, _) => updateSelection();
    }

    private void addScrollVelocity(ScrollVelocity sv) => AddTag(new ScrollVelocityBarTag(this, sv));
    private void addAdditiveVelocity(AdditiveVelocity av) => AddTag(new AdditiveVelocityBarTag(this, av));

    private void addScrollVelocityRange(IEnumerable<ScrollVelocity> scrollVelocities)
    {
        foreach (var sv in scrollVelocities) addScrollVelocity(sv);
    }

    private void addAdditiveVelocityRange(IEnumerable<AdditiveVelocity> additiveVelocities)
    {
        foreach (var av in additiveVelocities) addAdditiveVelocity(av);
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
