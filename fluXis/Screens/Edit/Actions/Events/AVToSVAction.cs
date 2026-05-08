using System.Collections.Generic;
using System.Linq;
using fluXis.Map.Structures;
using fluXis.Screens.Gameplay.Ruleset;

namespace fluXis.Screens.Edit.Actions.Events;

// this is being held together with hopes and dreams
public class AVToSVAction : EditorAction
{
    public override string Description => "convert Additive Velocities to Scroll Velocities";

    private List<ScrollVelocity> removedSVs;
    private List<AdditiveVelocity> removedAVs;
    private List<ScrollVelocity> addedSVs;

    public override void Run(EditorMap map)
    {
        removedSVs = map.MapInfo.ScrollVelocities.ToList();
        removedAVs = map.MapInfo.AdditiveVelocities.ToList();

        // get all group names from the objects
        var groupNames = removedSVs.SelectMany(sv => sv.Groups)
                                   .Concat(removedAVs.SelectMany(av => av.Groups))
                                   .Distinct()
                                   .ToList();

        // null/empty group
        if (!groupNames.Contains(null)) groupNames.Add(null);

        addedSVs = new List<ScrollVelocity>();

        foreach (var groupName in groupNames)
        {
            var group = new ScrollGroup();

            foreach (var sv in removedSVs.Where(sv => sv.Groups.Contains(groupName) || (groupName == null && sv.Groups.Count == 0)))
                group.AddVelocity(sv);

            foreach (var av in removedAVs.Where(av => av.Groups.Contains(groupName) || (groupName == null && av.Groups.Count == 0)))
                group.AddAdditiveVelocity(av);

            group.InitMarkers();

            addedSVs.AddRange(group.FlattenToScrollVelocities(groupName));
        }

        // if something goes wrong remove this and see if it works lol
        // this makes it so if multiple SVs have the same time and have the exact same group, we only keep the last
        // (it can happens if multiple additive velocities have the same time)
        // this "works" (hopefully) because somehow the final velocity is always the last to appear in the list
        addedSVs = addedSVs
                   .GroupBy(sv => (sv.Time, string.Join(",", sv.Groups.OrderBy(g => g))))
                   .Select(g => g.Last())
                   .ToList();

        // also remove this if something goes wrong (though i think it's fine)
        // this makes it so SVs with the same time and multipliers but different groups get merged together
        addedSVs = addedSVs
                   .GroupBy(sv => (sv.Time, sv.Multiplier))
                   .Select(g => new ScrollVelocity
                   {
                       Time = g.Key.Time,
                       Multiplier = g.Key.Multiplier,
                       Groups = g.SelectMany(sv => sv.Groups).Distinct().OrderBy(x => x).ToList()
                   })
                   .ToList();

        // foreach (var sv in removedSVs) map.Remove(sv);
        map.Clear<ScrollVelocity>();

        // foreach (var av in removedAVs) map.Remove(av);
        map.Clear<AdditiveVelocity>();

        // foreach (var sv in addedSVs) map.Add(sv);
        map.AddRange(addedSVs);
    }

    public override void Undo(EditorMap map)
    {
        // foreach (var sv in addedSVs) map.Remove(sv);
        map.Clear<ScrollVelocity>();

        // foreach (var sv in removedSVs) map.Add(sv);
        map.AddRange(removedSVs);

        // foreach (var av in removedAVs) map.Add(av);
        map.AddRange(removedAVs);
    }
}
