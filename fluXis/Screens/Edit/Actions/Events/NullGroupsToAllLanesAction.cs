using System.Collections.Generic;
using System.Linq;
using fluXis.Map.Structures;

namespace fluXis.Screens.Edit.Actions.Events;

public class NullGroupsToAllLanesAction : EditorAction
{
    public override string Description => "Replace all empty Groups array with lane groups";

    private List<ScrollVelocity> modified = new();

    public override void Run(EditorMap map)
    {
        var allLanes = Enumerable.Range(1, map.MapInfo.KeyCount)
                                 .Select(i => $"${i}")
                                 .ToList();

        modified = map.MapInfo.ScrollVelocities
                      .Where(sv => sv.Groups == null || sv.Groups.Count == 0)
                      .ToList();

        foreach (var sv in modified)
        {
            sv.Groups = allLanes.ToList();
            map.Update(sv);
        }
    }

    public override void Undo(EditorMap map)
    {
        foreach (var sv in modified)
        {
            sv.Groups = new List<string>();
            map.Update(sv);
        }
    }
}
