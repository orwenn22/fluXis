using System.Collections.Generic;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Screens.Edit.Tool;

namespace fluXis.Screens.Edit.Actions.ToolLogs;

public class EditorReApplyToolLogAction : EditorAction
{
    public override string Description => "Re-apply Tool Log";

    private readonly EditorTool tool;
    private readonly ToolLog oldToolLog;

    private List<IHasTag> oldPoints;
    private EditorAction reApplyAction;

    public EditorReApplyToolLogAction(EditorTool tool, ToolLog toolLog)
    {
        this.tool = tool;
        oldToolLog = toolLog;
    }

    public override void Run(EditorMap map)
    {
        oldPoints = new List<IHasTag>();

        foreach (var sv in map.MapInfo.ScrollVelocities)
        {
            if (sv.Tag == oldToolLog.Tag) oldPoints.Add(sv);
        }

        foreach (var av in map.MapInfo.AdditiveVelocities)
        {
            if (av.Tag == oldToolLog.Tag) oldPoints.Add(av);
        }

        foreach (var point in oldPoints)
            map.Remove(point as ITimedObject);

        map.Remove(oldToolLog);

        reApplyAction = tool.GetReApplyAction(oldToolLog);
        reApplyAction.Run(map);
    }

    public override void Undo(EditorMap map)
    {
        reApplyAction.Undo(map);

        map.Add(oldToolLog);

        foreach (var point in oldPoints)
            map.Add(point as ITimedObject);
    }
}
