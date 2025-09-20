using System.Collections.Generic;
using fluXis.Map.Structures;

namespace fluXis.Screens.Edit.Actions;

public class InsertMiddleLaneAction : EditorAction
{
    public override string Description => "Add middle lane to even key count";

    private IEnumerable<HitObject> notes { get; }
    private EditorMap map { get; }
    private int originalKeyCount { get; }

    public InsertMiddleLaneAction(IEnumerable<HitObject> notes, EditorMap map)
    {
        this.notes = notes;
        this.map = map;
        this.originalKeyCount = map.RealmMap.KeyCount;
    }

    public override void Run(EditorMap map)
    {
        if (originalKeyCount % 2 == 1) return; //only run this with even number of lanes
        if (!map.SetKeyMode(originalKeyCount + 1)) return;

        foreach (var hitObject in notes)
        {
            if (hitObject.Lane <= originalKeyCount / 2) continue; //should never happen, but check just in case

            hitObject.Lane++; //shift all notes that are on the right side of the playfield
        }

        foreach (var scrollVelocity in map.MapInfo.ScrollVelocities)
        {
            if (groupAllLanes(scrollVelocity.Groups))
            {
                //if the sv scrolled all lanes, just apply it to the newly created lane too
                scrollVelocity.Groups.Add("$" + (originalKeyCount + 1));
            }
            else
            {
                //otherwise, shift the lanes of the right side
                scrollVelocity.Groups = rShiftGroups(scrollVelocity.Groups);
            }
        }

        foreach (var scrollMultiplyEvent in map.MapEvents.ScrollMultiplyEvents)
        {
            if (groupAllLanes(scrollMultiplyEvent.Groups))
            {
                //if the sm scrolled all lanes, just apply it to the newly created lane too
                scrollMultiplyEvent.Groups.Add("$" + (originalKeyCount + 1));
            }
            else
            {
                //otherwise, shift the lanes of the right side
                scrollMultiplyEvent.Groups = rShiftGroups(scrollMultiplyEvent.Groups);
            }
        }
    }

    public override void Undo(EditorMap map)
    {
        if (map.RealmMap.KeyCount != originalKeyCount + 1) return; //should never happen

        foreach (var scrollMultiplyEvent in map.MapEvents.ScrollMultiplyEvents)
        {
            scrollMultiplyEvent.Groups = lShiftSv(scrollMultiplyEvent.Groups);
        }

        foreach (var scrollVelocity in map.MapInfo.ScrollVelocities)
        {
            scrollVelocity.Groups = lShiftSv(scrollVelocity.Groups);
        }

        foreach (var hitObject in notes)
        {
            if (hitObject.Lane <= originalKeyCount / 2) continue; //should never happen, but check just in case

            hitObject.Lane--;
        }

        if (!map.SetKeyMode(originalKeyCount)) return; //TODO: error message?
    }

    //this is used pre-conversion to detect if the initial scroll velocity or scroll multiply affected all lanes
    private bool groupAllLanes(List<string> groups)
    {
        bool[] scrollLanes = new bool[originalKeyCount];
        for (int i = 0; i < originalKeyCount; i++) scrollLanes[i] = false;

        foreach (var group in groups)
        {
            int groupIndex = int.Parse(group.Substring(1)) - 1; //groups start at 1, but our array starts at index 0
            if (groupIndex < originalKeyCount) scrollLanes[groupIndex] = true;
        }

        foreach (var lane in scrollLanes)
        {
            if (!lane) return false;
        }

        return true;
    }

    private List<string> rShiftGroups(List<string> groups)
    {
        List<string> newGroups = new();

        foreach (var group in groups)
        {
            int groupIndex = int.Parse(group.Substring(1));
            if (groupIndex > originalKeyCount / 2) groupIndex++;
            newGroups.Add("$" + groupIndex);
        }

        return newGroups;
    }

    private List<string> lShiftSv(List<string> groups)
    {
        List<string> newGroups = new();

        foreach (var group in groups)
        {
            int groupIndex = int.Parse(group.Substring(1));

            //since this method is used for undoing, at this point there are originalKeyCount+1 lanes.
            //however, we want to completely ignore the middle lane (we dont want to copy it or shift it)
            if (groupIndex == originalKeyCount / 2 + 1) continue;

            if (groupIndex > originalKeyCount / 2) groupIndex--;
            string groupString = "$" + groupIndex;
            if (!newGroups.Contains(groupString)) newGroups.Add(groupString);
        }

        return newGroups;
    }
}
