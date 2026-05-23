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

    // opens the tool with the settings/params stored in the ToolLog
    public abstract void OpenTool(Editor editor, ToolLog toolLog);

    // gets the action to re-apply the usage of the tool from the ToolLog, most of the time this should be the same type as the
    // action that caused the ToolLog entry to be added to the map
    public abstract EditorAction GetReApplyAction(ToolLog toolLog);
}
