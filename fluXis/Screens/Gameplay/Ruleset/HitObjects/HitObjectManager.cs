using System.Linq;
using fluXis.Configuration;
using fluXis.Map.Structures;
using fluXis.Screens.Gameplay.Audio.Hitsounds;
using fluXis.Screens.Gameplay.Input;
using fluXis.Screens.Gameplay.Ruleset.Playfields;
using fluXis.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Screens.Gameplay.Ruleset.HitObjects;

public partial class HitObjectManager : Container<HitObjectColumn>
{
    [Resolved]
    private ISkin skin { get; set; }

    [Resolved]
    private SkinManager skinManager { get; set; }

    [Resolved]
    private RulesetContainer ruleset { get; set; }

    [Resolved]
    private Playfield playfield { get; set; }

    [Resolved]
    private Hitsounding hitsounding { get; set; }

    private GameplayInput input => ruleset.Input;

    private Bindable<bool> useSnapColors;
    public bool UseSnapColors => useSnapColors.Value;

    public float ScrollSpeed
    {
        get
        {
            var speed = playfield.RealmMap.Settings.ScrollSpeed ?? ruleset.ScrollSpeed.Value;
            return speed * (speed / (speed * ruleset.Rate));
        }
    }

    private Bindable<bool> hitsounds;

    public double VisualTimeOffset { get; set; } = 0;

    public int KeyCount => playfield.RealmMap.KeyCount; // TODO: get rid of this

    public float HitPosition => DrawHeight - skin.SkinJson.Taiko.HitPosition - skin.SkinJson.Taiko.ColumnWidth / 2f;

    public bool Finished { get; private set; }

    public bool Break => timeUntilNextHitObject >= 2000;
    private double timeUntilNextHitObject => (nextHitObject?.Time ?? double.MaxValue) - Clock.CurrentTime;

    private HitObject nextHitObject
    {
        get
        {
            var all = this.Select(l => l.NextUp);
            return all.MinBy(h => h?.Time);
        }
    }

    [BackgroundDependencyLoader]
    private void load(FluXisConfig config)
    {
        RelativeSizeAxes = Axes.Both;

        InternalChild = new HitObjectColumn(ruleset.MapInfo, ruleset, this);

        useSnapColors = config.GetBindable<bool>(FluXisSetting.SnapColoring);
        hitsounds = config.GetBindable<bool>(FluXisSetting.Hitsounding);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        if (!playfield.IsSubPlayfield)
        {
            input.OnPress += _ =>
            {
                var hit = Child.NextUp;

                if (hit == null)
                    return;

                PlayHitSound(hit);
            };
        }
    }

    protected override void Update()
    {
        Finished = Children.All(l => l.Finished);
    }

    public float PositionAtLane(float lane)
    {
        return 0;
    }

    public Easing EasingAtTime(double time)
    {
        var events = ruleset.MapEvents.HitObjectEaseEvents;

        if (events.Count == 0)
            return Easing.None;

        var first = events.LastOrDefault(e => e.Time <= time);
        return first?.Easing ?? Easing.None;
    }

    public float WidthOfLane(int lane) => skin.SkinJson.Taiko.ColumnWidth;

    public DrawableHitObject CreateHitObject(HitObject hitObject)
    {
        var drawable = GetDrawableFor(hitObject);

        drawable.OnLoadComplete += _ =>
        {
            for (var i = 0; i < input.Pressed.Length; i++)
            {
                if (!input.Pressed[i])
                    continue;

                var bind = input.Keys[i];
                drawable.OnPressed(bind);
            }
        };

        return drawable;
    }

    public static DrawableHitObject GetDrawableFor(HitObject hit)
    {
        switch (hit.Type)
        {
            case HitObjectType.Tick:
                return new DrawableTickNote(hit);

            case HitObjectType.Landmine:
                return new DrawableLandmine(hit);

            case HitObjectType.TaikoHit:
                return new DrawableNote(hit);

            case HitObjectType.TaikoStrong:
                return new DrawableTaikoStrongHit(hit);

            default:
            {
                if (hit.LongNote)
                    return new DrawableLongNote(hit);

                return new DrawableTaikoStrongHit(hit);
            }
        }
    }

    public void PlayHitSound(HitObject hitObject, bool userTriggered = true)
    {
        if (ruleset.CatchingUp || playfield.IsSubPlayfield)
            return;

        // ignore hitsounds when the next is a
        // tick note since it would be played twice
        // when hitting them as a normal note
        if (hitObject is { Type: HitObjectType.Tick } && userTriggered) return;

        var sound = hitObject.HitSound;

        if (sound == ":normal" && hitObject.Type == HitObjectType.Tick)
        {
            sound = ":tick-big";

            if (hitObject.HoldTime > 0)
                sound = ":tick-small";
        }

        var channel = hitsounding.GetSample(sound, hitsounds.Value && !playfield.RealmMap.Settings.DisableHitSounds);
        channel?.Play();
    }
}
