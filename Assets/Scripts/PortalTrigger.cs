
using UnityEngine;

/// <summary>
/// This script should be attached to a portal GameObject with a trigger collider.
/// When an object with the "Player" tag enters the trigger, it calls the level completion logic.
/// </summary>
public class PortalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the player.
        if (other.CompareTag("Player"))
        {
            // Find the MenuManager in the scene.
            MenuManager menuManager = FindObjectOfType<MenuManager>();

            // If the MenuManager is found, call the PlayerReachedPortal method.
            if (menuManager != null)
            {
                menuManager.PlayerReachedPortal();
            }
            else
            {
                Debug.LogError("PortalTrigger could not find a MenuManager in the scene!");
            }
        }
    }
}
