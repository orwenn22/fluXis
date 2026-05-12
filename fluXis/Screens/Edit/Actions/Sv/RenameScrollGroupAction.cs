using System.Collections.Generic;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;

namespace fluXis.Screens.Edit.Actions.Sv;

public class RenameScrollGroupAction : EditorAction
{
    public override string Description => $"Rename scroll group {oldName} to {newName}";

    private readonly string oldName;
    private readonly string newName;
    private List<HitObject> modifiedNotes;
    private List<IHasGroups> pointsContainedOldNameOnly;
    private List<IHasGroups> pointsContainedOldAndNewName;

    public RenameScrollGroupAction(string oldName, string newName)
    {
        this.oldName = oldName;
        this.newName = newName;
    }

    public override void Run(EditorMap map)
    {
        modifiedNotes = new List<HitObject>();
        pointsContainedOldNameOnly = new List<IHasGroups>();
        pointsContainedOldAndNewName = new List<IHasGroups>();

        foreach (var hitObject in map.MapInfo.HitObjects)
        {
            if (hitObject.Group == oldName)
            {
                hitObject.Group = newName;
                map.Update(hitObject);
                modifiedNotes.Add(hitObject);
            }
        }

        foreach (var sv in map.MapInfo.ScrollVelocities)
        {
            if (sv.Groups.Contains(oldName))
            {
                sv.Groups.Remove(oldName);

                if (!sv.Groups.Contains(newName))
                {
                    sv.Groups.Add(newName);
                    pointsContainedOldNameOnly.Add(sv);
                }
                else
                {
                    pointsContainedOldAndNewName.Add(sv);
                }

                map.Update(sv);
            }
        }

        foreach (var av in map.MapInfo.AdditiveVelocities)
        {
            if (av.Groups.Contains(oldName))
            {
                av.Groups.Remove(oldName);

                if (!av.Groups.Contains(newName))
                {
                    av.Groups.Add(newName);
                    pointsContainedOldNameOnly.Add(av);
                }
                else
                {
                    pointsContainedOldAndNewName.Add(av);
                }

                map.Update(av);
            }
        }
    }

    public override void Undo(EditorMap map)
    {
        foreach (var hitObject in modifiedNotes)
        {
            hitObject.Group = oldName;
            map.Update(hitObject);
        }

        foreach (var point in pointsContainedOldNameOnly)
        {
            point.Groups.Remove(newName);
            point.Groups.Add(oldName);
            map.Update(point as ITimedObject);
        }

        foreach (var point in pointsContainedOldAndNewName)
        {
            point.Groups.Add(oldName);
            map.Update(point as ITimedObject);
        }
    }
}
