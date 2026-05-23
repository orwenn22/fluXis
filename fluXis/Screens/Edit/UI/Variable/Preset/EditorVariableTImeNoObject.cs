using System;
using osu.Framework.Graphics;

namespace fluXis.Screens.Edit.UI.Variable.Preset;

#nullable enable

public partial class EditorVariableTImeNoObject : EditorVariableNumber<double>
{
    private Func<double> getOffset { get; }

    public Action<double>? TimeChanged { get; init; }

    public EditorVariableTImeNoObject(Func<double>? getOffset = null)
    {
        this.getOffset = getOffset ?? (() => 0);

        Text = "Time";
        TooltipText = "The time in milliseconds when the event should trigger.";
        Formatting = "0.0000";
        FetchStepValue = () => 1;
        OnValueChanged = v =>
        {
            TimeChanged?.Invoke(v);
        };
    }

    protected override Drawable CreateExtraButton() => new EditorVariableToCurrentButton
    {
        Action = t =>
        {
            t -= getOffset();
            CurrentValue = t;
            TimeChanged?.Invoke(t);
        }
    };
}
