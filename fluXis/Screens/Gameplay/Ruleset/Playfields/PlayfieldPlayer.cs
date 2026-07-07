using System.Linq;
using fluXis.Database.Maps;
using fluXis.Map;
using fluXis.Mods;
using fluXis.Online.API.Models.Users;
using fluXis.Scoring;
using fluXis.Scoring.Processing;
using fluXis.Scoring.Processing.Health;
using fluXis.Screens.Gameplay.HUD;
using fluXis.Utils.Extensions;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Screens.Gameplay.Ruleset.Playfields;

public partial class PlayfieldPlayer : CompositeDrawable, IHUDDependencyProvider
{
    [CanBeNull]
    [Resolved(CanBeNull = true)]
    private GameplaySamples samples { get; set; }

    [Resolved]
    private RulesetContainer ruleset { get; set; }

    [Resolved]
    private RulesetData rulesetData { get; set; }

    public Playfield MainPlayfield { get; }
    public Playfield[] SubPlayfields { get; }

    RulesetContainer IHUDDependencyProvider.Ruleset => ruleset;
    public JudgementProcessor JudgementProcessor { get; } = new();
    public HealthProcessor HealthProcessor { get; private set; }
    public ScoreProcessor ScoreProcessor { get; private set; }

    private DependencyContainer dependencies;

    public PlayfieldPlayer(int index, int subCount)
    {
        MainPlayfield = new Playfield(index, 0);
        SubPlayfields = Enumerable.Range(1, subCount).Select(x => new Playfield(index, x)).ToArray();
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        dependencies.CacheAs(this);

        JudgementProcessor.AddDependants(new JudgementDependant[]
        {
            HealthProcessor = ruleset.CreateHealthProcessor(),
            ScoreProcessor = new ScoreProcessor(x => Schedule(x), ruleset.AsyncScoreCalculations)
            {
                Player = ruleset.CurrentPlayer ?? APIUser.Default,
                HitWindows = rulesetData.HitWindows,
                MapInfo = rulesetData.MapInfo,
                Mods = ruleset.Mods
            }
        });

        dependencies.CacheAs(JudgementProcessor);
        dependencies.CacheAs(HealthProcessor);

        AddInternal(dependencies.CacheAsAndReturn(new LaneSwitchManager(rulesetData.MapEvents.LaneSwitchEvents, rulesetData.MapInfo.RealmEntry!.KeyCount, rulesetData.MapInfo.NewLaneSwitchLayout, ruleset.Mods.Any(x => x is MirrorMod))));

        var content = new SortingContainer { RelativeSizeAxes = Axes.Both };
        content.Child = MainPlayfield;
        content.AddRange(SubPlayfields);
        AddInternal(content);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        JudgementProcessor.ApplyMap(rulesetData.MapInfo);
        HealthProcessor.OnSavedDeath += () => samples?.EarlyFail();
        ScoreProcessor.OnComboBreak += () =>
        {
            if (rulesetData.CatchingUp)
                return;

            samples?.Miss();
        };
    }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

    private partial class SortingContainer : Container<Playfield>
    {
        protected override int Compare(Drawable x, Drawable y)
        {
            var a = (Playfield)x;
            var b = (Playfield)y;

            var result = -a.AnimationZ.CompareTo(b.AnimationZ);

            if (result != 0)
                return result;

            return -a.SubIndex.CompareTo(b.SubIndex);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            SortInternal();
        }
    }

    protected override void Dispose(bool isDisposing)
    {
        ScoreProcessor.Dispose();
        base.Dispose(isDisposing);
    }

    #region IHUDDependencyProvider

    HitWindows IHUDDependencyProvider.HitWindows => rulesetData.HitWindows;
    RealmMap IHUDDependencyProvider.RealmMap => rulesetData.MapInfo.RealmEntry;
    MapInfo IHUDDependencyProvider.MapInfo => rulesetData.MapInfo;
    float IHUDDependencyProvider.PlaybackRate => rulesetData.Rate;
    double IHUDDependencyProvider.CurrentTime => ruleset.Time.Current;

    #endregion
}
