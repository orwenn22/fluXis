using System;
using System.Collections.Generic;
using System.ComponentModel;
using fluXis.Map.Structures.Bases;
using fluXis.Screens.Gameplay.Ruleset;
using Newtonsoft.Json;

namespace fluXis.Map.Structures;

public class AdditiveVelocity : ITimedObject, IHasGroups, IHasTag
{
    [JsonProperty("time")]
    public double Time { get; set; }

    [JsonProperty("group", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Group { get; set; }

    [DefaultValue("")]
    [JsonProperty("tag", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Tag { get; set; } = "";

    [JsonProperty("velocity_offset")]
    public double VelocityOffset { get; set; } = 0;

    [JsonProperty("groups", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public List<string> Groups { get; set; } = new();

    [JsonProperty("mask")]
    [Obsolete($"Use {nameof(AdditiveVelocity)}.{nameof(Groups)} instead.")]
    public List<bool> LaneMask
    {
        set
        {
            for (var i = 0; i < value.Count; i++)
            {
                if (value[i])
                    Groups.Add($"${i + 1}");
            }
        }
    }

    [JsonProperty("effect_name")]
    public string EffectName { get; set; } = "";

    public void Apply(ScrollGroup group) => group.AddAdditiveVelocity(this);
}
