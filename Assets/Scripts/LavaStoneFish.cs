using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUPERCharacter;

/// <summary>
/// Controls a lava-dwelling stone fish that bursts from the lava when the player is nearby.
/// Attach this component to the fish root transform. The fish should have a trigger collider
/// that covers the aggro range for detecting players.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LavaStoneFish : MonoBehaviour
{
    [Header("Player Interaction")]
    [SerializeField, Tooltip("Layers considered valid player targets when applying knockback.")]
    private LayerMask playerLayers = ~0;

    [Header("Jump Timing")]
    [SerializeField, Tooltip("Minimum delay (seconds) between jumps.")]
    private float minJumpInterval = 2f;
    [SerializeField, Tooltip("Maximum delay (seconds) between jumps.")]
    private float maxJumpInterval = 4f;

    [Header("Jump Motion")]
    [SerializeField, Tooltip("Height in meters the fish will travel above its start position.")]
    private float jumpHeight = 3.5f;
    [SerializeField, Tooltip("Seconds spent travelling upward.")]
    private float ascentDuration = 0.6f;
    [SerializeField, Tooltip("Optional pause at the top of the leap.")]
    private float apexHangTime = 0.15f;
    [SerializeField, Tooltip("Seconds spent travelling back down.")]
    private float descentDuration = 0.5f;
    [SerializeField, Tooltip("Optional offset applied to the splash position to keep the fish below the surface at rest.")]
    private float restingDepthOffset = 0f;

    [Header("Impact")]
    [SerializeField, Tooltip("Radius used to check for players to knock back while the fish is airborne.")]
    private float impactRadius = 1.1f;
    [SerializeField, Tooltip("Impulse strength applied to the player when hit.")]
    private float knockbackForce = 7.5f;
    [SerializeField, Tooltip("Upward bias added to the knockback direction.")]
    private float knockbackUpwardBias = 0.65f;

    [Header("Visuals")]
    [SerializeField, Tooltip("If assigned, this transform will rotate independently. Defaults to the GameObject.")]
    private Transform visualRoot;
    [SerializeField, Tooltip("Axis used when flipping the fish 180 degrees at the apex.")]
    private Vector3 flipAxis = Vector3.forward;

    private readonly HashSet<PlayerRespawn> hitPlayersThisJump = new HashSet<PlayerRespawn>();
    private bool isJumping;
    private float nextJumpTime;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion flippedRotation;
    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
        startPosition = cachedTransform.position;
        if (restingDepthOffset != 0f)
        {
            startPosition.y += restingDepthOffset;
            cachedTransform.position = startPosition;
        }

        visualRoot = visualRoot == null ? cachedTransform : visualRoot;
        startRotation = visualRoot.rotation;
        flippedRotation = Quaternion.AngleAxis(180f, flipAxis.normalized) * startRotation;

        ScheduleNextJump(true);
    }

    private void Update()
    {
        if (isJumping)
        {
            return;
        }

        if (Time.time >= nextJumpTime)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;
        hitPlayersThisJump.Clear();

        Vector3 apexPosition = startPosition + Vector3.up * jumpHeight;

        float elapsed = 0f;
        while (elapsed < ascentDuration)
        {
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, ascentDuration));
            cachedTransform.position = Vector3.Lerp(startPosition, apexPosition, t);
            visualRoot.rotation = startRotation;
            CheckForPlayerHits();
            elapsed += Time.deltaTime;
            yield return null;
        }

        cachedTransform.position = apexPosition;
        visualRoot.rotation = flippedRotation;

        if (apexHangTime > 0f)
        {
            float hang = 0f;
            while (hang < apexHangTime)
            {
                CheckForPlayerHits();
                hang += Time.deltaTime;
                yield return null;
            }
        }

        elapsed = 0f;
        while (elapsed < descentDuration)
        {
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, descentDuration));
            cachedTransform.position = Vector3.Lerp(apexPosition, startPosition, t);
            visualRoot.rotation = flippedRotation;
            CheckForPlayerHits();
            elapsed += Time.deltaTime;
            yield return null;
        }

        cachedTransform.position = startPosition;
        visualRoot.rotation = startRotation;

        ScheduleNextJump();
        isJumping = false;
    }

    private void ScheduleNextJump(bool forceSoon = false)
    {
        if (forceSoon)
        {
            float min = Mathf.Max(0.15f, minJumpInterval * 0.25f);
            float max = Mathf.Max(min, minJumpInterval);
            nextJumpTime = Time.time + Random.Range(min, max);
        }
        else
        {
            nextJumpTime = Time.time + Random.Range(minJumpInterval, maxJumpInterval);
        }
    }

    private void CheckForPlayerHits()
    {
        Collider[] hits = Physics.OverlapSphere(cachedTransform.position, impactRadius, playerLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            PlayerRespawn player = GetPlayerFromCollider(hits[i]);
            if (player == null || hitPlayersThisJump.Contains(player))
            {
                continue;
            }

            ApplyKnockback(player);
            hitPlayersThisJump.Add(player);
        }
    }

    private void ApplyKnockback(PlayerRespawn player)
    {
        SUPERCharacterAIO controller = player.GetComponentInChildren<SUPERCharacterAIO>();
        Rigidbody targetRigidbody = controller != null ? controller.GetComponent<Rigidbody>() : player.GetComponent<Rigidbody>();

        if (targetRigidbody == null)
        {
            return;
        }

        Vector3 horizontalDirection = (player.transform.position - cachedTransform.position);
        horizontalDirection.y = 0f;
        if (horizontalDirection.sqrMagnitude < 0.001f)
        {
            horizontalDirection = cachedTransform.forward;
        }

        horizontalDirection.Normalize();
        Vector3 impulse = (horizontalDirection + Vector3.up * knockbackUpwardBias).normalized * knockbackForce;
        targetRigidbody.AddForce(impulse, ForceMode.VelocityChange);
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0f, 0.35f);
        Gizmos.DrawWireSphere(Application.isPlaying ? cachedTransform.position : transform.position, impactRadius);
    }
#endif
}
