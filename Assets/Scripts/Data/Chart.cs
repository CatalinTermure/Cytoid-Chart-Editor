namespace CCE.Data
{
    /// <summary>
    /// Class holding all the necessary information for a chart.
    /// </summary>
    public class Chart : ChartData
    {
        [Newtonsoft.Json.JsonIgnore, System.NonSerialized]
        public LevelData.ChartFileData Data;

        /// <summary>
        /// Constructs a chart and does a member-wise copy of the parameters of <see cref="ChartData"/>.
        /// </summary>
        public Chart(ChartData chart, LevelData.ChartFileData data)
        {
            FormatVersion = chart.FormatVersion;
            TimeBase = chart.TimeBase;
            MusicOffset = chart.MusicOffset;

            Opacity = chart.Opacity;
            Size = chart.Size;

            RingColor = chart.RingColor;
            FillColors = chart.FillColors;

            PageList = chart.PageList;
            TempoList = chart.TempoList;
            NoteList = chart.NoteList;
            EventOrderList = chart.EventOrderList;

            Data = data;
        }
    }
}