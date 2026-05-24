using System.Collections.Generic;
using System.Linq;
using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Screens.Edit.Tabs.Shared.Points.List;
using fluXis.Screens.Edit.UI.Variable;
using Midori.Utils;
using Midori.Utils.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;

namespace fluXis.Screens.Edit.Tabs.Design.Points.Entries;

public partial class AdditiveVelocityEntry : PointListEntry
{
    protected override string Text => "Additive Velocity";
    protected override Colour4 Color => Theme.AdditiveVelocity;

    private AdditiveVelocity av => Object as AdditiveVelocity;

    public AdditiveVelocityEntry(AdditiveVelocity av)
        : base(av)
    {
    }

    public override ITimedObject CreateClone() => av.JsonCopy();

    protected override Drawable[] CreateValueContent()
    {
        string sign = av.VelocityOffset >= 0 ? "+" : "";
        string effectName = (av.EffectName != "") ? $" ({av.EffectName})" : "";
        return new FluXisSpriteText
        {
            Text = $"{sign}{av.VelocityOffset.ToStringInvariant("0.####")}{effectName}",
            Colour = Color
        }.Yield().ToArray<Drawable>();
    }

    protected override IEnumerable<Drawable> CreateSettings() => base.CreateSettings().Concat(new Drawable[]
    {
        new EditorVariableNumber<double>
        {
            Text = "Velocity Offset",
            TooltipText = "The speed to add to the scroll velocity.",
            ExtraText = "x",
            TextBoxWidth = 195,
            Formatting = "0.0000",
            CurrentValue = av.VelocityOffset,
            OnValueChanged = v =>
            {
                av.VelocityOffset = v;
                Map.Update(av);
            }
        },
        new EditorVariableTextBox
        {
            Text = "Effect name",
            TooltipText = "The effect name.",
            TextBoxWidth = 195,
            CurrentValue = av.EffectName,
            OnValueChanged = box =>
            {
                av.EffectName = box.Text;
                Map.Update(av);
            }
        },
        new EditorVariableLaneMask(Map, av),
        new EditorGroupInfo(Map, av)
    });
}
