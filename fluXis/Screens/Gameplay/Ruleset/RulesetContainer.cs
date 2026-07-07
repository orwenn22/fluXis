using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Map;
using fluXis.Mods;
using fluXis.Online.API.Models.Users;
using fluXis.Scoring;
using fluXis.Scoring.Processing.Health;
using fluXis.Screens.Gameplay.Input;
using fluXis.Screens.Gameplay.Ruleset.Playfields;
using fluXis.Utils.Extensions;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Screens.Gameplay.Ruleset;

public partial class RulesetContainer : CompositeDrawable
{
    public List<IMod> Mods { get; }
    public APIUser CurrentPlayer { get; init; }
    public PlayfieldManager PlayfieldManager { get; set; }

    public virtual bool AsyncScoreCalculations => false;

    [CanBeNull]
    [Resolved(CanBeNull = true)]
    private RulesetData resolvedRulesetData { get; set; }

    public RulesetData RulesetData;
    private readonly bool ownRulesetData;

    public event Action OnDeath;

    public DebugText DebugText { get; }

    private DependencyContainer dependencies;

    protected override bool ForceChildUpdate => true;

    public RulesetContainer(List<IMod> mods)
    {
        ownRulesetData = false;
        Mods = mods;

        // PlayfieldManager = new PlayfieldManager(RulesetData.MapInfo); // moved to load()
        DebugText = new DebugText();
    }

    public RulesetContainer(MapInfo map, MapEvents events, List<IMod> mods)
    {
        ownRulesetData = true;
        Mods = mods;

        RulesetData = new RulesetData()
        {
            MapInfo = map,
            MapEvents = events,
        };
        RulesetData.Rate = Mods.OfType<RateMod>().FirstOrDefault()?.Rate ?? 1;
        RulesetData.Input = CreateInput();

        PlayfieldManager = new PlayfieldManager(RulesetData.MapInfo);
        DebugText = new DebugText();

        RulesetData.ShakeTarget ??= this;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        dependencies.CacheAs(this);

        if (ownRulesetData)
        {
            if (RulesetData == null) throw new Exception("Ruleset container is null");

            LoadComponent(RulesetData);
            dependencies.CacheAs(RulesetData);
        }
        else
        {
            if (resolvedRulesetData == null) throw new Exception("No RulesetData found in dependencies");

            RulesetData = resolvedRulesetData;
            PlayfieldManager = new PlayfieldManager(RulesetData.MapInfo);
        }

        createHitWindows();
        createScrollGroups();

        if (ownRulesetData) AddInternal(dependencies.CacheAsAndReturn(RulesetData.Input));
        AddInternal(PlayfieldManager);
        AddInternal(DebugText);
        if (ownRulesetData) AddInternal(RulesetData);
    }

    protected virtual GameplayInput CreateInput() => new(RulesetData.IsPaused.GetBoundCopy(), RulesetData.MapInfo.RealmEntry!.KeyCount, RulesetData.MapInfo.IsDual);

    public HealthProcessor CreateHealthProcessor()
    {
        var processor = null as HealthProcessor;

        var difficulty = Math.Clamp(RulesetData.MapInfo.HealthDifficulty == 0 ? 8 : RulesetData.MapInfo.HealthDifficulty, 1, 10);
        difficulty *= Mods.Any(m => m is HardMod) ? 1.2f : 1f;

        if (Mods.Any(m => m is HardMod)) processor = new DrainHealthProcessor(difficulty);
        else if (Mods.Any(m => m is EasyMod)) processor = new RequirementHeathProcessor(difficulty) { HealthRequirement = EasyMod.HEALTH_REQUIREMENT };

        processor ??= new HealthProcessor(difficulty);
        processor.Clock = Clock;
        processor.InBreak = PlayfieldManager.InBreak;
        processor.OnFail = () => OnDeath?.Invoke();

        foreach (var mod in Mods.OfType<IApplicableToHealthProcessor>())
            mod.Apply(processor);

        return processor;
    }

    //TODO: move this to RulesetData?
    private void createHitWindows()
    {
        var difficulty = Math.Clamp(RulesetData.MapInfo.AccuracyDifficulty == 0 ? 8 : RulesetData.MapInfo.AccuracyDifficulty, 1, 10);
        difficulty *= Mods.Any(m => m is HardMod) ? 1.5f : 1;

        RulesetData.HitWindows = new HitWindows(difficulty, RulesetData.Rate);
        RulesetData.ReleaseWindows = new ReleaseWindows(difficulty, RulesetData.Rate);
        RulesetData.LandmineWindows = new LandmineWindows(difficulty, RulesetData.Rate);
    }

    private void createScrollGroups()
    {
        RulesetData.CreateScrollGroups();
    }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
}
