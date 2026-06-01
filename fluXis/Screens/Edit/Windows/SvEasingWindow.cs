using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using fluXis.Overlay.Auth.UI;
using fluXis.Screens.Edit.Actions;
using fluXis.Screens.Edit.Actions.Sv;
using fluXis.Screens.Edit.UI.Variable;
using fluXis.Screens.Edit.UI.Variable.Preset;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace fluXis.Screens.Edit.Windows;

public partial class SvEasingWindow : Window
{
    [Resolved]
    public EditorActionStack ActionStack { get; private set; }

    private readonly EditorSvEasingAction.SvEasingParams svEasingParams;
    private FluXisSpriteText averageText;
    private EditorVariableNumber<int> resolutionField;
    private AuthOverlayTextBox scrollGroupsTextBox;
    private AuthOverlayTextBox effectNameTextBox;

    private SvGraph svGraph;

    public SvEasingWindow()
        : this(new EditorSvEasingAction.SvEasingParams())
    {
    }

    public SvEasingWindow(EditorSvEasingAction.SvEasingParams svEasingParams)
    {
        Title = "SV Easing";
        this.svEasingParams = new EditorSvEasingAction.SvEasingParams(svEasingParams);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Content = new FillFlowContainer
        {
            Width = 500,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 10),
            Padding = new MarginPadding(20),
            Children = new Drawable[]
            {
                scrollGroupsTextBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Scroll groups",
                    Text = string.Join(",", svEasingParams.Groups)
                },
                new EditorVariableTImeNoObject
                {
                    Text = "Start time",
                    TooltipText = "start time.",
                    CurrentValue = svEasingParams.StartTime,
                    FetchStepValue = () => 1,
                    TimeChanged = v =>
                    {
                        svEasingParams.StartTime = v;
                        updateContent();
                    }
                },
                new EditorVariableTImeNoObject
                {
                    Text = "End time",
                    TooltipText = "end time.",
                    CurrentValue = svEasingParams.EndTime,
                    FetchStepValue = () => 1,
                    TimeChanged = v =>
                    {
                        svEasingParams.EndTime = v;
                        updateContent();
                    },
                },
                new EditorVariableDropdown<Easing>
                {
                    Text = "Easing",
                    TooltipText = "The easing function used to interpolate between start and end multiplier.",
                    Items = Enum.GetValues<Easing>().ToList(),
                    CurrentValue = svEasingParams.Easing,
                    OnValueChanged = easing =>
                    {
                        svEasingParams.Easing = easing;
                        updateContent();
                    }
                },
                new EditorVariableNumber<double>
                {
                    Text = "Start multiplier",
                    Formatting = "0.0000",
                    CurrentValue = svEasingParams.StartMultiplier,
                    OnValueChanged = v =>
                    {
                        svEasingParams.StartMultiplier = v;
                        updateContent();
                    }
                },
                new EditorVariableNumber<double>
                {
                    Text = "End multiplier",
                    Formatting = "0.0000",
                    CurrentValue = svEasingParams.EndMultiplier,
                    OnValueChanged = v =>
                    {
                        svEasingParams.EndMultiplier = v;
                        updateContent();
                    }
                },
                resolutionField = new EditorVariableNumber<int>
                {
                    Text = "Resolution",
                    CurrentValue = svEasingParams.Resolution,
                    OnValueChanged = v =>
                    {
                        if (v <= 0)
                        {
                            v = 1;
                            resolutionField.CurrentValue = v;
                        }

                        svEasingParams.Resolution = v;
                        updateContent();
                    }
                },
                new EditorVariableToggle
                {
                    Text = "Correction",
                    TooltipText = "Add AVs to correct the visual positioning between notes.",
                    CurrentValue = svEasingParams.Correction,
                    OnValueChanged = enabled => svEasingParams.Correction = enabled
                },
                new EditorVariableToggle
                {
                    Text = "Use AVs",
                    TooltipText = "Don't use \"Correction\" with this pls.",
                    CurrentValue = svEasingParams.UseAv,
                    OnValueChanged = enabled =>
                    {
                        svEasingParams.UseAv = enabled;
                        updateContent();
                    }
                },
                effectNameTextBox = new AuthOverlayTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Effect name",
                    Alpha = svEasingParams.UseAv ? 1 : 0,
                    Text = svEasingParams.AvEffectName
                },
                new HiddenSection
                {
                    HeaderText = "Preview",
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(5),
                        Child = svGraph = new SvGraph()
                    }
                },
                averageText = new FluXisSpriteText
                {
                    WebFontSize = 16,
                    Colour = Theme.Text,
                    Text = "Average: 1"
                },
                new AuthOverlayButton("Confirm") { Action = confirm },
            }
        };

        updateContent();
    }

    private void updateContent()
    {
        if (svEasingParams.Resolution <= 0) return;

        averageText.Text = $"Average: {EditorSvEasingAction.AverageVelocity(svEasingParams)}";
        effectNameTextBox.Alpha = svEasingParams.UseAv ? 1 : 0;
        svGraph.UpdateContent(svEasingParams);
    }

    private void confirm()
    {
        List<string> groups = new List<string>(scrollGroupsTextBox.Text.Trim().Split(","));
        // string effectName = effectNameTextBox.Text.Trim();

        ActionStack.Add(new EditorSvEasingAction(new EditorSvEasingAction.SvEasingParams(svEasingParams)
        {
            AvEffectName = "",
            Groups = new List<string>(groups)
        }));
    }

    private partial class SvGraph : CompositeDrawable
    {
        private readonly FillFlowContainer topContainer;
        private readonly FillFlowContainer bottomContainer;

        public SvGraph()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Theme.Background1,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            topContainer = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 176,
                                Direction = FillDirection.Horizontal,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 2,
                                Colour = Theme.Text,
                            },
                            bottomContainer = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 176,
                                Direction = FillDirection.Horizontal,
                            }
                        }
                    }
                }
            };
        }

        public void UpdateContent(EditorSvEasingAction.SvEasingParams svEasingParams)
        {
            if (svEasingParams.Resolution <= 0) return;
            if (svEasingParams.StartTime >= svEasingParams.EndTime) return;

            var effect = EditorSvEasingAction.GenerateEffect(svEasingParams);
            var values = (svEasingParams.UseAv) ? effect.Select(e => ((AdditiveVelocity)e).VelocityOffset) : effect.Select(e => ((ScrollVelocity)e).Multiplier);

            var maxScale = Math.Max(Math.Abs(values.Min()), Math.Abs(values.Max()));

            if (maxScale == 0) return;

            Colour4 colour = svEasingParams.UseAv ? Theme.AdditiveVelocity : Theme.ScrollVelocity;
            topContainer.FadeColour(colour, 200, Easing.Out);
            bottomContainer.FadeColour(colour, 200, Easing.Out);

            if (topContainer.Count == svEasingParams.Resolution)
            {
                for (int i = 0; i < svEasingParams.Resolution; i++)
                {
                    var value = values.ElementAt(i);
                    topContainer[i].ResizeHeightTo((value <= 0) ? 0f : (float)(value / maxScale), 200, Easing.Out);
                    bottomContainer[i].ResizeHeightTo((value >= 0) ? 0f : (float)(-value / maxScale), 200, Easing.Out);
                }

                return;
            }

            topContainer.Clear();
            bottomContainer.Clear();

            foreach (var value in values)
            {
                topContainer.Add(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Width = 1f / svEasingParams.Resolution,
                    Height = (value <= 0) ? 0f : (float)(value / maxScale),
                    Colour = Colour4.White
                });

                bottomContainer.Add(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f / svEasingParams.Resolution,
                    Height = (value >= 0) ? 0f : (float)(-value / maxScale),
                    Colour = Colour4.White
                });
            }
        }
    }
}
