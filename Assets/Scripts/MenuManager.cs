
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject buttonsContainer;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelMessageText;

    [Header("Game Objects")]
    public PlayerController playerController;

    [Header("Settings")]
    public float timeLimit = 60f;
    public int coinsToWin = 10;

    private bool isPaused;
    private bool resumeRequested;
    private bool isGameOver;
    private float currentTime;

    void Start()
    {
        currentTime = timeLimit;
        if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();

        if (levelMessageText != null) levelMessageText.gameObject.SetActive(false);
        
        // Start the game in a paused state
        PauseGame();
    }

    void Update()
    {
        if (isGameOver) return;

        if (resumeRequested)
        {
            resumeRequested = false;
            ResumeGame();
            return; // Return to avoid processing escape key on the same frame
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

    private void HandleTimer()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
        else
        {
            currentTime = 0;
            EndLevel("Time's up!");
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
            EndLevel("You beat level 1!");
        }
        else if (currentTime > 0 && currentCoins < coinsToWin)
        {
            EndLevel("You need at least " + coinsToWin + " coins to win!");
        }
    }

    private void EndLevel(string message)
    {
        isGameOver = true;
        Time.timeScale = 0f;
        if (playerController != null) playerController.enabled = false;
        if (buttonsContainer != null) buttonsContainer.SetActive(false);

        if (levelMessageText != null)
        {
            levelMessageText.text = message;
            levelMessageText.gameObject.SetActive(true);
        }
    }

    public void StartGame()
    {
        resumeRequested = true;
    }

    public void QuitGame()
    {
        // This will work in a built game, but not in the editor.
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        if (buttonsContainer != null) buttonsContainer.SetActive(true);
        if (playerController != null) playerController.enabled = false;
        isPaused = true;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        if (buttonsContainer != null) buttonsContainer.SetActive(false);
        if (playerController != null) playerController.enabled = true;
        isPaused = false;
    }
}
