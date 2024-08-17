using Newtonsoft.Json;

namespace CCE.Data
{
    public class Tempo
    {
        [JsonProperty("tick")] public int Tick;
        [JsonIgnore] public double Time;
        [JsonProperty("value")] public long Value;
    }
}