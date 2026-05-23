using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace fluXis.Screens.Edit.Actions.Sv;

public class EditorVibratoAction : EditorAction
{
    public override string Description => "Add vibrato";

    private double vibrationLength = 0.1;

    VibratoParams vibratoParams;

    private List<AdditiveVelocity> addedVelocities;
    private ToolLog toolLog;

    // old constructor, keeping it just in case
    public EditorVibratoAction(double startTime, double endTime, double startIntensity, double endIntensity, Easing easing, string effectName, List<string> groups)
    {
        vibratoParams = new VibratoParams
        {
            EffectName = effectName,
            Groups = new List<string>(groups),
            Frequency = 120,
            StartTime = startTime,
            EndTime = endTime,
            StartIntensity = startIntensity,
            EndIntensity = endIntensity,
            Easing = easing,
            UpMultiplierStart = 1,
            UpMultiplierEnd = 1,
            UpMultiplierEasing = Easing.None,
            DownMultiplierStart = 1,
            DownMultiplierEnd = 1,
            DownMultiplierEasing = Easing.None
        };
    }

    //TODO?: configurable vibrato length?
    public EditorVibratoAction(
        string effectName, List<string> groups, double frequency,
        double startTime, double endTime, double startIntensity, double endIntensity, Easing easing,
        double upMultiplierStart, double upMultiplierEnd, Easing upMultiplierEasing,
        double downMultiplierStart, double downMultiplierEnd, Easing downMultiplierEasing)
    {
        vibratoParams = new VibratoParams
        {
            EffectName = effectName,
            Groups = new List<string>(groups),
            Frequency = frequency,
            StartTime = startTime,
            EndTime = endTime,
            StartIntensity = startIntensity,
            EndIntensity = endIntensity,
            Easing = easing,
            UpMultiplierStart = upMultiplierStart,
            UpMultiplierEnd = upMultiplierEnd,
            UpMultiplierEasing = upMultiplierEasing,
            DownMultiplierStart = downMultiplierStart,
            DownMultiplierEnd = downMultiplierEnd,
            DownMultiplierEasing = downMultiplierEasing
        };
    }

    public EditorVibratoAction(VibratoParams vibratoParams)
    {
        this.vibratoParams = new VibratoParams(vibratoParams);
    }

