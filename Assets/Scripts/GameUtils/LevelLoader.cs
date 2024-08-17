using System.IO;
using CCE.Data;
using Newtonsoft.Json;

namespace CCE.GameUtils
{
    public static class LevelLoader
    {
        public static Chart LoadChart(string path, ChartMetadata metadata)
        {
            var chart = JsonConvert.DeserializeObject<Chart>(File.ReadAllText(path));
            chart.Metadata = metadata;
            return chart;
        }
    }
}