using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace CCE.Data
{
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Newtonsoft.Json uses reflection for ShouldSerialize methods")]
    public class Note
    {
        [JsonProperty("approach_rate")] public double ApproachRate = 1.0;
        [JsonProperty("fill_color")] public string FillColor;
        [JsonProperty("hold_tick")] public int HoldTick;
        [JsonProperty("id")] public int ID;
        [JsonProperty("next_id")] public int NextID;
        [JsonProperty("opacity")] public double Opacity = -1;
        [JsonProperty("page_index")] public int PageIndex;
        [JsonProperty("ring_color")] public string RingColor;
        [JsonProperty("size")] public double Size = -1;
        [JsonProperty("tick")] public int Tick;
        [JsonProperty("type")] public int Type;
        [JsonProperty("x")] public double X;
        [JsonIgnore] public double ApproachTime;
        [JsonIgnore] public double ActualOpacity = 1.0;
        [JsonIgnore] public double ActualSize = 1.0;
        [JsonIgnore] public int DragChainID = -1;
        [JsonIgnore] public double HoldTime;
        [JsonIgnore] public double Time;
        [JsonIgnore] public double Y;

        public Note()
        {
        }

        public Note(Note other)
        {
            PageIndex = other.PageIndex;
            Type = other.Type;
            ID = other.ID;
            Tick = other.Tick;
            X = other.X;
            HoldTick = other.HoldTick;
            NextID = other.NextID;
            ApproachRate = other.ApproachRate;
            Size = other.Size;
            RingColor = other.RingColor;
            FillColor = other.FillColor;
            Opacity = other.Opacity;
            ActualOpacity = other.ActualOpacity;
            ActualSize = other.ActualSize;
            Time = other.Time;
            HoldTime = other.HoldTime;
            ApproachTime = other.ApproachTime;
            Y = other.Y;
        }

        public bool ShouldSerializeOpacity()
        {
            return !(Opacity < 0);
        }

        public bool ShouldSerializeSize()
        {
            return !(Size < 0);
        }

        public bool ShouldSerializeApproachRate()
        {
            return Math.Abs(ApproachRate - 1.0) > 0.001;
        }
    }
}