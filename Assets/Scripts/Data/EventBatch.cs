using System.Collections.Generic;
using Newtonsoft.Json;

namespace CCE.Data
{
    public class EventBatch
    {
        [JsonProperty("event_list")] public List<Event> EventList = new();
        [JsonProperty("tick")] public int Tick;
    }
}