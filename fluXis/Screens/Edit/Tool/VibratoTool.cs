using fluXis.Map.Structures;
using fluXis.Screens.Edit.Actions;
using fluXis.Screens.Edit.Actions.Sv;
using fluXis.Screens.Edit.Windows;
using Newtonsoft.Json;

namespace fluXis.Screens.Edit.Tool;

public class VibratoTool : EditorTool
{
    public override string Name => "orwenn22.vibrato";
    public override bool Clonable => true;

    public override void OpenTool(Editor editor, ToolLog toolLog)
    {
        var vibratoParams = JsonConvert.DeserializeObject<EditorVibratoAction.VibratoParams>(toolLog.ToolSettings);
        editor.OpenWindow(new VibratoWindow(vibratoParams) { X = 100, Y = 100 });
    }

    public override EditorAction GetReApplyAction(ToolLog toolLog)
    {
        var vibratoParams = JsonConvert.DeserializeObject<EditorVibratoAction.VibratoParams>(toolLog.ToolSettings);
        return new EditorVibratoAction(vibratoParams);
    }

    public override EditorAction GetCloneAction(ToolLog toolLog, double cloneTime)
    {
        var vibratoParams = JsonConvert.DeserializeObject<EditorVibratoAction.VibratoParams>(toolLog.ToolSettings);
        double duration = vibratoParams.EndTime - vibratoParams.StartTime;
        vibratoParams.StartTime = cloneTime;
        vibratoParams.EndTime = cloneTime + duration;
        return new EditorVibratoAction(vibratoParams);
    }
}
