using Newtonsoft.Json;

namespace CCE.Data
{
    public class Page
    {
        [JsonIgnore] public int ActualStartTick;
        [JsonProperty("end_tick")] public int EndTick;
        [JsonProperty("scan_line_direction")] public int ScanLineDirection;
        [JsonProperty("start_tick")] public int StartTick;
        [JsonIgnore] public double StartTime, EndTime, ActualStartTime;
        [JsonIgnore] public double PageSize => EndTick - StartTick;
        [JsonIgnore] public double ActualPageSize => EndTick - ActualStartTick;
    }
}