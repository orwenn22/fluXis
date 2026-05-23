using fluXis.Map.Structures;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.EffectTags;

public partial class ToolLogTag : EditorTag
{
    public override Colour4 TagColour => toolLog.Color;

    private ToolLog toolLog => (ToolLog)TimedObject;

    public ToolLogTag(EditorTagContainer parent, ToolLog toolLog)
        : base(parent, toolLog)
    {
    }

    protected override void Update()
    {
        base.Update();
        Text.Text = $"{toolLog.Label}";
    }

    protected override bool OnClick(ClickEvent e)
    {
        Editor.ChangeToTab<ChartingTab>(x => x.Container.Sidebar.ShowPoint(toolLog));
        return true;
    }
}
