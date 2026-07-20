using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Overlay.Auth.UI;
using fluXis.Screens.Edit.Actions;
using fluXis.Screens.Edit.Actions.Sv;
using fluXis.Screens.Edit.UI.Variable;
using fluXis.Screens.Edit.UI.Variable.Preset;
using fluXis.Screens.Edit.Windows.UI;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace fluXis.Screens.Edit.Windows;

public partial class VibratoWindow : Window
{
    [Resolved]
    public EditorActionStack ActionStack { get; private set; }

    [Resolved]
    public Editor Editor { get; private set; }

    [Resolved]
    public EditorMap EditorMap { get; private set; }

    private AuthOverlayTextBox effectNameTextBox;
    private AuthOverlayTextBox scrollGroupsTextBox;

    private readonly EditorVibratoAction.VibratoParams vibratoParams;

    public VibratoWindow()
    {
        vibratoParams = new EditorVibratoAction.VibratoParams();
        Title = "Add Vibrato";
    }

    public VibratoWindow(EditorVibratoAction.VibratoParams vibratoParams)
    {
        this.vibratoParams = vibratoParams;
        Title = "Add Vibrato";
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Content = new FillFlowContainer
        {
            Width = 500,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding(20),
            Spacing = new Vector2(10),
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                effectNameTextBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Effect name",
                    Text = vibratoParams.EffectName
                },
                scrollGroupsTextBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Scroll groups",
                    Text = string.Join(",", vibratoParams.Groups)
                },
                new HiddenSection()
                {
                    HeaderText = "Properties",
                    Collapsed = false,
                    Content = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(20),
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new EditorVariableTImeNoObject
                            {
                                Text = "Start time",
                                TooltipText = "start time.",
                                CurrentValue = vibratoParams.StartTime,
                                FetchStepValue = () => 1,
                                TimeChanged = v => vibratoParams.StartTime = v,
                            },
                            new EditorVariableTImeNoObject
                            {
                                Text = "End time",
                                TooltipText = "end time.",
                                CurrentValue = vibratoParams.EndTime,
                                FetchStepValue = () => 1,
                                TimeChanged = v => vibratoParams.EndTime = v,
                            },
                            new EditorVariableDropdown<Easing>()
                            {
                                Text = "Easing",
                                TooltipText = "The easing function used to interpolate between start and end intensity.",
                                Items = Enum.GetValues<Easing>().ToList(),
                                CurrentValue = vibratoParams.Easing,
                                OnValueChanged = easing => vibratoParams.Easing = easing
                            },
                            new EditorVariableNumber<double>
                            {
                                Text = "Start intensity",
                                CurrentValue = vibratoParams.StartIntensity,
                                OnValueChanged = v => vibratoParams.StartIntensity = v,
                            },
                            new EditorVariableNumber<double>
                            {
                                Text = "End intensity",
                                CurrentValue = vibratoParams.EndIntensity,
                                OnValueChanged = v => vibratoParams.EndIntensity = v,
                            },
                        }
                    }
                },
                new HiddenSection
                {
                    HeaderText = "Advanced",
                    Content = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(20),
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new EditorVariableNumber<double>
                            {
                                Text = "Frequency",
                                CurrentValue = vibratoParams.Frequency,
                                OnValueChanged = v => vibratoParams.Frequency = v,
                            },
                            new EditorVariableNumber<double>
                            {
                                Text = "Up multiplier start",
                                Formatting = "0.0000",
                                CurrentValue = vibratoParams.UpMultiplierStart,
                                OnValueChanged = v => vibratoParams.UpMultiplierStart = v,
                            },
                            new EditorVariableNumber<double>
                            {
                                Text = "Up multiplier end",
                                Formatting = "0.0000",
                                CurrentValue = vibratoParams.UpMultiplierEnd,
                                OnValueChanged = v => vibratoParams.UpMultiplierEnd = v,
                            },
                            new EditorVariableDropdown<Easing>()
                            {
                                Text = "Up multiplier easing",
                                Items = Enum.GetValues<Easing>().ToList(),
                                CurrentValue = vibratoParams.UpMultiplierEasing,
                                OnValueChanged = easing => vibratoParams.UpMultiplierEasing = easing
                            },
                            new EditorVariableNumber<double>
                            {
                                Text = "Down multiplier start",
                                Formatting = "0.0000",
                                CurrentValue = vibratoParams.DownMultiplierStart,
                                OnValueChanged = v => vibratoParams.DownMultiplierStart = v,
                            },
                            new EditorVariableNumber<double>
                            {
                                Text = "Down multiplier end",
                                Formatting = "0.0000",
                                CurrentValue = vibratoParams.DownMultiplierEnd,
                                OnValueChanged = v => vibratoParams.DownMultiplierEnd = v,
                            },
                            new EditorVariableDropdown<Easing>()
                            {
                                Text = "Down multiplier easing",
                                Items = Enum.GetValues<Easing>().ToList(),
                                CurrentValue = vibratoParams.DownMultiplierEasing,
                                OnValueChanged = easing => vibratoParams.DownMultiplierEasing = easing
                            },
                        },
                    }
                },
                new AuthOverlayButton("Confirm") { Action = confirm },
            }
        };
    }

    private void confirm()
    {
        List<string> groups = new List<string>(scrollGroupsTextBox.Text.Trim().Split(","));
        string effectName = effectNameTextBox.Text.Trim();

        ActionStack.Add(new EditorVibratoAction(new EditorVibratoAction.VibratoParams(vibratoParams)
        {
            EffectName = effectName,
            Groups = new List<string>(groups)
        }));
    }
}
