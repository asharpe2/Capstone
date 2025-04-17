using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;
using TMPro;
using FMODUnity;

public class StartScreenManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] EventReference titleMusic;
    [SerializeField] EventReference fightMusic;

    [Header("UI Canvases & Panels")]
    [SerializeField] GameObject startScreenCanvas;
    [SerializeField] GameObject playCanvas;
    [SerializeField] GameObject interimCanvas;
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button backButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button resumeButton;
    [SerializeField] Button mainMenuPauseButton;

    [Header("Gameplay")]
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject player2Object;
    [SerializeField] RoundTimerManager roundTimerManager;
    [SerializeField] MonoBehaviour[] scriptsToDisable;

    [Header("Cinemachine")]
    [SerializeField] CinemachineVirtualCamera menuCamera;
    [SerializeField] CinemachineVirtualCamera mainCamera;
    [SerializeField] CinemachineVirtualCamera optionsCamera;
    [SerializeField] CinemachineVirtualCamera endCamera;

    [Header("Pause Input")]
    [SerializeField] InputActionReference pauseAction;

    [Header("Game Over")]
    [SerializeField] TextMeshProUGUI scoreText;

    private CinemachineBrain _brain;
    private bool _isPaused;
    private const float BlendTime = 2f;
    public static StartScreenManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void Start()
    {
        // Initial state
        startScreenCanvas.SetActive(true);
        playCanvas.SetActive(false);
        interimCanvas.SetActive(false);
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);

        SetGameplayActive(false);
        SetCamera(menuCamera);

        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        AudioManager.instance.PlayMusic(titleMusic);
    }

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += _ => TogglePause();
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= _ => TogglePause();
        pauseAction.action.Disable();
    }

    //────────────────────────────────────────────────────
    // Common helpers
    //────────────────────────────────────────────────────
    private void SetCamera(CinemachineVirtualCamera cam)
    {
        menuCamera.Priority = (cam == menuCamera) ? 10 : 0;
        mainCamera.Priority = (cam == mainCamera) ? 10 : 0;
        optionsCamera.Priority = (cam == optionsCamera) ? 10 : 0;
        endCamera.Priority = (cam == endCamera) ? 10 : 0;
    }

    private void SetGameplayActive(bool active)
    {
        // Toggle GameObjects & scripts
        playerObject?.SetActive(active);
        player2Object?.SetActive(active);
        foreach (var s in scriptsToDisable) s.enabled = active;

        // Start the round & reset if turning on
        if (active)
        {
            roundTimerManager.StartRoundTimer();
            GameManager.Instance.FullReset();
            AudioManager.instance.PlayMusic(fightMusic);
        }
        else
        {
            AudioManager.instance.PlayMusic(titleMusic);
        }
    }

    private IEnumerator MenuTransition(
        CinemachineVirtualCamera targetCam,
        GameObject panelToHide,
        GameObject panelToShow,
        Button buttonToSelect = null)
    {
        // Pre‐transition
        panelToHide?.SetActive(false);
        SetCamera(targetCam);

        // Blend
        yield return new WaitForSeconds(BlendTime);

        // Post‐transition
        panelToShow?.SetActive(true);
        if (buttonToSelect != null)
            EventSystem.current.SetSelectedGameObject(buttonToSelect.gameObject);
    }

    private IEnumerator GameTransition(
        CinemachineVirtualCamera targetCam,
        GameObject panelToHide,
        GameObject panelToShow,
        bool enableGameplay,
        float extraDelayAfterBlend = 0f)
    {
        // Pre‐transition
        panelToHide?.SetActive(false);
        SetCamera(targetCam);

        yield return new WaitForSeconds(BlendTime);
        yield return new WaitForSeconds(extraDelayAfterBlend);

        // Post‐transition
        panelToShow?.SetActive(true);
        SetGameplayActive(enableGameplay);

        // For start/restart: unlock any lingering input
        if (enableGameplay)
        {
            playerObject.GetComponent<PlayerController>()?.UnlockInput();
            player2Object.GetComponent<PlayerController>()?.UnlockInput();
        }
    }

    //────────────────────────────────────────────────────
    // Button handlers now boil down to single lines
    //────────────────────────────────────────────────────
    public void OnStartGame()
        => StartCoroutine(GameTransition(mainCamera, startScreenCanvas, playCanvas, true, 0.1f));

    public void RestartGame()
        => StartCoroutine(GameTransition(mainCamera, gameOverPanel, playCanvas, true));

    public void EndGame()
        => StartCoroutine(MenuTransition(menuCamera, gameOverPanel, mainMenuPanel, startButton));

    public void OpenOptionsMenu()
        => StartCoroutine(MenuTransition(optionsCamera, mainMenuPanel, optionsPanel, backButton));

    public void CloseOptionsMenu()
        => StartCoroutine(MenuTransition(menuCamera, optionsPanel, mainMenuPanel, startButton));

    public void HandleGameOver(bool player1Won)
    {
        // We still need one custom step for setting scoreText
        scoreText.text = player1Won ? "Player 1 Wins!" : "Player 2 Wins!";
        StartCoroutine(MenuTransition(endCamera, playCanvas /*or interimCanvas*/, gameOverPanel, restartButton));
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    //────────────────────────────────────────────────────
    // Pause
    //────────────────────────────────────────────────────
    public void TogglePause()
    {
        // don’t even toggle if we’re not in the actual match
        if (!playCanvas.activeSelf) return;

        if (_isPaused) ResumeGame();
        else           PauseGame();
    }
    private void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);

        // just lock controls, don’t touch music
        playerObject.GetComponent<PlayerController>()?.EnableUIControls();
        player2Object.GetComponent<PlayerController>()?.EnableUIControls();

        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        _isPaused = false;

        // unlock controls again
        playerObject.GetComponent<PlayerController>()?.EnableGameplayControls();
        player2Object.GetComponent<PlayerController>()?.EnableGameplayControls();
    }


    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        playCanvas.SetActive(false);
        pausePanel.SetActive(false);
        StartCoroutine(MenuTransition(menuCamera, null, mainMenuPanel, startButton));
    }
}
