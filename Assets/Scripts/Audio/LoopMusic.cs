using UnityEngine;
using System.Collections.Generic;

public class LoopMusic : MonoBehaviour
{
    [System.Serializable]
    public class MusicTrack
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public string trackName = "Track";
    }

    public enum LoopMode
    {
        NoLoop,      // Play once and stop
        SingleLoop,  // Loop current track
        AllLoop      // Loop entire playlist
    }

    [Header("Playlist Settings")]
    [SerializeField] private List<MusicTrack> playlist = new List<MusicTrack>();
    [SerializeField] private bool shufflePlaylist = false;
    
    [Header("Loop Settings")]
    [SerializeField] private LoopMode loopMode = LoopMode.AllLoop;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool useFadeInOnStart = true;
    
    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private bool isFading = false;
    private float targetVolume = 1f;
    private float fadeElapsedTime = 0f;
    private List<int> shuffledIndices = new List<int>();

    public System.Action<MusicTrack> OnTrackChanged;
    public System.Action<int> OnPlaylistIndexChanged;

    void Start()
    {
        InitializeAudioSource();
        InitializePlaylist();
        
        if (playlist.Count > 0)
        {
            PlayTrack(0);
        }
    }

    void Update()
    {
        // Handle fade in/out
        if (isFading)
        {
            HandleFading();
        }

        // Check if current track finished playing
        if (isPlaying && audioSource != null && !audioSource.isPlaying && !isFading)
        {
            PlayNextTrack();
        }
    }

    private void InitializeAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = false; // We manage looping manually
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    private void InitializePlaylist()
    {
        if (shufflePlaylist && playlist.Count > 1)
        {
            ShufflePlaylist();
        }
    }

    public void PlayTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= playlist.Count)
        {
            Debug.LogWarning($"Invalid track index: {trackIndex}");
            return;
        }

        currentTrackIndex = trackIndex;
        MusicTrack track = playlist[currentTrackIndex];

        if (audioSource.isPlaying)
        {
            FadeOut(fadeDuration, () => PlayCurrentTrack());
        }
        else
        {
            PlayCurrentTrack();
        }

        OnPlaylistIndexChanged?.Invoke(currentTrackIndex);
    }

    private void PlayCurrentTrack()
    {
        if (currentTrackIndex >= 0 && currentTrackIndex < playlist.Count)
        {
            MusicTrack track = playlist[currentTrackIndex];
            audioSource.clip = track.clip;
            
            if (useFadeInOnStart)
            {
                audioSource.volume = 0f;
                audioSource.Play();
                FadeIn(fadeDuration, track.volume);
            }
            else
            {
                audioSource.volume = track.volume;
                audioSource.Play();
            }

            isPlaying = true;
            targetVolume = track.volume;
            OnTrackChanged?.Invoke(track);
        }
    }

    public void PlayNextTrack()
    {
        if (playlist.Count == 0)
            return;

        int nextIndex = GetNextTrackIndex();
        PlayTrack(nextIndex);
    }

    public void PlayPreviousTrack()
    {
        if (playlist.Count == 0)
            return;

        int previousIndex = GetPreviousTrackIndex();
        PlayTrack(previousIndex);
    }

    private int GetNextTrackIndex()
    {
        int nextIndex = currentTrackIndex + 1;

        if (shufflePlaylist)
        {
            nextIndex = GetNextShuffledIndex();
        }
        else
        {
            if (nextIndex >= playlist.Count)
            {
                nextIndex = loopMode == LoopMode.NoLoop ? -1 : 0;
            }
        }

        return nextIndex;
    }

    private int GetPreviousTrackIndex()
    {
        int previousIndex = currentTrackIndex - 1;

        if (shufflePlaylist)
        {
            previousIndex = GetPreviousShuffledIndex();
        }
        else
        {
            if (previousIndex < 0)
            {
                previousIndex = loopMode == LoopMode.NoLoop ? -1 : playlist.Count - 1;
            }
        }

        return previousIndex;
    }

    private int GetNextShuffledIndex()
    {
        int currentShuffledPosition = shuffledIndices.IndexOf(currentTrackIndex);
        int nextPosition = currentShuffledPosition + 1;

        if (nextPosition >= shuffledIndices.Count)
        {
            if (loopMode == LoopMode.NoLoop)
                return -1;

            // Re-shuffle for next loop
            ShufflePlaylist();
            nextPosition = 0;
        }

        return shuffledIndices[nextPosition];
    }

    private int GetPreviousShuffledIndex()
    {
        int currentShuffledPosition = shuffledIndices.IndexOf(currentTrackIndex);
        int previousPosition = currentShuffledPosition - 1;

        if (previousPosition < 0)
        {
            return loopMode == LoopMode.NoLoop ? -1 : shuffledIndices[shuffledIndices.Count - 1];
        }

        return shuffledIndices[previousPosition];
    }

    public void FadeIn(float duration, float targetVol)
    {
        targetVolume = targetVol;
        fadeElapsedTime = 0f;
        isFading = true;
    }

    public void FadeOut(float duration, System.Action onComplete = null)
    {
        targetVolume = 0f;
        fadeElapsedTime = 0f;
        isFading = true;
    }

    private void HandleFading()
    {
        fadeElapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(fadeElapsedTime / fadeDuration);

        if (fadeElapsedTime <= 0)
        {
            audioSource.volume = 0f;
        }
        else
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, progress);
        }

        if (progress >= 1f)
        {
            audioSource.volume = targetVolume;
            isFading = false;
        }
    }

    public void ShufflePlaylist()
    {
        shuffledIndices.Clear();
        for (int i = 0; i < playlist.Count; i++)
        {
            shuffledIndices.Add(i);
        }

        // Fisher-Yates shuffle
        for (int i = shuffledIndices.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = shuffledIndices[i];
            shuffledIndices[i] = shuffledIndices[randomIndex];
            shuffledIndices[randomIndex] = temp;
        }
    }

    public void Pause()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
        }
    }

    public void Resume()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            isPlaying = true;
        }
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            isPlaying = false;
        }
    }

    public void SetLoopMode(LoopMode mode)
    {
        loopMode = mode;
    }

    public void AddTrack(AudioClip clip, string trackName = "Track", float volume = 1f)
    {
        playlist.Add(new MusicTrack { clip = clip, trackName = trackName, volume = volume });
    }

    public void RemoveTrack(int index)
    {
        if (index >= 0 && index < playlist.Count)
        {
            playlist.RemoveAt(index);
        }
    }

    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume);
        if (!isFading)
        {
            audioSource.volume = targetVolume;
        }
    }

    public MusicTrack GetCurrentTrack()
    {
        if (currentTrackIndex >= 0 && currentTrackIndex < playlist.Count)
            return playlist[currentTrackIndex];
        return null;
    }

    public int GetCurrentTrackIndex()
    {
        return currentTrackIndex;
    }

    public int GetPlaylistSize()
    {
        return playlist.Count;
    }

    public bool IsPlaying()
    {
        return isPlaying && audioSource != null && audioSource.isPlaying;
    }
}

