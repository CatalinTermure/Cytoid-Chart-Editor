using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace CCE.Data
{
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Newtonsoft.Json uses reflection for ShouldSerialize methods")]
    public class ChartMetadata
    {
        [JsonProperty("difficulty")] public int Difficulty;
        [JsonProperty("music_override")] public LevelData.MusicData MusicOverride;
        [JsonProperty("name")] public string Name;
        [JsonProperty("path")] public string Path;
        [JsonProperty("storyboard")] public StoryboardData Storyboard;
        [JsonProperty("type")] public string Type;

        [JsonIgnore] public string DisplayName => Name?.Length > 0 ? Name : Type;

        public bool ShouldSerializeMusicOverride()
        {
            return !String.IsNullOrEmpty(MusicOverride?.Path);
        }

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }

        public bool ShouldSerializeStoryboard()
        {
            return !String.IsNullOrEmpty(Storyboard?.Path);
        }

        public class StoryboardData
        {
            [JsonProperty("path")] public string Path;
        }
    }
}