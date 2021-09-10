using System;
using System.Collections.Generic;
using CCE.UI;
using Newtonsoft.Json;

namespace CCE.Data
{
    public class LevelData
    {
        [JsonProperty("artist")]
        [Displayable(Section = "Artist", Name = "Artist:", Filter = "Existing Level")]
        public string Artist;
        
        [JsonProperty("artist_localized")]
        [Displayable(Section = "Artist", Name = "Localized artist:", Filter = "Existing Level")]
        public string ArtistLocalized;

        [JsonProperty("artist_source")]
        [Displayable(Section = "Artist", Name = "Artist source:", Filter = "Existing Level")]
        public string ArtistSource;

        [JsonProperty("background")]
        [Displayable(Section = "Illustrator", Name = "Background:", Filter = "Existing Level")]
        public BackgroundData Background;

        [JsonProperty("charter")]
        [Displayable(Section = "Creator", Name = "Charter:", Filter = "Existing Level")]
        public string Charter;

        [JsonProperty("charts")] public List<ChartFileData> Charts = new List<ChartFileData>();

        [JsonProperty("id")]
        [Displayable(Name = "Level ID:")]
        public string ID;

        [JsonProperty("illustrator")]
        [Displayable(Section = "Illustrator", Name = "Illustrator:", Filter = "Existing Level")]
        public string Illustrator;
        
        [JsonProperty("illustrator_source")]
        [Displayable(Section = "Illustrator", Name = "Illustrator source:", Filter = "Existing Level")]
        public string IllustratorSource;

        [JsonProperty("music")] public MusicData Music;
        [JsonProperty("music_preview")] public MusicData MusicPreview;
        [JsonProperty("schema_version")] public int SchemaVersion = 2;
        
        [JsonProperty("storyboarder")]
        [Displayable(Section = "Creator", Name = "Storyboarder:", Filter = "Existing Level")]
        public string Storyboarder;

        [JsonProperty("title")]
        [Displayable(Name = "Song title:", Filter = "Existing Level")]
        public string Title;
        
        [JsonProperty("title_localized")]
        [Displayable(Name = "Localized title:", Filter = "Existing Level")]
        public string TitleLocalized;
        
        [JsonProperty("version")] public int Version = 1;

        [JsonIgnore]
        public string DisplayTitle => !String.IsNullOrWhiteSpace(TitleLocalized) ? TitleLocalized : Title;

        [JsonIgnore]
        public string DisplayArtist =>!String.IsNullOrWhiteSpace(ArtistLocalized) ? ArtistLocalized : Artist;

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