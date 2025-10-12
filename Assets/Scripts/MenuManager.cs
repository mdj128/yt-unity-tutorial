
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject buttonsContainer;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelMessageText;

    [Header("Manual Click Detection")]
    public GraphicRaycaster graphicRaycaster;
    public GameObject startButton;
    public GameObject quitButton;

    [Header("Game Objects")]
    public PlayerController playerController;
    public PlayerRespawn playerRespawn;

    [Header("Scene Flow")]
    [SerializeField] private string nextLevelSceneName = "Level2";
    [SerializeField] private float levelCompleteDelay = 2f;
    [SerializeField] private float failureMessageDuration = 2f;

    [Header("Settings")]
    public float timeLimit = 60f;
    public int coinsToWin = 10;

    private bool isPaused;
    private bool isGameOver;
    private float currentTime;
    private string defaultLevelMessage;
    private Coroutine messageCoroutine;

    void Start()
    {
        currentTime = timeLimit;
        if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();

        if (levelMessageText != null)
        {
            defaultLevelMessage = levelMessageText.text;
            levelMessageText.gameObject.SetActive(false);
        }

        EnsurePlayerRespawnReference();
        
        // Start the game in a paused state
        PauseGame();
    }

    void Update()
    {
        if (isGameOver) return;

        // Manual click detection as a workaround for EventSystem issues
        if (isPaused && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleManualClick();
        }

        // Toggle pause state with the Escape key
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (!isPaused)
        {
            HandleTimer();
        }
    }

    private void HandleManualClick()
    {
        Debug.Log("[DEBUG] Manual click detected while paused.");
        if (graphicRaycaster == null) return;

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == startButton)
            {
                Debug.Log("[DEBUG] Manual raycast hit START button.");
                StartGame();
                return; // Exit after handling the click
            }
            if (result.gameObject == quitButton)
            {
                Debug.Log("[DEBUG] Manual raycast hit QUIT button.");
                QuitGame();
                return; // Exit after handling the click
            }
        }
    }

    private void HandleTimer()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
        if (currentTime <= 0)
        {
            currentTime = 0;
            HandleLevelFailure("Time's up!");
        }

        if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    public void PlayerReachedPortal()
    {
        if (isGameOver) return;

        int currentCoins = 0;
        if (CollectableCounter.instance != null)
        {
            currentCoins = CollectableCounter.instance.currentCount;
        }

        if (currentTime > 0 && currentCoins >= coinsToWin)
        {
            if (!isGameOver)
            {
                StartCoroutine(LevelCompleteRoutine("You beat level 1!"));
            }
        }
        else if (currentTime > 0 && currentCoins < coinsToWin)
        {
            HandleLevelFailure("You need at least " + coinsToWin + " coins to win!");
        }
    }

    public void StartGame()
    {
        Debug.Log("[DEBUG] StartGame() method was called.");
        ResumeGame();
    }

    public void QuitGame()
    {
        Debug.Log("[DEBUG] QuitGame() method was called.");
        // This will work in a built game, but not in the editor.
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void PauseGame()
    {
        Debug.Log("[DEBUG] Pausing game.");
        Time.timeScale = 0f;
        if (buttonsContainer != null) buttonsContainer.SetActive(true);
        if (levelMessageText != null)
        {
            if (messageCoroutine == null && !string.IsNullOrEmpty(defaultLevelMessage))
            {
                levelMessageText.text = defaultLevelMessage;
            }
            levelMessageText.gameObject.SetActive(true);
        }
        if (playerController != null) playerController.enabled = false;
        isPaused = true;

        // Unlock the cursor so it's visible and can be used with UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        Debug.Log("[DEBUG] Resuming game.");
        Time.timeScale = 1f;
        if (buttonsContainer != null) buttonsContainer.SetActive(false);
        if (levelMessageText != null && !isGameOver && messageCoroutine == null)
        {
            levelMessageText.gameObject.SetActive(false);
        }
        if (playerController != null) playerController.enabled = true;
        isPaused = false;

        // Lock the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void EnsurePlayerRespawnReference()
    {
        if (playerRespawn != null) return;

        if (playerController != null)
        {
            playerRespawn = playerController.GetComponent<PlayerRespawn>();
            if (playerRespawn == null)
            {
                playerRespawn = playerController.GetComponentInParent<PlayerRespawn>();
            }
        }

        if (playerRespawn == null)
        {
            playerRespawn = FindObjectOfType<PlayerRespawn>();
            if (playerRespawn == null)
            {
                Debug.LogWarning("[DEBUG] MenuManager could not automatically locate PlayerRespawn.");
            }
        }
    }

    private void HandleLevelFailure(string message)
    {
        Debug.Log("[DEBUG] Level failure: " + message);

        currentTime = timeLimit;
        if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();

        ShowTemporaryMessage(message, failureMessageDuration);

        if (playerRespawn != null)
        {
            playerRespawn.KillPlayer();
        }
        else
        {
            Debug.LogWarning("[DEBUG] PlayerRespawn reference is missing; cannot respawn player.");
        }
    }

    private void ShowTemporaryMessage(string message, float duration)
    {
        if (levelMessageText == null) return;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(ShowTemporaryMessageRoutine(message, duration));
    }

    private IEnumerator ShowTemporaryMessageRoutine(string message, float duration)
    {
        levelMessageText.text = message;
        levelMessageText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        levelMessageText.text = defaultLevelMessage;
        if (!isPaused)
        {
            levelMessageText.gameObject.SetActive(false);
        }
        messageCoroutine = null;
    }

    private IEnumerator LevelCompleteRoutine(string message)
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (buttonsContainer != null) buttonsContainer.SetActive(false);
        if (playerController != null) playerController.enabled = false;

        if (levelMessageText != null)
        {
            if (messageCoroutine != null)
            {
                StopCoroutine(messageCoroutine);
                messageCoroutine = null;
            }
            levelMessageText.text = message;
            levelMessageText.gameObject.SetActive(true);
        }

        // Unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, levelCompleteDelay));

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelSceneName);
    }
}
