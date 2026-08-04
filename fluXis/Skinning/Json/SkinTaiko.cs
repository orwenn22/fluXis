using Newtonsoft.Json;

namespace fluXis.Skinning.Json;

public class SkinTaiko
{
    [JsonProperty("column_width")]
    public int ColumnWidth { get; set; } = 192;

    [JsonProperty("hit_position")]
    public int HitPosition { get; set; } = 0;
}
