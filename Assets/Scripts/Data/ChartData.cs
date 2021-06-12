using System;
using System.Collections.Generic;
using Newtonsoft.Json;
// ReSharper disable RedundantDefaultMemberInitializer

namespace CCE.Data
{ 
    public class ChartData
    {
        [JsonProperty("format_version")] public int FormatVersion = 0;
        [JsonProperty("time_base")] public int TimeBase = 480;
        [JsonProperty("music_offset")] public double MusicOffset = 0;

        [JsonProperty("size")] public double Size = 1.0;
        [JsonProperty("opacity")] public double Opacity = 1.0;

        [JsonProperty("ring_color")] public string RingColor = null;
        [JsonProperty("fill_colors")] public List<string> FillColors =
            new List<string>(12) { null, null, null, null, null, null, null, null, null, null, null, null };
        
        [JsonProperty("display_boundaries")] public bool? DisplayBoundaries;
        [JsonProperty("display_background")] public bool? DisplayBackground;
        [JsonProperty("horizontal_margin")] public int? HorizontalMargin;
        [JsonProperty("vertical_margin")] public int? VerticalMargin;
        [JsonProperty("skip_music_on_completion")] public bool? SkipMusicONCompletion;

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

        [JsonProperty("page_list")] public List<Page> PageList = new List<Page>();
        [JsonProperty("tempo_list")] public List<Tempo> TempoList = new List<Tempo>();
        [JsonProperty("event_order_list")] public List<EventOrder> EventOrderList = new List<EventOrder>();
        [JsonProperty("note_list")] public List<Note> NoteList = new List<Note>();
    }
    
    public class Page
    {
        [JsonProperty("start_tick")] public int StartTick;
        [JsonProperty("end_tick")] public int EndTick;
        [JsonProperty("scan_line_direction")] public int ScanLineDirection;
        [JsonIgnore] public double StartTime, EndTime, ActualStartTime;
        [JsonIgnore] public int ActualStartTick;
        [JsonIgnore] public double PageSize => EndTick - StartTick;
        [JsonIgnore] public double ActualPageSize => EndTick - ActualStartTick;
    }
    
    public class Tempo
    {
        [JsonProperty("tick")] public int Tick;
        [JsonProperty("value")] public long Value;
        [JsonIgnore] public double Time;
    }

    public class EventOrder
    {
        [JsonProperty("tick")] public int Tick;
        [JsonProperty("event_list")] public List<Event> EventList = new List<Event>();

        public class Event
        {
            [JsonProperty("type")] public int Type;
            [JsonProperty("args")] public string Args;
        }
    }
}