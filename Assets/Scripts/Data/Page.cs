using Newtonsoft.Json;

namespace CCE.Data
{
    public class Page
    {
        /// In Cytoid, the start tick of a page is actually the end tick of the previous page, not the serialized start tick.
        [JsonIgnore] public int ActualStartTick;

        [JsonProperty("start_tick")] public int StartTick;
        [JsonProperty("end_tick")] public int EndTick;

        /// Direction of the scan line for this page. 0 for down(top to bottom), 1 for up(bottom to top).
        [JsonProperty("scan_line_direction")] public int ScanLineDirection;

        /// StartTick converted to seconds.
        [JsonIgnore] public double StartTime;

        /// EndTick converted to seconds.
        [JsonIgnore] public double EndTime;

        /// <see cref="ActualStartTick"/> converted to seconds.
        [JsonIgnore] public double ActualStartTime;

        [JsonIgnore] public int PageSize => EndTick - StartTick;
        [JsonIgnore] public int ActualPageSize => EndTick - ActualStartTick;
    }
}