using Newtonsoft.Json;

namespace CCE.Data
{
    public class Tempo
    {
        [JsonProperty("tick")] public int Tick;
        [JsonIgnore] public double Time;
        /// <summary>
        /// Duration of a beat in microseconds.
        /// </summary>
        [JsonProperty("value")] public long Value;

        public static long ComputeValueFromBpm(double bpm)
        {
            if (bpm == 0)
            {
                return 1;
            }

            return (long)(120000000 / bpm);
        }
    }
}