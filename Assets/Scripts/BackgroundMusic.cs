
using UnityEngine;

/// <summary>
/// Manages background music for a scene along with optional ambient audio.
/// Requires an AudioSource component on the same GameObject for the music track.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic instance;

    [Header("Music Settings")]
    [Tooltip("The audio clip to be played as background music.")]
    public AudioClip musicClip;

    [Tooltip("Play the music automatically when the scene starts.")]
    public bool playOnStart = true;

    [Tooltip("The volume of the music (0=silent, 1=full volume).")]
    [Range(0f, 1f)]
    public float volume = 0.5f;

    [Tooltip("Delay in seconds before the music starts playing.")]
    public float startDelay = 0f;

    [Header("Ambient Settings")]
    [Tooltip("Optional ambient loop that plays alongside the music.")]
    public AudioClip ambientClip;

    [Tooltip("Play the ambient audio automatically when the scene starts.")]
    public bool ambientPlayOnStart = true;

    [Tooltip("The volume of the ambient audio (0=silent, 1=full volume).")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.5f;

    private AudioSource musicSource;
    private AudioSource ambientSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Configure the primary AudioSource for music.
        musicSource = GetComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        ConfigureAmbientSource();
    }

    void Start()
    {
        if (playOnStart) PlayMusic();
        if (ambientPlayOnStart) PlayAmbient();
    }

    void Update()
    {
        // Allows for adjusting the volume in the inspector during runtime for easy testing.
        if (musicSource != null && musicSource.volume != volume)
        {
            musicSource.volume = volume;
        }

        if (ambientSource != null && ambientSource.volume != ambientVolume)
        {
            ambientSource.volume = ambientVolume;
        }
    }

    /// <summary>
    /// Starts playing the music after the specified delay.
    /// </summary>
    public void PlayMusic()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.PlayDelayed(startDelay);
        }
        else
        {
            Debug.LogWarning("BackgroundMusic: No music clip assigned.");
        }
    }

    /// <summary>
    /// Plays the ambient audio immediately if configured.
    /// </summary>
    public void PlayAmbient()
    {
        if (ambientSource == null)
        {
            ConfigureAmbientSource();
        }

        if (ambientSource != null && ambientSource.clip != null)
        {
            ambientSource.Play();
        }
    }

    /// <summary>
    /// Stops both music and ambient audio.
    /// </summary>
    public void Stop()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }

    /// <summary>
    /// Plays a sound effect as a one-shot on the music audio source.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    public void PlaySound(AudioClip clip)
    {
        if (musicSource == null)
        {
            Debug.LogWarning("BackgroundMusic: Music source missing, cannot play sound.");
            return;
        }

        musicSource.PlayOneShot(clip);
    }

    private void ConfigureAmbientSource()
    {
        if (ambientSource != null || ambientClip == null)
        {
            return;
        }

        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.clip = ambientClip;
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.volume = ambientVolume;
        ambientSource.spatialBlend = 0f;
    }

    /// <summary>
    /// Backwards compatibility helper to trigger both tracks.
    /// </summary>
    public void Play()
    {
        PlayMusic();
        PlayAmbient();
    }
}
