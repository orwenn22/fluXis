using fluXis.Map.Structures;
using fluXis.Screens.Edit.Actions;

namespace fluXis.Screens.Edit.Tool;

// the goal of this thing is to make it possible to open corresponding tool windows/panel/whatever with correct settings and
// be able to reapply the usage of tool with those exact settings from a ToolLog
// the name "Tool" is a bit vague for this, this is mostly intended for quaver-like plugins that generates lots of SVs
public abstract class EditorTool
{
    // this maps to ToolLog's ToolName field, when clicking on a ToolLogTag it will look for a tool with a name that matches the
    // one in the ToolLog
    public abstract string Name { get; }

    // if set to true, the context menu will display a "Clone to current time" entry that will call GetCloneAction()
    // TODO: actually implement that context menu thing lol, but that would probably involve displaying the ToolLogs in the sidebar properly
    public virtual bool Clonable => false;

    // opens the tool with the settings/params stored in the ToolLog
    public abstract void OpenTool(Editor editor, ToolLog toolLog);

    // gets the action to re-apply the usage of the tool from the ToolLog, most of the time this should be the same type as the
    // action that caused the ToolLog entry to be added to the map
    public abstract EditorAction GetReApplyAction(ToolLog toolLog);

    // clone the effect at specified time
    public virtual EditorAction GetCloneAction(ToolLog toolLog, double cloneTime) => throw new System.NotImplementedException();
}
