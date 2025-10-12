using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes a floating platform sink when players stand on it and rise back up once they leave.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SinkingPlatform : MonoBehaviour
{
    [Header("Sink Settings")]
    [SerializeField, Tooltip("Distance (meters) the platform sinks when fully loaded.")]
    private float sinkDistance = 0.75f;
    [SerializeField, Tooltip("Seconds to move from rest to fully sunk (and vice versa).")]
    private float sinkDuration = 1.5f;

    [Header("Detection")]
    [SerializeField, Tooltip("Layers counted as players standing on the platform.")]
    private LayerMask playerLayers = ~0;
    [SerializeField, Tooltip("Optional override for the detection collider. Defaults to the collider on this GameObject.")]
    private Collider detectionCollider;

    private readonly HashSet<Collider> trackedColliders = new HashSet<Collider>();
    private readonly HashSet<PlayerRespawn> trackedPlayers = new HashSet<PlayerRespawn>();

    private Transform cachedTransform;
    private Vector3 initialPosition;
    private Vector3 targetOffset;
    private Vector3 sinkVelocity;

    private void Awake()
    {
        cachedTransform = transform;
        initialPosition = cachedTransform.position;
        targetOffset = Vector3.zero;

        detectionCollider = detectionCollider != null ? detectionCollider : GetComponent<Collider>();
        if (detectionCollider != null)
        {
            detectionCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        Vector3 desiredPosition = initialPosition + targetOffset;
        cachedTransform.position = Vector3.SmoothDamp(cachedTransform.position, desiredPosition, ref sinkVelocity, Mathf.Max(0.01f, sinkDuration), Mathf.Infinity, Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTrackPlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryTrackPlayer(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (trackedColliders.Remove(other))
        {
            PlayerRespawn player = GetPlayerFromCollider(other);
            if (player != null)
            {
                trackedPlayers.Remove(player);
                UpdateTargetOffset();
            }
        }
    }

    private void TryTrackPlayer(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayers) == 0 || trackedColliders.Contains(other))
        {
            return;
        }

        PlayerRespawn player = GetPlayerFromCollider(other);
        if (player != null)
        {
            trackedColliders.Add(other);
            trackedPlayers.Add(player);
            UpdateTargetOffset();
        }
    }

    private void UpdateTargetOffset()
    {
        bool shouldSink = trackedPlayers.Count > 0;
        targetOffset = shouldSink ? Vector3.down * sinkDistance : Vector3.zero;
    }

    private PlayerRespawn GetPlayerFromCollider(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        PlayerRespawn player = other.GetComponent<PlayerRespawn>();
        if (player == null)
        {
            player = other.GetComponentInParent<PlayerRespawn>();
        }

        return player;
    }

    private void OnDisable()
    {
        trackedColliders.Clear();
        trackedPlayers.Clear();
        targetOffset = Vector3.zero;
        sinkVelocity = Vector3.zero;
        cachedTransform.position = initialPosition;
    }
}
