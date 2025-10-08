using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerFeedback : MonoBehaviour
{
    public static PlayerFeedback instance;

    [Header("Screen Blink")]
    public Image blinkImage;
    public float blinkDuration = 0.1f;
    public Color blinkColor = new Color(1, 0, 0, 0.5f);

    [Header("Camera Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    [Header("Floating Text")]
    public GameObject floatingTextPrefab;
    public Transform floatingTextSpawnPoint;

    private Camera mainCamera;
    private Vector3 originalCameraPos;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mainCamera = Camera.main;

        // Setup objects if they are not assigned
        if (blinkImage == null)
        {
            SetupBlinkImage();
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
    }

    public void TriggerEffects()
    {
        StartCoroutine(Blink());
        StartCoroutine(Shake());
        ShowFloatingText();
    }

    private IEnumerator Blink()
    {
        if (blinkImage == null) yield break;
        blinkImage.color = blinkColor;
        blinkImage.enabled = true;
        yield return new WaitForSeconds(blinkDuration);
        blinkImage.enabled = false;
    }

    private IEnumerator Shake()
    {
        originalCameraPos = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            mainCamera.transform.localPosition = new Vector3(originalCameraPos.x + x, originalCameraPos.y + y, originalCameraPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalCameraPos;
    }

    private void ShowFloatingText()
    {
        if (floatingTextPrefab != null && floatingTextSpawnPoint != null)
        {
            GameObject textGO = Instantiate(floatingTextPrefab, floatingTextSpawnPoint.position, Quaternion.identity, floatingTextSpawnPoint);
            Destroy(textGO, 1f);
        }
    }

    private void SetupBlinkImage()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("FeedbackCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        GameObject blinkGO = new GameObject("BlinkImage");
        blinkGO.transform.SetParent(canvas.transform);
        blinkImage = blinkGO.AddComponent<Image>();
        blinkImage.color = new Color(1, 0, 0, 0.5f);
        blinkImage.rectTransform.anchorMin = Vector2.zero;
        blinkImage.rectTransform.anchorMax = Vector2.one;
        blinkImage.rectTransform.sizeDelta = Vector2.zero;
        blinkImage.enabled = false;
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