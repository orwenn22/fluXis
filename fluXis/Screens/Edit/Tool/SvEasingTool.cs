using fluXis.Map.Structures;
using fluXis.Screens.Edit.Actions;
using fluXis.Screens.Edit.Actions.Sv;
using fluXis.Screens.Edit.Windows;
using Newtonsoft.Json;

namespace fluXis.Screens.Edit.Tool;

public class SvEasingTool : EditorTool
{
    public override string Name => "orwenn22.sveasing";
    public override bool Clonable => true;

    public override void OpenTool(Editor editor, ToolLog toolLog)
    {
        var svEasingParams = JsonConvert.DeserializeObject<EditorSvEasingAction.SvEasingParams>(toolLog.ToolSettings);
        editor.OpenWindow(new SvEasingWindow(svEasingParams) { X = 100, Y = 100 });
    }

    public override EditorAction GetReApplyAction(ToolLog toolLog)
    {
        var svEasingParams = JsonConvert.DeserializeObject<EditorSvEasingAction.SvEasingParams>(toolLog.ToolSettings);
        return new EditorSvEasingAction(svEasingParams);
    }

    public override EditorAction GetCloneAction(ToolLog toolLog, double cloneTime)
    {
        var svEasingParams = JsonConvert.DeserializeObject<EditorSvEasingAction.SvEasingParams>(toolLog.ToolSettings);
        double duration = svEasingParams.EndTime - svEasingParams.StartTime;
        svEasingParams.StartTime = cloneTime;
        svEasingParams.EndTime = cloneTime + duration;
        return new EditorSvEasingAction(svEasingParams);
    }
}
