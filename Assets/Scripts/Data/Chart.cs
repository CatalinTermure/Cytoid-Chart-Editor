/// <summary>
/// Class holding all the necessary information for a chart.
/// </summary>
public class Chart : ChartJSON
{
    [Newtonsoft.Json.JsonIgnore, System.NonSerialized]
    public LevelData.ChartData Data;

    /// <summary>
    /// Constructs a chart and does a member-wise copy of the parameters of <see cref="ChartJSON"/>.
    /// </summary>
    public Chart(ChartJSON chart, LevelData.ChartData data)
    {
        format_version = chart.format_version;
        time_base = chart.time_base;
        music_offset = chart.music_offset;

        ring_color = chart.ring_color;
        fill_colors = chart.fill_colors;

        page_list = chart.page_list;
        tempo_list = chart.tempo_list;
        note_list = chart.note_list;
        event_order_list = chart.event_order_list;

        Data = data;
    }
}