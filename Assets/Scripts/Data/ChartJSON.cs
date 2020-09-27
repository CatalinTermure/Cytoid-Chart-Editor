using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChartJSON
{
    public int format_version = 0;
    public int time_base = 480;
    public int start_offset_time = 0;
    public double music_offset = 0;

    public double size = 1.0;
    public double opacity = 1.0;

    public bool ShouldSerializesize()
    {
        return Math.Abs(size - 1.0) > 0.001;
    }

    public bool ShouldSerializeopacity()
    {
        return Math.Abs(opacity - 1.0) > 0.001;
    }

    public string ring_color = null;
    public List<string> fill_colors = new List<string>(12) { null, null, null, null, null, null, null, null, null, null, null, null };

    public bool ShouldSerializefill_colors()
    {
        return !fill_colors.TrueForAll((string s) => s == null);
    }

    public List<Page> page_list = new List<Page>();
    public List<Tempo> tempo_list = new List<Tempo>();
    public List<EventOrder> event_order_list = new List<EventOrder>();
    public List<Note> note_list = new List<Note>();
}

[Serializable]
public class Page
{
    public int start_tick;
    public int end_tick;
    public int scan_line_direction;
    [NonSerialized, JsonIgnore]
    public double start_time, end_time, actual_start_time;
    [NonSerialized, JsonIgnore]
    public int actual_start_tick;
    [JsonIgnore]
    public double PageSize
    {
        get => end_tick - start_tick;
    }
    [JsonIgnore]
    public double ActualPageSize
    {
        get => end_tick - actual_start_tick;
    }
}

[Serializable]
public class Tempo
{
    public int tick;
    public int value;
    [NonSerialized, JsonIgnore]
    public double time;
}

[Serializable]
public class EventOrder
{
    public int tick;
    public List<Event> event_list = new List<Event>();

    [Serializable]
    public class Event
    {
        public int type;
        public string args;
    }
}

[Serializable]
public class Note
{
    public int page_index;
    public int type;
    public int id;
    public int tick;
    public double x;
    public int hold_tick;
    public int next_id;
    public double approach_rate = 1.0;
    public double size = -1;
    public string ring_color = null;
    public string fill_color = null;
    public double opacity = -1;

    [NonSerialized, JsonIgnore]
    public double actual_opacity = 1.0, actual_size = 1.0;
    [NonSerialized, JsonIgnore]
    public double time, hold_time, approach_time, y;

    public Note() { }

    public Note(Note other)
    {
        page_index = other.page_index;
        type = other.type;
        id = other.id;
        tick = other.tick;
        x = other.x;
        hold_tick = other.hold_tick;
        next_id = other.next_id;
        approach_rate = other.approach_rate;
        size = other.size;
        ring_color = other.ring_color;
        fill_color = other.fill_color;
        opacity = other.opacity;
        actual_opacity = other.actual_opacity;
        actual_size = other.actual_size;
        time = other.time;
        hold_time = other.hold_time;
        approach_time = other.approach_time;
        y = other.y;
    }

    public bool ShouldSerializeopacity()
    {
        return !(opacity < 0);
    }

    public bool ShouldSerializesize()
    {
        return !(size < 0);
    }

    public bool ShouldSerializeapproach_rate()
    {
        return Math.Abs(approach_rate - 1.0) > 0.001;
    }
}