using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using fluXis.Input;
using fluXis.Map;
using fluXis.Mods;
using fluXis.Replays;
using fluXis.Screens.Gameplay.Input;
using fluXis.Screens.Gameplay.Ruleset;
using osu.Framework.Timing;

namespace fluXis.Screens.Gameplay.Replays;

public partial class ReplayRulesetContainer : RulesetContainer, IFrameBasedClock, IAdjustableClock
{
    public override bool AsyncScoreCalculations => true;

    public Replay Replay { get; private set; }
    public bool RequireSyncFrames { get; set; } = false;

    private List<ReplayFrame> frames { get; set; }
    private Stack<ReplayFrame> handledFrames { get; }
    private List<FluXisGameplayKeybind> currentPressed = new();

    public double CurrentTime { get; private set; }
    public double ElapsedFrameTime { get; private set; }
    public double FramesPerSecond => RulesetData.ParentClock.FramesPerSecond;
    public bool IsRunning => RulesetData.ParentClock.IsRunning;

    double IClock.Rate => RulesetData.ParentClock.Rate;

    double IAdjustableClock.Rate
    {
        get => RulesetData.ParentClock.Rate;
        set => RulesetData.ParentClock.Rate = value;
    }

    public ReplayRulesetContainer(Replay replay, List<IMod> mods)
        : base(mods)
    {
        Replay = replay;

        frames = replay.Frames;
        handledFrames = new Stack<ReplayFrame>();

        Clock = this;
        CurrentTime = -4000;
    }

    public ReplayRulesetContainer(Replay replay, MapInfo map, MapEvents events, List<IMod> mods)
        : base(map, events, mods)
    {
        Replay = replay;
        RulesetData.AllowReverting = true;

        frames = replay.Frames;
        handledFrames = new Stack<ReplayFrame>();

        Clock = this;
        CurrentTime = -4000;
    }

    protected override GameplayInput CreateInput() => new ReplayInput(RulesetData.IsPaused.GetBoundCopy(), RulesetData.MapInfo.RealmEntry!.KeyCount, RulesetData.MapInfo.IsDual);

    private int skippedFrames = 0;
    private double skipElapsed = 0;

    public override bool UpdateSubTree()
    {
        var target = RulesetData.ParentClock.CurrentTime;
        ElapsedFrameTime = target - CurrentTime;
        RulesetData.CatchingUp = Math.Abs(ElapsedFrameTime) > 20;

        if (target > CurrentTime)
        {
            if (frames.Count != 0)
            {
                var first = frames[0];

                if (target > first.Time)
                    target = first.Time;
            }
        }
        else if (target < CurrentTime)
        {
            if (handledFrames.Count != 0)
            {
                var frame = handledFrames.Peek();

                if (target < frame.Time)
                    target = frame.Time - 1;
            }
        }

        if (RequireSyncFrames)
        {
            if (target > Replay.LastSync)
            {
                reset();
                RulesetData.ParentClock.Stop();
                return base.UpdateSubTree();
            }

            if (!RulesetData.ParentClock.IsRunning)
                RulesetData.ParentClock.Start();
        }

        CurrentTime = target;

        if (Math.Abs(RulesetData.ParentClock.CurrentTime - CurrentTime) > 40 && skippedFrames < 100 && skipElapsed < 10)
        {
            skippedFrames++;

            var sw = new Stopwatch();

            sw.Start();
            base.UpdateSubTree();
            sw.Stop();

            var el = sw.ElapsedTicks / TimeSpan.TicksPerMillisecond;
            skipElapsed += el;

            UpdateSubTree();
            return true;
        }

        reset();
        return base.UpdateSubTree();

        void reset()
        {
            skippedFrames = 0;
            skipElapsed = 0;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (frames.Count >= 0)
        {
            while (frames.Count > 0 && frames[0].Time <= Clock.CurrentTime)
            {
                var frame = frames[0];
                frames.RemoveAt(0);
                handledFrames.Push(frame);

                if (frame.Type == ReplayFrameType.Input)
                    handlePresses(frame.Actions);
            }

            while (handledFrames.Count > 0)
            {
                var result = handledFrames.Peek();

                if (Clock.CurrentTime >= result.Time)
                    break;

                revertFrame(handledFrames.Pop());
            }
        }
    }

    private void revertFrame(ReplayFrame frame)
    {
        switch (frame.Type)
        {
            case ReplayFrameType.Input:
            {
                foreach (var keybind in currentPressed)
                    RulesetData.Input.ReleaseKey(keybind);

                currentPressed.Clear();
                break;
            }
        }

        frames.Insert(0, frame);
    }

    private void handlePresses(List<int> frameActionsInt)
    {
        var frameActions = frameActionsInt.Select(i => (FluXisGameplayKeybind)i).ToList();

        foreach (var keybind in frameActions)
        {
            if (currentPressed.Contains(keybind))
                continue;

            RulesetData.Input.PressKey(keybind);
        }

        foreach (var keybind in currentPressed)
        {
            if (frameActions.Contains(keybind))
                continue;

            RulesetData.Input.ReleaseKey(keybind);
        }

        currentPressed = frameActions;
    }

    public void ReplaceReplay(Replay replay)
    {
        handledFrames.Clear();

        RulesetData.CatchingUp = true; // prevents hit sounds from being played
        Replay = replay;
        frames = Replay.Frames;

        if (frames.Count >= 0)
        {
            while (frames.Count > 0 && frames[0].Time <= Clock.CurrentTime)
            {
                var frame = frames[0];
                frames.RemoveAt(0);
                handledFrames.Push(frame);

                // no need to handle presses past the current time
            }
        }
    }

    public void Reset() => RulesetData.ParentClock.Reset();
    public void Start() => RulesetData.ParentClock.Start();
    public void Stop() => RulesetData.ParentClock.Stop();
    public bool Seek(double position) => RulesetData.ParentClock.Seek(position);
    public void ResetSpeedAdjustments() => RulesetData.ParentClock.ResetSpeedAdjustments();

    public void ProcessFrame() { }
}
