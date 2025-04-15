using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;
using System.Collections;
using TMPro;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject startScreenCanvas;
    public GameObject playCanvas;
    public GameObject interimCanvas;
    public Button startButton;
    public Button backButton;
    public Button restartButton;

    [Header("Gameplay Control")]
    public GameObject playerObject; // Parent of player input/movement
    public GameObject player2Object; // Parent of player input/movement
    public MonoBehaviour[] scriptsToDisable; // Any gameplay scripts to lock

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject gameOverPanel;

    [Header("Cameras")]
    public CinemachineVirtualCamera menuCamera;
    public CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera optionsCamera;
    public CinemachineVirtualCamera endCamera;
    private CinemachineBrain brain;

    [Header("Input")]
    public InputActionAsset inputActions; // Drag in your InputActions asset

    private InputAction startAction;
    public TextMeshProUGUI scoreText;

    public static StartScreenManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // Optional safety
    }

    void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        startScreenCanvas.SetActive(true);
        SetGameplayActive(false);


        // Focus the first button
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        startAction.performed -= ctx => OnStartGame();
        startAction.Disable();
    }

    private void SetCamera(CinemachineVirtualCamera cam)
    {
        // Set target camera to high priority, others low
        menuCamera.Priority = (cam == menuCamera) ? 10 : 0;
        mainCamera.Priority = (cam == mainCamera) ? 10 : 0;
        optionsCamera.Priority = (cam == optionsCamera) ? 10 : 0;
        endCamera.Priority = (cam == endCamera) ? 10 : 0;
    }

    public void ShowMainMenu()
    {
        SetCamera(menuCamera);
    }

    public void OnStartGame()
    {
        StartCoroutine(TransitionToGameplayAccurate());
    }

    private IEnumerator TransitionToGameplayAccurate()
    {
        SetCamera(mainCamera);
        startScreenCanvas.SetActive(false);

        yield return new WaitForSeconds(2); // Wait for camera blend to finish
        playCanvas.SetActive(true);

        // Additional delay to let the button release clear
        yield return new WaitForSeconds(0.1f);

        SetGameplayActive(true);

        // Unlock player input to ensure residual button presses are ignored
        PlayerController pc1 = playerObject.GetComponent<PlayerController>();
        PlayerController pc2 = player2Object.GetComponent<PlayerController>();
        if (pc1 != null) pc1.UnlockInput();
        if (pc2 != null) pc2.UnlockInput();
    }

    void SetGameplayActive(bool isActive)
    {
        if (playerObject != null)
            playerObject.SetActive(isActive);

        if (player2Object != null)
            player2Object.SetActive(isActive);

        foreach (var script in scriptsToDisable)
            script.enabled = isActive;
    }


    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void EndGame()
    {
        StartCoroutine(TransitionFromEnd());
    }

    private IEnumerator TransitionFromEnd()
    {
        SetCamera(menuCamera);
        gameOverPanel.SetActive(true);
        GameManager.Instance.FullReset();

        yield return new WaitForSeconds(2f); // match Cinemachine blend time

        mainMenuPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    public void RestartGame()
    {
        StartCoroutine(TransitionToRestart());
    }

    private IEnumerator TransitionToRestart()
    {
        // Transition camera and UI as before.
        SetCamera(mainCamera);
        gameOverPanel.SetActive(false);

        // Call your full reset routine.
        GameManager.Instance.FullReset();

        yield return new WaitForSeconds(2f);

        // Re-enable gameplay UI and inputs.
        playCanvas.SetActive(true);
        SetGameplayActive(true);

        // (Optionally) reinitialize any interim components.
    }


    public void OpenOptionsMenu()
    {
        StartCoroutine(TransitionToOptions());
    }

    private IEnumerator TransitionToOptions()
    {
        SetCamera(optionsCamera);
        mainMenuPanel.SetActive(false);

        yield return new WaitForSeconds(2f); // match Cinemachine blend time

        optionsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
    }

    public void CloseOptionsMenu()
    {
        StartCoroutine(TransitionToMainMenu());
    }

    private IEnumerator TransitionToMainMenu()
    {
        SetCamera(menuCamera);
        optionsPanel.SetActive(false);

        yield return new WaitForSeconds(2f);

        mainMenuPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    public void HandleGameOver(bool win)
    {
        StartCoroutine(TransitionToGameOver(win));
    }

    private IEnumerator TransitionToGameOver(bool win)
    {
        SetGameplayActive(false);
        SetCamera(endCamera);
        playCanvas.SetActive(false);
        interimCanvas.SetActive(false);

        // Wait for camera blend
        yield return new WaitForSeconds(2f);

        // 1) Show the panel
        gameOverPanel.SetActive(true);

        // 2) (Optional) Wait until next frame so Unity can register the new UI layout
        yield return null;

        // 3) Now highlight the Restart button
        EventSystem.current.SetSelectedGameObject(restartButton.gameObject);

        // Update the score text
        if (win) scoreText.text = "Player 1 Wins!";
        else scoreText.text = "Player 2 Wins!";
    }
}
