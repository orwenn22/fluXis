using System;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.BarTags;

public partial class ScrollVelocityBarTag : BarTag
{
    public override Colour4 TagColour => velocity.Multiplier < 0 ? Theme.Red : Theme.ScrollVelocity;
    private ScrollVelocity velocity => (ScrollVelocity)TimedObject;

    public override float BarLength => Math.Clamp(Math.Abs((float)velocity.Multiplier), 0.01f, 10f) * 25f; // max length of 250

    public ScrollVelocityBarTag(BarTagContainer parent, ScrollVelocity timedObject)
        : base(parent, timedObject)
    {
    }

    protected override bool OnClick(ClickEvent e)
    {
        Editor.ChangeToTab<DesignTab>(x => x.Container.Sidebar.ShowPoint(velocity));
        return true;
    }
}
