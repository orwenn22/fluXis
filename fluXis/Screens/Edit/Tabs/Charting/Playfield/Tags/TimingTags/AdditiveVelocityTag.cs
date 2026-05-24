using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using Midori.Utils.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.TimingTags;

public partial class AdditiveVelocityTag : EditorTag
{
    public override Colour4 TagColour => Theme.AdditiveVelocity;

    private AdditiveVelocity additiveVelocity => (AdditiveVelocity)TimedObject;

    public AdditiveVelocityTag(EditorTagContainer parent, AdditiveVelocity av)
        : base(parent, av)
    {
    }

    protected override void Update()
    {
        base.Update();

        string sign = additiveVelocity.VelocityOffset >= 0 ? "+" : "";
        string effectName = (additiveVelocity.EffectName != "") ? $" ({additiveVelocity.EffectName})" : "";
        Text.Text = $"{sign}{additiveVelocity.VelocityOffset.ToStringInvariant("0.####")}{effectName}";
    }

    protected override bool OnClick(ClickEvent e)
    {
        Editor.ChangeToTab<DesignTab>(x => x.Container.Sidebar.ShowPoint(additiveVelocity));
        return true;
    }
}
