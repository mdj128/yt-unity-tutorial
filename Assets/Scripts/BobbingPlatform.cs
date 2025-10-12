using UnityEngine;

/// <summary>
/// Applies a continuous bobbing motion (vertical sine wave) to a platform.
/// Useful for floating rocks or platforms to add timing challenges.
/// </summary>
public class BobbingPlatform : MonoBehaviour
{
    [SerializeField, Tooltip("Amplitude of the vertical bobbing motion (in meters).")]
    private float amplitude = 0.5f;

    [SerializeField, Tooltip("Speed of the bobbing motion (cycles per second).")]
    private float bobSpeed = 0.5f;

    [SerializeField, Tooltip("Phase offset (in radians) to desynchronize multiple platforms.")]
    private float phaseOffset = 0f;

    [SerializeField, Tooltip("If true, the bobbing uses unscaled time (ignores pause).")]
    private bool useUnscaledTime = false;

    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    private void OnEnable()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float offset = Mathf.Sin((time * bobSpeed * Mathf.PI * 2f) + phaseOffset) * amplitude;

        Vector3 newPosition = originalPosition;
        newPosition.y += offset;
        transform.position = newPosition;
    }

    private void OnDisable()
    {
        transform.position = originalPosition;
    }
}
