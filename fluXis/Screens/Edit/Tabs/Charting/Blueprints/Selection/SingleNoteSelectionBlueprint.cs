using fluXis.Map.Structures;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace fluXis.Screens.Edit.Tabs.Charting.Blueprints.Selection;

public partial class SingleNoteSelectionBlueprint : NoteSelectionBlueprint
{
    public SingleNoteSelectionBlueprint(HitObject info)
        : base(info)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChild = new BlueprintNotePiece
        {
            RelativeSizeAxes = Axes.X,
            Width = 0.5f,
            Anchor = Anchor.Centre
        };
    }

    protected override void Update()
    {
        base.Update();
        Height = HitObject.DrawHeight;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Middle)
            return false;

        if (Object.Type == HitObjectType.Tick)
        {
            Object.HoldTime = Object.HoldTime > 0 ? 0 : 1;
        }
        else if (Object.Type == HitObjectType.Landmine)
        {
            Object.Hidden = !Object.Hidden;
        }

        return true;
    }
}
