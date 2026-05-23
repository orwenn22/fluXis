using fluXis.Graphics.Sprites.Icons;
using fluXis.Graphics.UserInterface.Menus;
using fluXis.Graphics.UserInterface.Menus.Items;
using fluXis.Map.Structures;
using fluXis.Screens.Edit.Actions;
using fluXis.Screens.Edit.Actions.ToolLogs;
using fluXis.Screens.Edit.Tool;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.EffectTags;

public partial class ToolLogTag : EditorTag, IHasContextMenu
{
    [Resolved]
    public EditorActionStack ActionStack { get; private set; }

    public override Colour4 TagColour => toolLog.Color;

    private ToolLog toolLog => (ToolLog)TimedObject;

    public MenuItem[] ContextMenuItems => new MenuItem[]
    {
        new MenuActionItem("Open Tool", FontAwesome6.Solid.ScrewdriverWrench, MenuItemType.Highlighted, openTool),
        new MenuActionItem("Re-apply", FontAwesome6.Solid.ArrowsRotate, MenuItemType.Highlighted, reApply),
        new MenuSpacerItem(),
        new MenuToggleItem("Effective", FontAwesome6.Solid.Cube, toggleEffective, () => toolLog.Effective),
        new MenuSpacerItem(),
        new MenuActionItem("Delete Log", FontAwesome6.Solid.XMark, MenuItemType.Normal, () => deleteLog(false)),
        new MenuActionItem("Delete Points and Log", FontAwesome6.Solid.Trash, MenuItemType.Dangerous, () => deleteLog(true)),
    };

    public ToolLogTag(EditorTagContainer parent, ToolLog toolLog)
        : base(parent, toolLog)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Alpha = toolLog.Effective ? 1.0f : 0.4f;
    }

    protected override void Update()
    {
        base.Update();
        Text.Text = $"{toolLog.Label}";
    }

    protected override bool OnClick(ClickEvent e)
    {
        openTool();
        return true;
    }

    private void openTool() => Editor.OpenToolFromLog(toolLog);

    private void reApply()
    {
        EditorTool tool = Editor.GetTool(toolLog);
        ActionStack.Add(new EditorReApplyToolLogAction(tool, toolLog));
    }

    private void toggleEffective()
    {
        ActionStack.Add(new EditorToggleToolLogAction(toolLog));
        Alpha = toolLog.Effective ? 1.0f : 0.4f;
    }

    private void deleteLog(bool deletePoints) => ActionStack.Add(new EditorDeleteToolLogAction(toolLog, deletePoints));
}