    public override void Run(EditorMap map)
    {
        //TODO: check if there are other effects called "vibrato" in the time range, and append some number if there is
        if (string.IsNullOrEmpty(vibratoParams.EffectName)) vibratoParams.EffectName = "vibrato";

        if (vibratoParams.Groups.Count == 0 || (vibratoParams.Groups.Count == 1 && vibratoParams.Groups[0] == ""))
        {
            int laneCount = map.MapInfo.KeyCount;
            for (int i = 1; i <= laneCount; i++) vibratoParams.Groups.Add($"${i}");
        }

        var effectName = vibratoParams.EffectName;
        var groups = vibratoParams.Groups;
        var startTime = vibratoParams.StartTime;
        var endTime = vibratoParams.EndTime;
        var startIntensity = vibratoParams.StartIntensity;
        var endIntensity = vibratoParams.EndIntensity;
        var downMultiplierStart = vibratoParams.DownMultiplierStart;
        var downMultiplierEnd = vibratoParams.DownMultiplierEnd;
        var upMultiplierStart = vibratoParams.UpMultiplierStart;
        var upMultiplierEnd = vibratoParams.UpMultiplierEnd;

        DefaultEasingFunction easingFunction = new DefaultEasingFunction(vibratoParams.Easing);
        DefaultEasingFunction upMultiplierEasingFunction = new DefaultEasingFunction(vibratoParams.UpMultiplierEasing);
        DefaultEasingFunction downMultiplierEasingFunction = new DefaultEasingFunction(vibratoParams.DownMultiplierEasing);

        addedVelocities = new List<AdditiveVelocity>();

        var hitObjectsInRange = new List<HitObject>();

        foreach (var hitObject in map.MapInfo.HitObjects)
        {
            string hitObjectGroup = string.IsNullOrEmpty(hitObject.Group) ? $"${hitObject.Lane}" : hitObject.Group;
            if (hitObject.Time >= startTime && hitObject.Time <= endTime && groups.Contains(hitObjectGroup)) // we might want to add everything regardless of groups sometimes? idk
                hitObjectsInRange.Add(hitObject);
        }

        double vibratoInterval = 1000.0 / vibratoParams.Frequency;

        // Collect all the raw vibrato times first
        var rawTimes = new List<double>();
        for (double t = startTime; t <= endTime; t += vibratoInterval)
            rawTimes.Add(t);

        // Snap times that are within vibrationLength ms of a hit object to that hit object's time
        var snappedTimes = rawTimes.Select(t =>
                                   {
                                       var nearby = hitObjectsInRange
                                                    .Where(h => Math.Abs(h.Time - t) < vibrationLength) // replace vibrationLength by 1.0 if something goes wrong
                                                    .OrderBy(h => Math.Abs(h.Time - t))
                                                    .FirstOrDefault();
                                       return nearby?.Time ?? t;
                                   })
                                   // .Distinct().OrderBy(t => t)
                                   .ToList();

        int sign = 1;
        double previousAvValue = 0;

        for (int i = 0; i < snappedTimes.Count; i++)
        {
            double t = snappedTimes[i];
            bool isFirst = i == 0;
            bool isLast = t >= endTime - vibratoInterval; // last peak before end

            // Lerp intensity based on position in range
            double progress = (t - startTime) / (endTime - startTime);
            double easedProgress = easingFunction.ApplyEasing(progress);
            double intensity = startIntensity + (endIntensity - startIntensity) * easedProgress;
            double avValue = sign * intensity * (1.0 / vibrationLength);

            //avValue *= avValue >= 0 ? downMultiplier : upMultiplier;
            if (avValue >= 0)
            {
                double multiplierEasedProgress = downMultiplierEasingFunction.ApplyEasing(progress);
                double multiplier = downMultiplierStart + (downMultiplierEnd - downMultiplierStart) * multiplierEasedProgress;
                avValue *= multiplier;
            }
            else
            {
                double multiplierEasedProgress = upMultiplierEasingFunction.ApplyEasing(progress);
                double multiplier = upMultiplierStart + (upMultiplierEnd - upMultiplierStart) * multiplierEasedProgress;
                avValue *= multiplier;
            }

            if (isFirst)
            {
                // t: +intensity, t+1: 0
                AddAV(map, t, avValue, effectName);
                AddAV(map, t + vibrationLength, 0, effectName);
            }
            else if (isLast)
            {
                // t-1: av_value (current sign), t: 0
                AddAV(map, t - vibrationLength, -previousAvValue, effectName);
                AddAV(map, t, 0, effectName);
            }
            else
            {
                // t-1: previous sign's value (to ramp in), t: av_value, t+1: 0
                AddAV(map, t - vibrationLength, -previousAvValue, effectName);
                AddAV(map, t, avValue, effectName);
                AddAV(map, t + vibrationLength, 0, effectName);
            }

            sign = -sign;
            previousAvValue = avValue;
        }

        double lastAvTime = addedVelocities.Last().Time;

        // add corrective AVs for notes that aren't on snapped times
        foreach (var hitObject in hitObjectsInRange)
        {
            if (snappedTimes.Contains(hitObject.Time) || hitObject.Time >= lastAvTime) continue;

            int previousAVindex = this.previousAV(hitObject.Time) - 1;
            if (previousAVindex < 0) continue;

            double t = hitObject.Time;
            double avValue = addedVelocities[previousAVindex].VelocityOffset;

            //NOTE: these adds the new corrective AVs at the end of the array, but this shouldn't be an issue (hopefully)
            AddAV(map, t - vibrationLength, -avValue, effectName + "_corr");
            AddAV(map, t, avValue, effectName + "_corr");
            AddAV(map, t + vibrationLength, 0, effectName + "_corr");
        }

        map.Add(toolLog = new ToolLog
        {
            Time = startTime,
            Label = $"Vibrato \"{effectName}\"",
            Color = Theme.AdditiveVelocity,
            ToolName = "orwenn22.vibrato",
            ToolSettings = JsonConvert.SerializeObject(vibratoParams),
            Effective = true,
        });
    }

