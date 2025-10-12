using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HazardZone : MonoBehaviour
{
    [Tooltip("If enabled, the collider will be forced into trigger mode on reset to ensure the player passes through it.")]
    [SerializeField] private bool forceTriggerCollider = true;

    [Header("Respawn Behaviour")]
    [Tooltip("If enabled, the player will sink before being respawned (useful for lava).")]
    [SerializeField] private bool sinkPlayerBeforeRespawn = false;

    [SerializeField, Tooltip("Distance the player sinks when sink behaviour is enabled.")]
    private float sinkDistance = 1.5f;

    [SerializeField, Tooltip("Speed at which the player sinks.")]
    private float sinkSpeed = 1f;

    [SerializeField, Tooltip("Additional delay after sinking before respawn.")]
    private float postSinkDelay = 0.5f;

    private Collider hazardCollider;

    private void Awake()
    {
        hazardCollider = GetComponent<Collider>();
        if (forceTriggerCollider && hazardCollider != null)
        {
            hazardCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryKillPlayer(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryKillPlayer(collision.collider);
    }

    private void TryKillPlayer(Collider other)
    {
        if (other == null)
        {
            return;
        }

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
        if (respawn == null)
        {
            respawn = other.GetComponentInParent<PlayerRespawn>();
        }

        if (respawn != null)
        {
            if (sinkPlayerBeforeRespawn)
            {
                respawn.SinkIntoLava(sinkDistance, sinkSpeed, postSinkDelay);
            }
            else
            {
                respawn.KillPlayer();
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (forceTriggerCollider)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
    }
#endif
}
