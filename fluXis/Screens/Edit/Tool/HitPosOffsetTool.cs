using fluXis.Map.Structures;
using fluXis.Screens.Edit.Actions;
using fluXis.Screens.Edit.Actions.Sv;
using fluXis.Screens.Edit.Windows;
using Newtonsoft.Json;

namespace fluXis.Screens.Edit.Tool;

public class HitPosOffsetTool : EditorTool
{
    public override string Name => "orwenn22.hitposoffset";

    public override void OpenTool(Editor editor, ToolLog toolLog)
    {
        var hitPosOffsetParams = JsonConvert.DeserializeObject<EditorHitPosOffestAction.HitPosOffsetParams>(toolLog.ToolSettings);
        editor.OpenWindow(new HitPosOffsetWindow(hitPosOffsetParams) { X = 100, Y = 100 });
    }

    public override EditorAction GetReApplyAction(ToolLog toolLog)
    {
        var hitPosOffsetParams = JsonConvert.DeserializeObject<EditorHitPosOffestAction.HitPosOffsetParams>(toolLog.ToolSettings);
        return new EditorHitPosOffestAction(hitPosOffsetParams);
    }
}
