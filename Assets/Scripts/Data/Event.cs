using Newtonsoft.Json;

namespace CCE.Data
{
    public class Event
    {
        [JsonProperty("args")] public string Args;
        [JsonProperty("type")] public int Type;
    }
}