using System.Collections.Generic;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Map.Structures.Events.Scrolling;

namespace fluXis.Screens.Edit.Actions.Sv;

public class EditorScaleSvAction : EditorAction
{
    public override string Description => "Scaled sv";

    private List<ITimedObject> items;
    private List<double> oldValues; // this is to avoid losing precision
    private double multiplier;

    public EditorScaleSvAction(List<ITimedObject> items, double multiplier)
    {
        this.multiplier = multiplier;
        this.items = new List<ITimedObject>(items);
    }

    public override void Run(EditorMap map)
    {
        oldValues = new List<double>();

        foreach (var item in items)
        {
            if (item is ScrollVelocity sv)
            {
                oldValues.Add(sv.Multiplier);
                sv.Multiplier *= multiplier;
            }
            else if (item is ScrollMultiplierEvent sm)
            {
                oldValues.Add(sm.Multiplier);
                sm.Multiplier *= (float)multiplier; // what
            }
            else if (item is AdditiveVelocity av)
            {
                oldValues.Add(av.VelocityOffset);
                av.VelocityOffset *= multiplier;
            }
            else continue;

            map.Update(item);
        }
    }

    public override void Undo(EditorMap map)
    {
        int i = 0;

        foreach (var item in items)
        {
            if (item is ScrollVelocity sv) sv.Multiplier = oldValues[i];
            else if (item is ScrollMultiplierEvent sm) sm.Multiplier = (float)oldValues[i];
            else if (item is AdditiveVelocity av) av.VelocityOffset = oldValues[i];
            else continue;

            ++i;
            map.Update(item);
        }
    }
}
