using System;
using fluXis.Input;
using fluXis.Map.Structures;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace fluXis.Screens.Gameplay.Ruleset.HitObjects;

public partial class DrawableTaikoStrongHit : DrawableHitObject
{
    public override bool CanBeRemoved
    {
        get
        {
            if (Time.Current - Data.Time > HitWindows.TimingFor(HitWindows.LowestHitable)) return true;

            if (Judged)
            {
                // keep the object alive for a bit after so we can register double inputs
                if (Math.Abs(TimeDelta - firstKeyPressDelta) < 15)
                {
                    return false;
                }

                return true;
            }


            return false;
        }
    }

    public double firstKeyPressDelta = 0f;

    public bool IsPrimary
    {
        get
        {
            if (Data.Type == HitObjectType.Normal)
            {
                int lane = (Data.Lane - 1) % 4;
                return lane == 1 || lane == 2;
            }
            else
            {
                return Data.TaikoIsPrimary;
            }
        }
    }

    private bool incorrectKey = false;

    public DrawableTaikoStrongHit(HitObject data)
        : base(data)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChild = Skin.GetTaikoHitObject(IsPrimary).With(d => d.RelativeSizeAxes = Axes.X);
        Width = Skin.SkinJson.Taiko.ColumnWidth; // not necessary, but just in case
    }

    protected override void CheckJudgement(bool byUser, double offset)
    {
        if (!byUser)
        {
            ApplyResult(HitWindows.TimingFor(HitWindows.Lowest));
            return;
        }

        if (!HitWindows.CanBeHit(offset))
            return;

        if (incorrectKey)
        {
            ApplyResult(HitWindows.TimingFor(HitWindows.Lowest));
            return;
        }

        ApplyResult(offset);
        firstKeyPressDelta = TimeDelta;
    }

    public override void OnPressed(FluXisGameplayKeybind key)
    {
        if (!Column.IsFirst(this))
            return;

        JudgeNote(key);
    }

    public void JudgeNote(FluXisGameplayKeybind key)
    {
        if (Judged)
        {
            if (correctKey(key))
            {
                // TODO: register bonus somehow?

                firstKeyPressDelta = 99999; // this is to destroy the object
            }
            else
            {
                int idx = Column.HitObjects.IndexOf(this);

                if (Column.HitObjects.Count > idx + 1)
                {
                    if (Column.HitObjects[idx + 1] is DrawableNote { Judged: false } nextNote)
                        nextNote.JudgeNote(key);
                    else if (Column.HitObjects[idx + 1] is DrawableTaikoStrongHit { Judged: false } nextStrong)
                        nextStrong.JudgeNote(key);
                }
            }
        }

        if (!correctKey(key))
        {
            incorrectKey = true;
            UpdateJudgement(true); // judge self first

            int idx = Column.HitObjects.IndexOf(this);

            if (Column.HitObjects.Count > idx + 1)
            {
                if (Column.HitObjects[idx + 1] is DrawableNote { Judged: false } nextNote)
                    nextNote.JudgeNote(key);
                else if (Column.HitObjects[idx + 1] is DrawableTaikoStrongHit { Judged: false } nextStrong)
                    nextStrong.JudgeNote(key);
            }
        }
        else
        {
            incorrectKey = false;
            UpdateJudgement(true);
        }
    }

    private bool correctKey(FluXisGameplayKeybind key)
    {
        return (IsPrimary && (key == FluXisGameplayKeybind.KeyTaiko2 || key == FluXisGameplayKeybind.KeyTaiko3))
               || (!IsPrimary && (key == FluXisGameplayKeybind.KeyTaiko1 || key == FluXisGameplayKeybind.KeyTaiko4));
    }
}
