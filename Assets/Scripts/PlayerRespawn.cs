using System.Collections;
using SUPERCharacter;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField, Tooltip("Delay before the player is placed back at the spawn point.")]
    private float respawnDelay = 0.5f;

    [Header("Lava Death Settings")]
    [SerializeField, Tooltip("Distance (in meters) the player should sink before respawning.")]
    private float lavaSinkDistance = 1.5f;
    [SerializeField, Tooltip("Speed at which the player sinks into lava.")]
    private float lavaSinkSpeed = 1f;
    [SerializeField, Tooltip("Extra wait time after sinking before teleporting back to spawn.")]
    private float lavaPostSinkDelay = 0.5f;
    [SerializeField, Tooltip("Interval between feedback pulses while sinking.")]
    private float lavaFeedbackInterval = 0.4f;

    [Header("Animation")]
    [SerializeField, Tooltip("Animator bool parameter toggled while the player is burning.")]
    private string burningBoolParameter = "IsBurning";

    private SUPERCharacterAIO movementController;
    private Rigidbody controllerRigidbody;
    private Transform characterTransform;
    private Collider characterCollider;
    private PlayerFeedback playerFeedback;
    private Animator characterAnimator;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private Vector3 characterOffsetFromRoot;
    private Quaternion characterRotationOffset;

    private bool isRespawning;

    private void Awake()
    {
        movementController = GetComponentInChildren<SUPERCharacterAIO>();
        if (movementController == null)
        {
            Debug.LogError("PlayerRespawn could not locate SUPERCharacterAIO on the player prefab.", this);
            enabled = false;
            return;
        }

        characterTransform = movementController.transform;
        controllerRigidbody = movementController.GetComponent<Rigidbody>();
        characterCollider = movementController.GetComponent<Collider>();
        playerFeedback = PlayerFeedback.instance != null ? PlayerFeedback.instance : FindObjectOfType<PlayerFeedback>();
        characterAnimator = movementController._3rdPersonCharacterAnimator;

        if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>();
        }

        characterOffsetFromRoot = characterTransform.position - transform.position;
        characterRotationOffset = characterTransform.rotation * Quaternion.Inverse(transform.rotation);
    }

    private void Start()
    {
        Transform reference = respawnPoint != null ? respawnPoint : characterTransform;
        spawnPosition = reference.position;
        spawnRotation = reference.rotation;
    }

    public void RegisterCheckpoint(Transform checkpoint)
    {
        respawnPoint = checkpoint;
        spawnPosition = checkpoint.position;
        spawnRotation = checkpoint.rotation;
    }

    public void KillPlayer()
    {
        if (!isRespawning && gameObject.activeInHierarchy)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    public void SinkIntoLava(float? sinkDistanceOverride = null, float? sinkSpeedOverride = null, float? postSinkDelayOverride = null)
    {
        if (!isRespawning && gameObject.activeInHierarchy)
        {
            float sinkDistance = Mathf.Max(0f, sinkDistanceOverride ?? lavaSinkDistance);
            float sinkSpeed = Mathf.Max(0.01f, sinkSpeedOverride ?? lavaSinkSpeed);
            float postSinkDelay = Mathf.Max(0f, postSinkDelayOverride ?? lavaPostSinkDelay);
            StartCoroutine(LavaSinkRoutine(sinkDistance, sinkSpeed, postSinkDelay));
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        if (controllerRigidbody != null)
        {
            controllerRigidbody.linearVelocity = Vector3.zero;
            controllerRigidbody.angularVelocity = Vector3.zero;
        }

        if (movementController != null)
        {
            movementController.PausePlayer(PauseModes.MakeKinematic);
        }

        if (respawnDelay > 0f)
        {
            yield return new WaitForSeconds(respawnDelay);
        }

        TeleportToSpawn();

        if (movementController != null)
        {
            movementController.UnpausePlayer(0f);
        }

        SetBurningState(false);
        isRespawning = false;
    }

    private IEnumerator LavaSinkRoutine(float sinkDistance, float sinkSpeed, float postSinkDelay)
    {
        isRespawning = true;
        SetBurningState(true);

        bool prevEnableMovement = false;
        bool prevEnableCamera = false;
        bool prevControllerPaused = false;
        bool prevIsKinematic = false;
        RigidbodyConstraints prevConstraints = RigidbodyConstraints.None;

        if (movementController != null)
        {
            prevEnableMovement = movementController.enableMovementControl;
            prevEnableCamera = movementController.enableCameraControl;
            prevControllerPaused = movementController.controllerPaused;

            movementController.enableMovementControl = false;
            movementController.enableCameraControl = false;
        }

        if (controllerRigidbody != null)
        {
            controllerRigidbody.linearVelocity = Vector3.zero;
            controllerRigidbody.angularVelocity = Vector3.zero;
            prevIsKinematic = controllerRigidbody.isKinematic;
            prevConstraints = controllerRigidbody.constraints;
            controllerRigidbody.isKinematic = true;
            controllerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        bool colliderDisabled = false;
        if (characterCollider != null && characterCollider.enabled)
        {
            characterCollider.enabled = false;
            colliderDisabled = true;
        }

        float sunk = 0f;
        float feedbackTimer = 0f;
        TriggerLavaFeedback();
        while (sunk < sinkDistance)
        {
            float step = Mathf.Min(sinkSpeed * Time.deltaTime, sinkDistance - sunk);
            transform.position += Vector3.down * step;
            sunk += step;

            feedbackTimer += Time.deltaTime;
            if (lavaFeedbackInterval > 0f && feedbackTimer >= lavaFeedbackInterval)
            {
                TriggerLavaFeedback();
                feedbackTimer = 0f;
            }

            yield return null;
        }

        float waitTime = Mathf.Max(respawnDelay, postSinkDelay);
        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        TeleportToSpawn();

        if (colliderDisabled && characterCollider != null)
        {
            characterCollider.enabled = true;
        }

        if (movementController != null)
        {
            movementController.enableMovementControl = prevEnableMovement;
            movementController.enableCameraControl = prevEnableCamera;
            movementController.controllerPaused = prevControllerPaused;
        }

        if (controllerRigidbody != null)
        {
            controllerRigidbody.isKinematic = prevIsKinematic;
            controllerRigidbody.constraints = prevConstraints;
            if (!controllerRigidbody.isKinematic)
            {
                controllerRigidbody.linearVelocity = Vector3.zero;
                controllerRigidbody.angularVelocity = Vector3.zero;
            }
        }

        SetBurningState(false);
        isRespawning = false;
    }

    private void TriggerLavaFeedback()
    {
        if (playerFeedback == null)
        {
            playerFeedback = PlayerFeedback.instance != null ? PlayerFeedback.instance : FindObjectOfType<PlayerFeedback>();
        }

        if (playerFeedback != null)
        {
            playerFeedback.TriggerEffects();
        }
    }

    private void TeleportToSpawn()
    {
        Vector3 rootPosition = spawnPosition - characterOffsetFromRoot;
        Quaternion rootRotation = spawnRotation * Quaternion.Inverse(characterRotationOffset);

        transform.SetPositionAndRotation(rootPosition, rootRotation);
        characterTransform.SetPositionAndRotation(spawnPosition, spawnRotation);

        if (controllerRigidbody != null)
        {
            if (!controllerRigidbody.isKinematic)
            {
                controllerRigidbody.linearVelocity = Vector3.zero;
                controllerRigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                controllerRigidbody.Sleep();
            }
        }
    }

    private void SetBurningState(bool isBurning)
    {
        if (characterAnimator != null && !string.IsNullOrEmpty(burningBoolParameter))
        {
            characterAnimator.SetBool(burningBoolParameter, isBurning);
        }
    }
}
