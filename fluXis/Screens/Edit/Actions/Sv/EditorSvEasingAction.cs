using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Utils;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace fluXis.Screens.Edit.Actions.Sv;

// NOTE: here the generic average and correction functions in SvUtils cannot be used because this might generate an effect with only AVs

public class EditorSvEasingAction : EditorAction
{
    public override string Description => "Generate SV easing";

    private readonly SvEasingParams svEasingParams;
    private List<ITimedObject> addedEffects;
    private ToolLog addedLog;

    public EditorSvEasingAction(SvEasingParams svEasingParams)
    {
        this.svEasingParams = svEasingParams;
    }

    public override void Run(EditorMap map)
    {
        addedEffects = new List<ITimedObject>();

        if (svEasingParams.Groups.Count == 0 || (svEasingParams.Groups.Count == 1 && string.IsNullOrEmpty(svEasingParams.Groups[0])))
        {
            svEasingParams.Groups.Clear();
            for (int i = 1; i <= map.MapInfo.KeyCount; i++) svEasingParams.Groups.Add($"${i}");
        }

        addedEffects.AddRange(GenerateEffect(svEasingParams));
        if (svEasingParams.Correction) addedEffects.AddRange(GenerateCorrection(svEasingParams, map, AverageVelocity(svEasingParams))); // maybe forcing an average velocity as a parameter could be a good thing?

        string tag = MapUtils.GetHash(JsonConvert.SerializeObject(svEasingParams));
        addedEffects.ForEach(e =>
        {
            if (e is IHasTag hasTag) hasTag.Tag = tag;
        });

        foreach (var timedObject in addedEffects) map.Add(timedObject);

        map.Add(addedLog = new ToolLog
        {
            Time = svEasingParams.StartTime,
            Color = svEasingParams.UseAv ? Theme.AdditiveVelocity : Theme.ScrollVelocity,
            Label = svEasingParams.UseAv ? $"AV easing ({svEasingParams.AvEffectName})" : "SV easing",
            ToolName = "orwenn22.sveasing",
            ToolSettings = JsonConvert.SerializeObject(svEasingParams),
            Tag = tag,
            Effective = true
        });
    }

    public override void Undo(EditorMap map)
    {
        map.Remove(addedLog);
        foreach (var e in addedEffects) map.Remove(e);
    }

    public static List<ITimedObject> GenerateEffect(SvEasingParams svEasingParams)
    {
        double timeInterval = (svEasingParams.EndTime - svEasingParams.StartTime) / svEasingParams.Resolution;
        DefaultEasingFunction easingFunction = new DefaultEasingFunction(svEasingParams.Easing);
        List<ITimedObject> result = new List<ITimedObject>();

        double time = svEasingParams.StartTime;

        for (int i = 0; i < svEasingParams.Resolution; i++)
        {
            double progress = (time + (timeInterval / 2.0) - svEasingParams.StartTime) / (svEasingParams.EndTime - svEasingParams.StartTime);
            double easedProgress = easingFunction.ApplyEasing(progress);
            double finalMultiplier = svEasingParams.StartMultiplier + (svEasingParams.EndMultiplier - svEasingParams.StartMultiplier) * easedProgress;

            if (svEasingParams.UseAv)
            {
                result.Add(new AdditiveVelocity
                {
                    Time = time,
                    EffectName = svEasingParams.AvEffectName,
                    Groups = new List<string>(svEasingParams.Groups),
                    VelocityOffset = finalMultiplier,
                });
            }
            else
            {
                result.Add(new ScrollVelocity
                {
                    Time = time,
                    Groups = new List<string>(svEasingParams.Groups),
                    Multiplier = finalMultiplier,
                });
            }

            time += timeInterval;
        }

        return result;
    }

    public static List<ITimedObject> GenerateCorrection(SvEasingParams svParams, EditorMap map, double targetVelocity)
    {
        var corrective = new List<ITimedObject>();

        var notes = map.MapInfo.HitObjects
                       .Where(h => h.Time > svParams.StartTime && h.Time < svParams.EndTime)
                       .OrderBy(h => h.Time)
                       .ToList();

        foreach (var note in notes)
        {
            double noteTime = note.Time;

            // ok so, basically, for every note, we want to add an additive AV spike to make it so the average velocity
            // between the start of the effect and the note is the target velocity
            double windowMs = noteTime - svParams.StartTime; // total ms we care about
            const double spike_ms = 0.05; // width of the corrective spike (one side)

            // Average of base effect over [StartTime, noteTime - spikeMs]
            double avgBefore = AverageVelocityInRange(svParams, svParams.StartTime, noteTime - spike_ms);

            // Solve: targetVelocity * windowMs = avgBefore * (windowMs - spikeMs) + correctiveValue * spikeMs
            double correctiveValue = (targetVelocity * windowMs - avgBefore * (windowMs - spike_ms)) / spike_ms;

            corrective.Add(new AdditiveVelocity
            {
                Time = noteTime - spike_ms,
                EffectName = svParams.AvEffectName + "_corr",
                Groups = new List<string>(svParams.Groups),
                VelocityOffset = correctiveValue,
            });
            corrective.Add(new AdditiveVelocity
            {
                Time = noteTime,
                EffectName = svParams.AvEffectName + "_corr",
                Groups = new List<string>(svParams.Groups),
                VelocityOffset = -correctiveValue, // cancel the spike
            });
            corrective.Add(new AdditiveVelocity
            {
                Time = noteTime + spike_ms,
                EffectName = svParams.AvEffectName + "_corr",
                Groups = new List<string>(svParams.Groups),
                VelocityOffset = 0, // end of correction
            });
        }

        return corrective;
    }

