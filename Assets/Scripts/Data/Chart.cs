using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace CCE.Data
{
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Newtonsoft.Json uses reflection for ShouldSerialize methods")]
    public class Chart
    {
        [JsonIgnore][NonSerialized] public ChartMetadata Metadata;

        [JsonProperty("display_background")] public bool? DisplayBackground;

        [JsonProperty("display_boundaries")] public bool? DisplayBoundaries;
        [JsonProperty("event_order_list")] public List<EventBatch> OrderedEventBatches = new();

        /// <summary>
        /// Fill color override for notes. The array goes like this:
        /// <list>
        ///   <item> 0 = Click down </item>
        ///   <item> 1 = Click up </item>
        ///   <item> 2 = Drag down </item>
        ///   <item> 3 = Drag up </item>
        ///   <item> 4 = Hold down </item>
        ///   <item> 5 = Hold up </item>
        ///   <item> 6 = Long hold down </item>
        ///   <item> 7 = Long hold up </item>
        ///   <item> 8 = Flick down </item>
        ///   <item> 9 = Flick up </item>
        ///   <item> 10 = CDrag down </item>
        ///   <item> 11 = CDrag up </item>
        /// </list>
        /// </summary>
        [JsonProperty("fill_colors")]
        public List<string> FillColors =
            new(12) { null, null, null, null, null, null, null, null, null, null, null, null };

        /// <summary>
        /// The default fill colors for notes.
        /// </summary>
        [JsonIgnore]
        public static readonly string[] DefaultFillColors =
        {
            "#35A7FF", "#FF5964", "#39E59E", "#39E59E", "#35A7FF", "#FF5964", "#F2C85A", "#F2C85A", "#35A7FF",
            "#FF5964", "#39E59E", "#39E59E"
        };

        /// <summary>
        /// Index into the FillColors array by the note type. This is for the color when the
        /// scanline is going down. To get the color of the note when the scanline is going up, add
        /// 1 to the value.
        /// </summary>
        public static readonly int[] ColorIndexByNoteType = { 0, 4, 6, 2, 2, 8, 10, 10 };

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
        /// <summary>
        /// Duration of a beat in ticks
        /// </summary>
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
}