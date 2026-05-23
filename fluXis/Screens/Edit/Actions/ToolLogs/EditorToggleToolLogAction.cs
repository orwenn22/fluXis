using fluXis.Map.Structures;

namespace fluXis.Screens.Edit.Actions.ToolLogs;

public class EditorToggleToolLogAction : EditorAction
{
    public override string Description => "Toggle Tool Log";

    private readonly ToolLog toolLog;

    public EditorToggleToolLogAction(ToolLog toolLog)
    {
        this.toolLog = toolLog;
    }

    public override void Run(EditorMap map)
    {
        toggleToolLog(map);
    }

    public override void Undo(EditorMap map)
    {
        toggleToolLog(map);
    }

    private void toggleToolLog(EditorMap map)
    {
        if (toolLog.Effective)
        {
            foreach (var sv in map.MapInfo.ScrollVelocities)
            {
                if (sv.Tag == toolLog.Tag) toolLog.ScrollVelocities.Add(sv);
            }

            foreach (var av in map.MapInfo.AdditiveVelocities)
            {
                if (av.Tag == toolLog.Tag) toolLog.AdditiveVelocities.Add(av);
            }

            //TODO: more objects?

            foreach (var sv in toolLog.ScrollVelocities) map.Remove(sv);
            foreach (var av in toolLog.AdditiveVelocities) map.Remove(av);
        }
        else
        {
            foreach (var sv in toolLog.ScrollVelocities) map.Add(sv);
            foreach (var av in toolLog.AdditiveVelocities) map.Add(av);
            toolLog.ScrollVelocities.Clear();
            toolLog.AdditiveVelocities.Clear();
        }

        toolLog.Effective = !toolLog.Effective;
    }
}
