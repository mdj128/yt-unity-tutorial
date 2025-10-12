using UnityEngine;

/// <summary>
/// Adds and maintains a point light that hovers over the lava surface so it emits a warm glow.
/// Attach this to the lava terrain or mesh; it will create a child light automatically.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class LavaGlowLight : MonoBehaviour
{
    [SerializeField]
    private Color lightColor = new Color(1f, 0.45f, 0f);

    [SerializeField]
    [Tooltip("Brightness of the glow light. Adjust to taste.")]
    private float intensity = 15f;

    [SerializeField]
    [Tooltip("Extra distance to add to the computed light range.")]
    private float rangePadding = 5f;

    [SerializeField]
    [Tooltip("How high above the surface the light should sit.")]
    private float heightOffset = 2.5f;

    private Light glowLight;

    private void OnEnable()
    {
        EnsureLight();
        UpdateLightTransform();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        EnsureLight();
        UpdateLightTransform();
    }
#endif

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateLightTransform();
        }
#endif
    }

    private void EnsureLight()
    {
        if (glowLight != null)
        {
            return;
        }

        Transform child = transform.Find("Lava Glow Light");
        if (child == null)
        {
            GameObject lightGO = new GameObject("Lava Glow Light");
            child = lightGO.transform;
            child.SetParent(transform);
        }

        glowLight = child.GetComponent<Light>();
        if (glowLight == null)
        {
            glowLight = child.gameObject.AddComponent<Light>();
        }

        glowLight.type = LightType.Point;
        glowLight.shadows = LightShadows.Soft;
    }

    private void UpdateLightTransform()
    {
        if (glowLight == null)
        {
            return;
        }

        Bounds bounds;
        if (!TryGetTargetBounds(out bounds))
        {
            bounds = new Bounds(transform.position, Vector3.one * 4f);
        }

        Vector3 position = bounds.center;
        position.y = bounds.max.y + heightOffset;
        glowLight.transform.position = position;

        float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) + rangePadding;
        glowLight.range = Mathf.Max(0.1f, radius);
        glowLight.intensity = Mathf.Max(0f, intensity);
        glowLight.color = lightColor;
    }

    private bool TryGetTargetBounds(out Bounds bounds)
    {
        if (TryGetComponent(out Terrain terrain) && terrain.terrainData != null)
        {
            Bounds terrainBounds = terrain.terrainData.bounds;
            Vector3 worldCenter = terrain.transform.TransformPoint(terrainBounds.center);
            Vector3 worldSize = Vector3.Scale(terrainBounds.size, terrain.transform.lossyScale);
            bounds = new Bounds(worldCenter, worldSize);
            return true;
        }

        if (TryGetComponent(out Renderer renderer))
        {
            bounds = renderer.bounds;
            return true;
        }

        if (TryGetComponent(out Collider collider))
        {
            bounds = collider.bounds;
            return true;
        }

        bounds = default;
        return false;
    }
}