    public override void Undo(EditorMap map)
    {
        if (toolLog != null)
        {
            map.Remove(toolLog);
            toolLog = null;
        }

        if (addedVelocities == null) return;

        foreach (var av in addedVelocities)
            map.Remove(av);

        addedVelocities.Clear();
    }

    private void AddAV(EditorMap map, double time, double velocityOffset, string effectName)
    {
        var av = new AdditiveVelocity
        {
            Time = time,
            EffectName = effectName,
            Groups = new List<string>(vibratoParams.Groups),
            VelocityOffset = velocityOffset,
        };
        map.Add(av);
        addedVelocities.Add(av);
    }

    private int previousAV(double time)
    {
        if (addedVelocities.Count == 0) return -1;

        int previous = 0;

        for (int i = 0; i < addedVelocities.Count; ++i)
        {
            if (addedVelocities[i].Time >= time) return previous;

            previous = i;
        }

        return previous;
    }

    public class VibratoParams
    {
        public VibratoParams()
        {
            EffectName = ""; // this should not be left empty
            StartTime = 0;
            EndTime = 0;
            Easing = Easing.None;
            StartIntensity = 10;
            EndIntensity = 10;
            Frequency = 120;
            UpMultiplierStart = 1;
            UpMultiplierEnd = 1;
            UpMultiplierEasing = Easing.None;
            DownMultiplierStart = 1;
            DownMultiplierEnd = 1;
            DownMultiplierEasing = Easing.None;
        }

        public VibratoParams(VibratoParams other)
        {
            EffectName = other.EffectName;
            Groups = (other.Groups == null) ? new List<string>() : new List<string>(other.Groups);
            Frequency = other.Frequency;
            StartTime = other.StartTime;
            EndTime = other.EndTime;
            StartIntensity = other.StartIntensity;
            EndIntensity = other.EndIntensity;
            Easing = other.Easing;
            UpMultiplierStart = other.UpMultiplierStart;
            UpMultiplierEnd = other.UpMultiplierEnd;
            UpMultiplierEasing = other.UpMultiplierEasing;
            DownMultiplierStart = other.DownMultiplierStart;
            DownMultiplierEnd = other.DownMultiplierEnd;
            DownMultiplierEasing = other.DownMultiplierEasing;
        }

        [JsonProperty("effect-name")]
        public string EffectName;

        [JsonProperty("groups")]
        public List<string> Groups;

        [JsonProperty("frequency")]
        public double Frequency;

        [JsonProperty("start-time")]
        public double StartTime;

        [JsonProperty("end-time")]
        public double EndTime;

        [JsonProperty("start-intensity")]
        public double StartIntensity;

        [JsonProperty("end-intensity")]
        public double EndIntensity;

        [JsonProperty("easing")]
        public Easing Easing;

        [JsonProperty("up-multiplier-start")]
        public double UpMultiplierStart;

        [JsonProperty("up-multiplier-end")]
        public double UpMultiplierEnd;

        [JsonProperty("up-multiplier-easing")]
        public Easing UpMultiplierEasing;

        [JsonProperty("down-multiplier-start")]
        public double DownMultiplierStart;

        [JsonProperty("down-multiplier-end")]
        public double DownMultiplierEnd;

        [JsonProperty("down-multiplier-easing")]
        public Easing DownMultiplierEasing;
    }
}
