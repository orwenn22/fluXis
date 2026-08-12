using System.Collections.Generic;
using fluXis.Graphics.UserInterface.Color;
using fluXis.Map.Structures;
using fluXis.Utils;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace fluXis.Screens.Edit.Actions.Sv;

public class EditorHitPosOffestAction : EditorAction
{
    public override string Description => "Add HitPosOffset";

    private double vibrationLength = 0.05;

    private HitPosOffsetParams hitPosOffsetParams;

    private List<AdditiveVelocity> addedVelocities;
    private ToolLog toolLog;

    public EditorHitPosOffestAction(HitPosOffsetParams hitPosOffsetParams)
    {
        this.hitPosOffsetParams = hitPosOffsetParams;
        addedVelocities = new List<AdditiveVelocity>();
    }

    public override void Run(EditorMap map)
    {
        addedVelocities.Clear();

        // maybe these checks shouldn't be done here?
        // TODO: check if there are other effects called "hitposoffset" in the time range, and append some number if there is
        if (string.IsNullOrEmpty(hitPosOffsetParams.EffectName)) hitPosOffsetParams.EffectName = "hitposoffset";

        if (hitPosOffsetParams.Groups.Count == 0 || (hitPosOffsetParams.Groups.Count == 1 && hitPosOffsetParams.Groups[0] == ""))
        {
            hitPosOffsetParams.Groups.Clear(); // in case there is an empty string
            int laneCount = map.MapInfo.KeyCount;
            for (int i = 1; i <= laneCount; i++) hitPosOffsetParams.Groups.Add($"${i}");
        }

        var startTime = hitPosOffsetParams.StartTime;
        var endTime = hitPosOffsetParams.EndTime;
        var groups = hitPosOffsetParams.Groups;
        var startIntensity = hitPosOffsetParams.StartIntensity;
        var endIntensity = hitPosOffsetParams.EndIntensity;
        var effectName = hitPosOffsetParams.EffectName;
        DefaultEasingFunction easingFunction = new DefaultEasingFunction(hitPosOffsetParams.Easing);

        var hitObjectsTimes = new List<double>();
        string tag = MapUtils.GetHash(JsonConvert.SerializeObject(hitPosOffsetParams));

        foreach (var hitObject in map.MapInfo.HitObjects)
        {
            string hitObjectGroup = string.IsNullOrEmpty(hitObject.Group) ? $"${hitObject.Lane}" : hitObject.Group;
            if (hitObject.Time >= startTime && hitObject.Time <= endTime && groups.Contains(hitObjectGroup)) // we might want to add everything regardless of groups sometimes? idk
                hitObjectsTimes.Add(hitObject.Time);

            if (hitObject.Type == HitObjectType.Normal && hitObject.EndTime >= startTime && hitObject.EndTime <= endTime && groups.Contains(hitObjectGroup))
                hitObjectsTimes.Add(hitObject.EndTime);
        }

        foreach (var t in hitObjectsTimes)
        {
            double progress = (t - startTime) / (endTime - startTime);
            double easedProgress = easingFunction.ApplyEasing(progress);
            double intensity = startIntensity + (endIntensity - startIntensity) * easedProgress;

            double avValue = intensity * (1.0 / vibrationLength);

            AddAV(map, t - vibrationLength, avValue, effectName, tag);
            AddAV(map, t, -avValue, effectName, tag);
            AddAV(map, t + vibrationLength, 0, effectName, tag);
        }

        map.Add(toolLog = new ToolLog
        {
            Time = startTime,
            Label = $"hitpos offset \"{effectName}\"",
            Color = Theme.TimeOffset,
            ToolName = "orwenn22.hitposoffset",
            ToolSettings = JsonConvert.SerializeObject(hitPosOffsetParams),
            Tag = tag,
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

    private void AddAV(EditorMap map, double time, double velocityOffset, string effectName, string tag)
    {
        var av = new AdditiveVelocity
        {
            Time = time,
            EffectName = effectName,
            Groups = new List<string>(hitPosOffsetParams.Groups),
            VelocityOffset = velocityOffset,
            Tag = tag
        };
        map.Add(av);
        addedVelocities.Add(av);
    }

    public class HitPosOffsetParams
    {
        public HitPosOffsetParams()
        {
            EffectName = ""; // this should not be left empty
            Groups = new List<string>();
            StartTime = 0;
            EndTime = 0;
            Easing = Easing.None;
            StartIntensity = 10;
            EndIntensity = 10;
        }

        public HitPosOffsetParams(HitPosOffsetParams other)
        {
            EffectName = other.EffectName;
            Groups = (other.Groups == null) ? new List<string>() : new List<string>(other.Groups);
            StartTime = other.StartTime;
            EndTime = other.EndTime;
            StartIntensity = other.StartIntensity;
            EndIntensity = other.EndIntensity;
            Easing = other.Easing;
        }

        [JsonProperty("effect-name")]
        public string EffectName;

        [JsonProperty("groups")]
        public List<string> Groups;

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
    }
}
