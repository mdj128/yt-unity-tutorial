using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFeedback : MonoBehaviour
{
    public static PlayerFeedback instance;

    [Header("Camera Shake")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private AnimationCurve shakeDampingCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Screen Flash")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0f, 0.45f);
    [SerializeField] private AnimationCurve flashAlphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Floating Text")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform floatingTextSpawnPoint;
    [SerializeField] private float floatingTextLifetime = 1f;

    private Coroutine shakeCoroutine;
    private Coroutine flashCoroutine;
    private Vector3 cachedCameraLocalPosition;
    private Color flashColorTransparent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (targetCamera == null)
        {
            targetCamera = GetComponentInParent<Camera>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (flashImage == null)
        {
            SetupFlashImage();
        }

        if (floatingTextPrefab == null)
        {
            SetupFloatingTextPrefab();
        }

        if (floatingTextSpawnPoint == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                floatingTextSpawnPoint = player.transform;
            }
        }

        if (flashImage != null)
        {
            flashImage.raycastTarget = false;
            flashColorTransparent = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            flashImage.color = flashColorTransparent;
            flashImage.enabled = false;
            flashImage.gameObject.SetActive(false);
        }
    }

    public void TriggerEffects()
    {
        TriggerShake();
        TriggerFlash();
        ShowFloatingText();
    }

    private void TriggerShake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogWarning("PlayerFeedback: No camera assigned for shake effect.");
                return;
            }
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            targetCamera.transform.localPosition = cachedCameraLocalPosition;
            shakeCoroutine = null;
        }

        cachedCameraLocalPosition = targetCamera.transform.localPosition;
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float progress = elapsed / shakeDuration;
            float damping = shakeDampingCurve.Evaluate(progress);
            Vector2 offset = Random.insideUnitCircle * shakeMagnitude * damping;

            targetCamera.transform.localPosition = cachedCameraLocalPosition + new Vector3(offset.x, offset.y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        targetCamera.transform.localPosition = cachedCameraLocalPosition;
        shakeCoroutine = null;
    }

    private void TriggerFlash()
    {
        if (flashImage == null)
        {
            return;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.gameObject.SetActive(true);
        flashImage.enabled = true;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            float progress = elapsed / flashDuration;
            float alphaScale = flashAlphaCurve.Evaluate(progress);

            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashColor.a * alphaScale);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        flashImage.color = flashColorTransparent;
        flashImage.enabled = false;
        flashImage.gameObject.SetActive(false);
        flashCoroutine = null;
    }

    private void ShowFloatingText()
    {
        if (floatingTextPrefab != null && floatingTextSpawnPoint != null)
        {
            GameObject textGO = Instantiate(floatingTextPrefab, floatingTextSpawnPoint.position, Quaternion.identity, floatingTextSpawnPoint);
            Destroy(textGO, floatingTextLifetime);
        }
    }

    private void SetupFlashImage()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        GameObject flashGO = new GameObject("CoinStealFlash", typeof(RectTransform), typeof(Image));
        flashGO.transform.SetParent(canvas.transform, false);

        flashImage = flashGO.GetComponent<Image>();
        flashImage.rectTransform.anchorMin = Vector2.zero;
        flashImage.rectTransform.anchorMax = Vector2.one;
        flashImage.rectTransform.offsetMin = Vector2.zero;
        flashImage.rectTransform.offsetMax = Vector2.zero;
        flashImage.rectTransform.SetAsLastSibling();
        flashImage.color = flashColor;
    }

    private void SetupFloatingTextPrefab()
    {
        GameObject floatingTextGO = new GameObject("FloatingText");
        Text floatingText = floatingTextGO.AddComponent<Text>();
        floatingText.text = "-1";
        floatingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        floatingText.fontSize = 40;
        floatingText.alignment = TextAnchor.MiddleCenter;
        floatingText.color = Color.red;
        ContentSizeFitter sizeFitter = floatingTextGO.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        floatingTextGO.SetActive(false);

        floatingTextPrefab = floatingTextGO;
    }
}
