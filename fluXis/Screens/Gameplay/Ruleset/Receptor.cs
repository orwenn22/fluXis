using fluXis.Screens.Gameplay.Ruleset.Playfields;
using fluXis.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Screens.Gameplay.Ruleset;

public partial class Receptor : CompositeDrawable
{
    [Resolved]
    private ISkin skin { get; set; }

    [Resolved]
    private RulesetContainer ruleset { get; set; }

    [Resolved]
    private Playfield playfield { get; set; }

    public override bool RemoveCompletedTransforms => true;

    private Drawable up;
    private Drawable downPrimary;
    private Drawable downSecondary;

    private BindableInt lastKeyPress { get; } = new(); // 0: nothing, 1: primary, 2: secondary

    [BackgroundDependencyLoader]
    private void load()
    {
        Width = skin.SkinJson.Taiko.ColumnWidth;
        Height = skin.SkinJson.Taiko.ColumnWidth;
        // AutoSizeAxes = Axes.Y;
        Anchor = Anchor.BottomCentre;
        Origin = Anchor.BottomCentre;
        Masking = true;

        InternalChildren = new[]
        {
            up = skin.GetTaikoReceptor(),
            downPrimary = skin.GetTaikoReceptorDown(true),
            downSecondary = skin.GetTaikoReceptorDown(false),
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        lastKeyPress.BindValueChanged(v =>
        {
            if (v.NewValue == 0)
            {
                up.Show();
                downPrimary.Hide();
                downSecondary.Hide();
            }
            else if (v.NewValue == 1)
            {
                up.Hide();
                downPrimary.Show();
                downSecondary.Hide();
            }
            else if (v.NewValue == 2)
            {
                up.Hide();
                downPrimary.Hide();
                downSecondary.Show();
            }
        }, true);

        FinishTransforms(true);
    }

    protected override void Update()
    {
        int baseKeyIndex = playfield.Index * 4;

        bool allReleased = true;

        for (int keyIndex = 0; keyIndex < 4; keyIndex++)
        {
            if (!ruleset.Input.Pressed[baseKeyIndex + keyIndex]) continue;

            allReleased = false;
            if (keyIndex == 1 || keyIndex == 2) lastKeyPress.Value = 1;
            else lastKeyPress.Value = 2;
        }

        if (allReleased) lastKeyPress.Value = 0;
    }
}
