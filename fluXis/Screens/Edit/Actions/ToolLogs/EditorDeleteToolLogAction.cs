using System.Collections.Generic;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;

namespace fluXis.Screens.Edit.Actions.ToolLogs;

public class EditorDeleteToolLogAction : EditorAction
{
    public override string Description => "Delete Tool Log";

    private readonly bool deletePoints;

    private List<IHasTag> objects;
    private ToolLog toolLog;

    public EditorDeleteToolLogAction(ToolLog toolLog, bool deletePoints)
    {
        this.toolLog = toolLog;
        this.deletePoints = deletePoints;
    }

    public override void Run(EditorMap map)
    {
        objects = new List<IHasTag>();

        foreach (var sv in map.MapInfo.ScrollVelocities)
        {
            if (sv.Tag == toolLog.Tag)
                objects.Add(sv);
        }

        foreach (var av in map.MapInfo.AdditiveVelocities)
        {
            if (av.Tag == toolLog.Tag)
                objects.Add(av);
        }

        //TODO: other objects?

        foreach (var o in objects) o.Tag = "";

        if (deletePoints)
        {
            foreach (var o in objects)
                map.Remove(o as ITimedObject);
        }

        map.Remove(toolLog);
    }

    public override void Undo(EditorMap map)
    {
        map.Add(toolLog);

        if (deletePoints)
        {
            foreach (var o in objects)
            {
                map.Add(o as ITimedObject);
            }
        }

        foreach (var o in objects)
            o.Tag = toolLog.Tag;
    }
}
