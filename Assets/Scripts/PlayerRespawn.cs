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

    private SUPERCharacterAIO movementController;
    private Rigidbody controllerRigidbody;
    private Transform characterTransform;

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

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        if (movementController != null)
        {
            movementController.PausePlayer(PauseModes.MakeKinematic);
        }

        if (controllerRigidbody != null)
        {
            controllerRigidbody.linearVelocity = Vector3.zero;
            controllerRigidbody.angularVelocity = Vector3.zero;
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

        isRespawning = false;
    }

    private void TeleportToSpawn()
    {
        Vector3 rootPosition = spawnPosition - characterOffsetFromRoot;
        Quaternion rootRotation = spawnRotation * Quaternion.Inverse(characterRotationOffset);

        transform.SetPositionAndRotation(rootPosition, rootRotation);
        characterTransform.SetPositionAndRotation(spawnPosition, spawnRotation);

        if (controllerRigidbody != null)
        {
            controllerRigidbody.linearVelocity = Vector3.zero;
            controllerRigidbody.angularVelocity = Vector3.zero;
        }
    }
}
