using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public int schema_version = 2;
    public int version = 1;

    public string id;

    public string title;
    public string title_localized;

    public bool ShouldSerializetitle_localized()
    {
        return title_localized != null && title_localized != "";
    }

    public string artist;
    public string artist_localized;

    public bool ShouldSerializeartist_localized()
    {
        return artist_localized != null && artist_localized != "";
    }

    public string artist_source;

    public string illustrator;
    public string illustrator_source;

    public string charter;
    public string storyboarder;

    public bool ShouldSerializestoryboarder()
    {
        return storyboarder != null && storyboarder != "";
    }

    public MusicData music;
    public MusicData music_preview;

    public BackgroundData background;

    public List<ChartData> charts = new List<ChartData>();

    [Serializable]
    public class MusicData
    {
        public string path;
    }

    [Serializable]
    public class ChartData
    {
        public string type;
        public string name;
        public int difficulty;
        public string path;
        public MusicData music_override;
        public StoryboardData storyboard;

        public bool ShouldSerializemusic_override()
        {
            return music_override != null && music_override.path != null;
        }

        public bool ShouldSerializename()
        {
            return name != null;
        }

        public bool ShouldSerializestoryboard()
        {
            return storyboard != null && storyboard.path != null;
        }

        [Serializable]
        public class StoryboardData
        {
            public string path;
        }
    }

    [Serializable]
    public class BackgroundData
    {
        public string path;
    }
}