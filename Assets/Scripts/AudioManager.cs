using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class responsible for managing and playing audio resources.
/// </summary>
public class AudioManager
{
    private AudioClip Music = null;
    private AudioSource MusicSource = null;

    private double lastStartTime = -1;
    private double timeBeforeStart = 0;

    public double PlaybackSpeed = 1;

    /// <summary>
    /// Gets the time point of the audio.
    /// </summary>
    public double Time
    {
        get => MusicSource.isPlaying ? (AudioSettings.dspTime - lastStartTime) * PlaybackSpeed + timeBeforeStart : timeBeforeStart;
    }

    public bool IsPlaying
    {
        get => MusicSource.isPlaying;
    }

    public double MaxTime
    {
        get => Music.length * PlaybackSpeed;
    }

    public void SetTime(double time)
    {
        timeBeforeStart = time;
    }

    public void SetSource(AudioSource source)
    {
        MusicSource = source;
        MusicSource.clip = Music;
    }

    /// <summary>
    /// Plays the <see cref="AudioClip"/> loaded into the <see cref="AudioManager"/>.
    /// </summary>
    public double Play()
    {
        if(Music == null)
        {
            return 0;
        }

        if(timeBeforeStart < 0)
        {
            MusicSource.time = 0;
            lastStartTime = AudioSettings.dspTime + 0.5 - timeBeforeStart;
            timeBeforeStart = 0;
            MusicSource.PlayScheduled(lastStartTime);
        }
        else
        {
            MusicSource.time = (float)timeBeforeStart;
            lastStartTime = AudioSettings.dspTime + 0.5;
            MusicSource.PlayScheduled(lastStartTime);
        }

        return lastStartTime;
    }

    public void Pause()
    {
        timeBeforeStart = Time;
        MusicSource.Stop();
    }

    /// <summary>
    /// Construct an <see cref="AudioManager"/> that plays from an <see cref="AudioSource"/> and loads a music file.
    /// </summary>
    /// <param name="source"> <see cref="AudioSource"/> from which to play the audio. </param>
    /// <param name="musicPath"> Path of the music file to load. </param>
    public AudioManager(string musicPath)
    {
        LoadAudio(musicPath);
    }

    /// <summary>
    /// Unloads the audio file loaded by <see cref="AudioManager"/>.
    /// </summary>
    public void UnloadAudio()
    {
        if(Music != null)
        {
            Music.UnloadAudioData();
            Object.Destroy(Music);
            Music = null;
        }
    }

    /// <summary>
    /// Loads audio from the file specified by path.
    /// </summary>
    /// <param name="path"> Path of the audio file to load. </param>
    public void LoadAudio(string path)
    {
        UnloadAudio();
        LoadMusicFromFile(path);
        Music.LoadAudioData();
    }

    private void LoadMusicFromFile(string path)
    {
        AudioType type = AudioType.UNKNOWN;
        switch (Path.GetExtension(path))
        {
            case ".ogg":
                type = AudioType.OGGVORBIS;
                break;
            case ".mp3":
                type = AudioType.MPEG;
                break;
            case ".wav":
                type = AudioType.WAV;
                break;
            default:
                Debug.LogError("CCELog: Audio file type is unsupported.");
                break;
        }

        #if CCE_DEBUG
        File.AppendAllText(Path.Combine(Application.persistentDataPath, "LoadChartLog.txt"), $"Loading music file from path: file://{path}\n");
        #endif

        using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, type))
        {
            var req = www.SendWebRequest();

            while(!req.isDone)
            {

            }

            if(www.isNetworkError || www.isHttpError)
            {
                Debug.LogError("CCELog: " + www.error);
            }
            else
            {
                Music = DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }
}
