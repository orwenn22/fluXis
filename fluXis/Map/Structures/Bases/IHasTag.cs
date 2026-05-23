using System.ComponentModel;
using Newtonsoft.Json;

namespace fluXis.Map.Structures.Bases;

public interface IHasTag
{
    [DefaultValue("")]
    [JsonProperty("tag", DefaultValueHandling = DefaultValueHandling.Ignore)]
    string Tag { get; set; }
}
