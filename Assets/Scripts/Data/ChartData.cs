using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace CCE.Data
{
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Newtonsoft.Json uses reflection for ShouldSerialize methods")]
    public class ChartData
    {
        [JsonProperty("display_background")] public bool? DisplayBackground;

        [JsonProperty("display_boundaries")] public bool? DisplayBoundaries;
        [JsonProperty("event_order_list")] public List<EventOrder> EventOrderList = new();

        [JsonProperty("fill_colors")] public List<string> FillColors =
            new(12) { null, null, null, null, null, null, null, null, null, null, null, null };

        [JsonProperty("format_version")] public int FormatVersion;
        [JsonProperty("horizontal_margin")] public int? HorizontalMargin;
        [JsonProperty("music_offset")] public double MusicOffset;
        [JsonProperty("note_list")] public List<Note> NoteList = new();
        [JsonProperty("opacity")] public double Opacity = 1.0;

        [JsonProperty("page_list")] public List<Page> PageList = new();

        [JsonProperty("ring_color")] public string RingColor;

        [JsonProperty("size")] public double Size = 1.0;

        [JsonProperty("skip_music_on_completion")]
        public bool? SkipMusicOnCompletion;

        [JsonProperty("tempo_list")] public List<Tempo> TempoList = new();
        [JsonProperty("time_base")] public int TimeBase = 480;
        [JsonProperty("vertical_margin")] public int? VerticalMargin;

        public bool ShouldSerializeSize()
        {
            return Math.Abs(Size - 1.0) > 0.001;
        }

        public bool ShouldSerializeOpacity()
        {
            return Math.Abs(Opacity - 1.0) > 0.001;
        }

        public bool ShouldSerializeRingColor()
        {
            return !String.IsNullOrEmpty(RingColor);
        }

        public bool ShouldSerializeFillColors()
        {
            return !FillColors.TrueForAll(String.IsNullOrEmpty);
        }
    }

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

    public class Tempo
    {
        [JsonProperty("tick")] public int Tick;
        [JsonIgnore] public double Time;
        [JsonProperty("value")] public long Value;
    }

    public class EventOrder
    {
        [JsonProperty("event_list")] public List<Event> EventList = new();
        [JsonProperty("tick")] public int Tick;

        public class Event
        {
            [JsonProperty("args")] public string Args;
            [JsonProperty("type")] public int Type;
        }
    }
}