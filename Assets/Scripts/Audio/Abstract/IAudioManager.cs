namespace CCE.Audio.Abstract
{
    public interface IAudioManager
    {
        public bool IsInitialized { get; }
        public bool IsPlaying { get; }
        public double Time { get; set; }
        public double MaxTime { get; }
        /// <summary>
        /// Initializes the audio manager. Must be called before any other methods.
        /// </summary>
        public void Initialize();
        /// <summary>
        /// Plays the currently loaded hitsound.
        /// </summary>
        public void PlayHitsound();
        /// <summary>
        /// Loads an audio stream for playback. Must be called before playing.
        /// </summary>
        /// <param name="audioStream"> The audio stream to be loaded. Must be created by this <see cref="IAudioManager"/>. </param>
        /// <param name="loadForPlaybackSpeed"> If true, allows SetPlaybackSpeed to be called. </param>
        public void LoadAudio(IAudioStream audioStream, bool loadForPlaybackSpeed = false);
        /// <summary>
        /// Plays the currently loaded audio stream. Must be called after LoadAudio.
        /// </summary>
        /// <returns> The current time of the audio stream. </returns>
        public double Play();
        /// <summary>
        /// Pauses the currently playing audio stream, can be resumed with Play.
        /// </summary>
        public void Pause();
        /// <summary>
        /// Stops the currently playing audio stream. Cannot be resumed.
        /// </summary>
        public void Stop();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="looping"> </param>
        /// <returns> An audio stream to be used with this <see cref="IAudioManager"/> </returns>
        public IAudioStream CreateStream(string path, bool looping = false);
        /// <summary>
        /// Sets the playback speed of the audio stream. Must be called after LoadAudio with loadForPlaybackSpeed set to true.
        /// </summary>
        /// <param name="playbackSpeed"> The desired playback speed (e.g. 1 = normal, 2 = double speed, 0.5 = half speed) </param>
        void SetPlaybackSpeed(double playbackSpeed);
        /// <summary>
        /// Sets the volume of the audio.
        /// </summary>
        /// <param name="volume"> A value from 0 to 1 representing the volume of the audio, relative to the absolute volume of the loaded audio. </param>
        public void SetMusicVolume(float volume);
        /// <summary>
        /// Sets the volume of hitsounds.
        /// </summary>
        /// <param name="volume"> A value from 0 to 1 representing the volume of the hitsounds, relative to the absolute volume of the loaded hitsounds. </param>
        public void SetHitsoundVolume(float volume);
    }
}