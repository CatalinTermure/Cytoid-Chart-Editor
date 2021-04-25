using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CCE.Data
{
    public class LevelData
    {
        [JsonProperty("artist")] public string Artist;
        [JsonProperty("artist_localized")] public string ArtistLocalized;

        [JsonProperty("artist_source")] public string ArtistSource;

        [JsonProperty("background")] public BackgroundData Background;

        [JsonProperty("charter")] public string Charter;

        [JsonProperty("charts")] public List<ChartFileData> Charts = new List<ChartFileData>();

        [JsonProperty("id")] public string ID;

        [JsonProperty("illustrator")] public string Illustrator;
        [JsonProperty("illustrator_source")] public string IllustratorSource;

        [JsonProperty("music")] public MusicData Music;
        [JsonProperty("music_preview")] public MusicData MusicPreview;
        [JsonProperty("schema_version")] public int SchemaVersion = 2;
        [JsonProperty("storyboarder")] public string Storyboarder;

        [JsonProperty("title")] public string Title;
        [JsonProperty("title_localized")] public string TitleLocalized;
        [JsonProperty("version")] public int Version = 1;

        [JsonIgnore] public string DisplayTitle => TitleLocalized?.Length > 0 ? TitleLocalized : Title;

        [JsonIgnore] public string DisplayArtist => ArtistLocalized?.Length > 0 ? ArtistLocalized : Artist;

        public bool ShouldSerializeTitleLocalized()
        {
            return !String.IsNullOrEmpty(TitleLocalized);
        }

        public bool ShouldSerializeLocalizedArtist()
        {
            return !String.IsNullOrEmpty(ArtistLocalized);
        }

        public bool ShouldSerializeStoryboarder()
        {
            return !String.IsNullOrEmpty(Storyboarder);
        }

        public class MusicData
        {
            [JsonProperty("path")] public string Path;
        }

        public class BackgroundData
        {
            [JsonProperty("path")] public string Path;
        }

        public class ChartFileData
        {
            [JsonProperty("difficulty")] public int Difficulty;
            [JsonProperty("music_override")] public MusicData MusicOverride;
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
}