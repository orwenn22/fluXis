using System.Collections.Generic;
using fluXis.Map.Structures.Bases;

namespace fluXis.Screens.Edit.Actions.Sv;

public class EditorRemoveScrollGroupAction : EditorAction
{
    public override string Description => $"Remove group '{groupName}' to {items.Count} events";

    private readonly string groupName;
    private readonly List<ITimedObject> items;
    private List<IHasGroups> modifiedItems;

    public EditorRemoveScrollGroupAction(List<ITimedObject> items, string groupName)
    {
        this.items = new List<ITimedObject>(items);
        this.groupName = groupName;
    }

    public override void Run(EditorMap map)
    {
        modifiedItems = new List<IHasGroups>();

        foreach (ITimedObject item in items)
        {
            if (item is not IHasGroups groups) continue;
            if (!groups.Groups.Contains(groupName)) continue;

            modifiedItems.Add(groups);
            groups.Groups.Remove(groupName);
            map.Update(item);
        }
    }

    public override void Undo(EditorMap map)
    {
        foreach (IHasGroups groups in modifiedItems)
        {
            groups.Groups.Add(groupName);
            map.Update(groups as ITimedObject);
        }
    }
}
