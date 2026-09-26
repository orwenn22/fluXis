using System;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using osu.Framework.Graphics;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags.BarTags;

public partial class AdditiveVelocityBarTag : BarTag
{
    public override Colour4 TagColour => additiveVelocity.VelocityOffset < 0 ? Theme.Orange : Theme.AdditiveVelocity;

    private AdditiveVelocity additiveVelocity => (AdditiveVelocity)TimedObject;

    public override float BarLength => Math.Clamp(Math.Abs((float)additiveVelocity.VelocityOffset), 0.01f, 10f) * 25f; // max length of 250

    public AdditiveVelocityBarTag(BarTagContainer parent, ITimedObject timedObject)
        : base(parent, timedObject)
    {
    }
}
