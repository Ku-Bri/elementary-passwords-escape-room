using UnityEngine;

[DisallowMultipleComponent]
public class MusicSingleton : MonoBehaviour
{
    public static MusicSingleton Instance { get; private set; }

    [Header("Assign in Inspector (optional)")]
    [SerializeField] private AudioClip defaultClip;

    [Header("AudioSource settings")]
    [Range(0f, 1f)] public float volume = 0.6f;
    [Range(-3f, 3f)] public float pitch = 1f;
    public bool playOnStart = true;

    private AudioSource _src;

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);               // ensure only one lives
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);         // persist across scenes

        // Ensure we have an AudioSource configured for looping
        _src = GetComponent<AudioSource>();
        if (_src == null) _src = gameObject.AddComponent<AudioSource>();
        _src.loop = true;
        _src.playOnAwake = false;

        // Apply user settings
        _src.volume = volume;
        _src.pitch = pitch;

        // Optionally start right away if a default clip is set
        if (playOnStart && defaultClip != null)
        {
            Play(defaultClip, loop: true);
        }
    }

    /// <summary>
    /// Play a clip (optionally looping). If a clip is already playing and it's
    /// the same clip, this does nothing.
    /// </summary>
    public void Play(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        // If already playing this exact clip, keep going
        if (_src.isPlaying && _src.clip == clip && _src.loop == loop) return;

        _src.clip = clip;
        _src.loop = loop;
        _src.volume = volume; // ensure latest volume is used
        _src.pitch = pitch;
        _src.Play();
    }

    /// <summary>Stop playback (keeps the clip reference).</summary>
    public void Stop() => _src.Stop();

    /// <summary>Pause playback.</summary>
    public void Pause() => _src.Pause();

    /// <summary>Resume if paused.</summary>
    public void Resume()
    {
        if (_src.clip != null && !_src.isPlaying)
            _src.UnPause();
    }

    /// <summary>Change volume at runtime (0..1).</summary>
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        _src.volume = volume;
    }

    /// <summary>Swap clip and start playing (loops by default).</summary>
    public void SetClipAndPlay(AudioClip newClip, bool loop = true)
        => Play(newClip, loop);

    // Optional: make sure inspector changes apply while running
    private void OnValidate()
    {
        if (_src == null) _src = GetComponent<AudioSource>();
        if (_src != null)
        {
            _src.volume = Mathf.Clamp01(volume);
            _src.pitch = Mathf.Clamp(pitch, -3f, 3f);
        }
    }
}