    public static double AverageVelocity(SvEasingParams svEasingParams)
    {
        double timeInterval = (svEasingParams.EndTime - svEasingParams.StartTime) / svEasingParams.Resolution;
        DefaultEasingFunction easingFunction = new DefaultEasingFunction(svEasingParams.Easing);
        double sum = 0;

        double time = svEasingParams.StartTime;

        for (int i = 0; i < svEasingParams.Resolution; i++)
        {
            double progress = (time + (timeInterval / 2.0) - svEasingParams.StartTime) / (svEasingParams.EndTime - svEasingParams.StartTime);
            double easedProgress = easingFunction.ApplyEasing(progress);
            double finalMultiplier = svEasingParams.StartMultiplier + (svEasingParams.EndMultiplier - svEasingParams.StartMultiplier) * easedProgress;

            sum += finalMultiplier;

            time += timeInterval;
        }

        return sum / svEasingParams.Resolution;
    }

    public static double AverageVelocityInRange(SvEasingParams svParams, double rangeStart, double rangeEnd)
    {
        double timeInterval = (svParams.EndTime - svParams.StartTime) / svParams.Resolution;
        DefaultEasingFunction easingFunction = new DefaultEasingFunction(svParams.Easing);

        double weightedSum = 0;
        double totalWeight = 0;
        double time = svParams.StartTime;

        for (int i = 0; i < svParams.Resolution; i++)
        {
            double stepEnd = time + timeInterval;

            // clamp the step to the requested range
            double clampedStart = Math.Max(time, rangeStart);
            double clampedEnd = Math.Min(stepEnd, rangeEnd);

            if (clampedEnd > clampedStart)
            {
                double midProgress = (time + (timeInterval / 2.0) - svParams.StartTime) / (svParams.EndTime - svParams.StartTime);
                double easedProgress = easingFunction.ApplyEasing(midProgress);
                double stepMultiplier = svParams.StartMultiplier + (svParams.EndMultiplier - svParams.StartMultiplier) * easedProgress;

                double weight = clampedEnd - clampedStart;
                weightedSum += stepMultiplier * weight;
                totalWeight += weight;
            }

            time += timeInterval;
        }

        return totalWeight > 0 ? weightedSum / totalWeight : svParams.StartMultiplier;
    }

    public class SvEasingParams
    {
        [JsonProperty("start-time")]
        public double StartTime;

        [JsonProperty("end-time")]
        public double EndTime;

        [JsonProperty("groups")]
        public List<string> Groups;

        [JsonProperty("start-multiplier")]
        public double StartMultiplier;

        [JsonProperty("end-multiplier")]
        public double EndMultiplier;

        [JsonProperty("easing")]
        public Easing Easing;

        [JsonProperty("resolution")]
        public int Resolution;

        [DefaultValue(false)]
        [JsonProperty("correction", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Correction;

        [DefaultValue(false)]
        [JsonProperty("use-av", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool UseAv;

        [DefaultValue("")]
        [JsonProperty("av-effect-name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AvEffectName;

        public SvEasingParams()
        {
            StartTime = 0;
            EndTime = 0;
            Groups = new List<string>();
            StartMultiplier = 1;
            EndMultiplier = 1;
            Easing = Easing.None;
            Resolution = 8;
            Correction = false;
            UseAv = false;
            AvEffectName = "";
        }

        public SvEasingParams(SvEasingParams other)
        {
            StartTime = other.StartTime;
            EndTime = other.EndTime;
            Groups = new List<string>(other.Groups);
            StartMultiplier = other.StartMultiplier;
            EndMultiplier = other.EndMultiplier;
            Easing = other.Easing;
            Resolution = other.Resolution;
            Correction = other.Correction;
            UseAv = other.UseAv;
            AvEffectName = other.AvEffectName;
        }
    }
}
