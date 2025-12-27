using CCE.Utils;

namespace CCE.Data
{
    public class CurrentChartProvider : SingletonMonoBehaviour<CurrentChartProvider>
    {
        private Level _currentLevel;
        private Chart _currentChart;

        public static Level CurrentLevel { get => Instance._currentLevel; set => Instance._currentLevel = value; }
        public static Chart CurrentChart { get => Instance._currentChart; set => Instance._currentChart = value; }
    }
}