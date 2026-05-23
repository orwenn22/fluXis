using System.Collections.Generic;
using System.ComponentModel;
using fluXis.Map.Structures.Bases;
using Newtonsoft.Json;
using osuTK.Graphics;

namespace fluXis.Map.Structures;

// intended to store some metadata to know which tool has been used at which timestemps and with which params/settings
// this way it is easier to replicate the effect at other parts of the map or re-apply it
public class ToolLog : ITimedObject
{
    [JsonProperty("time")]
    public double Time { get; set; }

    // hopefully loops dont actually mess with this
    [DefaultValue("")]
    [JsonProperty("group", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Group { get; set; }

    // used in the EditorTag corresponding to this ToolLog
    [JsonProperty("label")]
    public string Label { get; set; }

    // used in the EditorTag corresponding to this ToolLog
    [JsonProperty("color")]
    public Color4 Color { get; set; }

    // used to identify which tool was responsible for creating this ToolLong
    [JsonProperty("tool-name")]
    public string ToolName { get; set; }

    //should store a json-representation of the settings
    [JsonProperty("tool-settings", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string ToolSettings { get; set; }

    // this is used to find which SVs or other kind of events were added by the tool, and basically makes it easier to undo the change later on
    [JsonProperty("tag", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Tag { get; set; }

    // if set to false, it means all relevant objects are stored in the tool log and are not directly in the map
    [JsonProperty("effective")]
    public bool Effective { get; set; }

    // stores objects that were added by the tool if the log is not effective
    [JsonProperty("scroll-velocities")]
    public List<ScrollVelocity> ScrollVelocities { get; set; } = new();

    [JsonProperty("additive-velocities")]
    public List<AdditiveVelocity> AdditiveVelocities { get; set; } = new();

    //TODO: more objects? (also add them in EditorToggleToolLogAction and others)
}
