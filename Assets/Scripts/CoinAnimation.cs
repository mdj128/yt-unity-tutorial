
using UnityEngine;

/// <summary>
/// Animates a GameObject to float and rotate, and handles being collected by a player.
/// </summary>
public class CoinAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The speed at which the coin rotates around its Z-axis.")]
    [SerializeField] private float rotationSpeed = 100.0f;

    [Tooltip("The speed of the up-and-down bobbing motion.")]
    [SerializeField] private float bobSpeed = 2.0f;

    [Tooltip("The maximum height the coin will bob up and down from its starting position.")]
    [SerializeField] private float bobHeight = 0.25f;

    [Header("Collection Settings")]
    [Tooltip("The tag of the GameObject that can collect this coin.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("A custom sound to play on collection. If left empty, the default sound from the counter will be used.")]
    [SerializeField] private AudioClip collectionSound;

    // The initial position of the coin, stored to calculate bobbing offset.
    private Vector3 startPosition;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void Start()
    {
        // Store the starting position of the coin to serve as the center of the bobbing motion.
        startPosition = transform.position;
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        // 1. Rotation:
        // Rotate the coin around its local forward axis (Z-axis) at a consistent speed.
        // Time.deltaTime ensures the rotation is smooth and independent of the frame rate.
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // 2. Bobbing:
        // Calculate the vertical offset using a sine wave for a smooth up-and-down motion.
        // Time.time provides a steadily increasing value to drive the wave.
        // The result of Sin() is multiplied by bobHeight to control the amplitude of the motion.
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        // Apply the bobbing offset to the coin's starting position.
        // This prevents any positional drift and keeps the bobbing centered.
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }

    /// <summary>
    /// Called when another collider enters this object's trigger zone.
    /// </summary>
    /// <param name="other">The collider of the object that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger has the specified player tag.
        if (other.CompareTag(playerTag))
        {
            CollectCoin();
        }
    }

    /// <summary>
    /// Handles the logic for collecting the coin.
    /// </summary>
    private void CollectCoin()
    {
        // For now, we'll just print a message and disable the coin.
        // You can add more complex behavior here, like playing a sound or adding to a score.
        Debug.Log("Coin collected!");

        // Use the singleton to add to the count, passing our custom sound.
        if (CollectableCounter.instance != null)
        {
            CollectableCounter.instance.AddToCount(collectionSound);
        }

        // TODO: Add sound effect for collection.
        // TODO: Add points to the player's score.

        // Deactivate the coin GameObject so it disappears from the scene.
        gameObject.SetActive(false);
    }
}
