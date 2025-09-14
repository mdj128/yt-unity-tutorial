
using UnityEngine;

/// <summary>
/// Manages background music for a scene.
/// Requires an AudioSource component on the same GameObject.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    /// <summary>
    /// The audio clip to be played as background music.
    /// </summary>
    [Tooltip("The audio clip to be played as background music.")]
    public AudioClip musicClip;

    /// <summary>
    /// If checked, the music will start playing automatically when the scene loads.
    /// </summary>
    [Tooltip("Play the music automatically when the scene starts.")]
    public bool playOnStart = true;

    /// <summary>
    /// The volume of the music. 0 is silent, 1 is full volume.
    /// </summary>
    [Tooltip("The volume of the music (0=silent, 1=full volume).")]
    [Range(0f, 1f)]
    public float volume = 0.5f;

    /// <summary>
    /// The delay in seconds before the music starts playing.
    /// </summary>
    [Tooltip("Delay in seconds before the music starts playing.")]
    public float startDelay = 0f;

    private AudioSource audioSource;

    void Awake()
    {
        // Get the AudioSource component attached to this GameObject.
        audioSource = GetComponent<AudioSource>();

        // Configure the AudioSource
        audioSource.clip = musicClip;
        audioSource.volume = volume;
        audioSource.loop = true; // Background music should typically loop.
        audioSource.playOnAwake = true; // We will control playback manually.
    }

    void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    void Update()
    {
        // Allows for adjusting the volume in the inspector during runtime for easy testing.
        if (audioSource.volume != volume)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Starts playing the music after the specified delay.
    /// </summary>
    public void Play()
    {
        if (audioSource.clip != null)
        {
            audioSource.PlayDelayed(startDelay);
        }
        else
        {
            Debug.LogWarning("BackgroundMusic: No music clip assigned.");
        }
    }

    /// <summary>
    /// Stops the music.
    /// </summary>
    public void Stop()
    {
        audioSource.Stop();
    }
}
