using fluXis.Configuration;
using fluXis.Map.Structures.Bases;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.BarTags;

public partial class BarTag : Box
{
    public virtual Colour4 TagColour => Colour4.White;

    [Resolved]
    protected Editor Editor { get; private set; }

    [Resolved]
    private EditorPlayfield playfield { get; set; }

    private BarTagContainer parent { get; }

    public ITimedObject TimedObject { get; }

    virtual public float BarLength => 50f;

    private Bindable<ScrollDirection> scrollDirection;

    public BarTag(BarTagContainer parent, ITimedObject timedObject)
    {
        this.parent = parent;
        TimedObject = timedObject;
    }

    [BackgroundDependencyLoader]
    private void load(FluXisConfig config)
    {
        Width = BarLength;
        Height = 3;
        Anchor = Anchor.TopRight;
        Origin = Anchor.CentreRight;
        Colour = TagColour;

        scrollDirection = config.GetBindable<ScrollDirection>(FluXisSetting.ScrollDirection);
    }

    private void updateScale() => Scale = new Vector2(1, scrollDirection.Value == ScrollDirection.Up ? -1 : 1);

    protected override void LoadComplete()
    {
        base.LoadComplete();

        scrollDirection.BindValueChanged(_ => updateScale(), true);
    }

    protected override void Update()
    {
        base.Update();

        Width = BarLength;
        Colour = TagColour;
        Y = parent.ToLocalSpace(playfield.HitObjectContainer.ScreenSpacePositionAtTime(TimedObject.Time, 0)).Y;
    }
}
