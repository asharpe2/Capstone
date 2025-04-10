using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject startScreenCanvas;
    public GameObject playCanvas;
    public Button startButton;

    [Header("Gameplay Control")]
    public GameObject playerObject; // Parent of player input/movement
    public GameObject player2Object; // Parent of player input/movement
    public MonoBehaviour[] scriptsToDisable; // Any gameplay scripts to lock

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public Button backButton; // First button in options menu

    [Header("Cameras")]
    public CinemachineVirtualCamera menuCamera;
    public CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera optionsCamera;

    [Header("Input")]
    public InputActionAsset inputActions; // Drag in your InputActions asset

    private InputAction startAction;

    void Start()
    {
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
    }

    public void ShowMainMenu()
    {
        SetCamera(menuCamera);
    }

    public void OnStartGame()
    {
        startScreenCanvas.SetActive(false);
        playCanvas.SetActive(true);
        SetGameplayActive(true);
        SetCamera(mainCamera);
    }

    void SetGameplayActive(bool isActive)
    {
        if (playerObject != null)
            playerObject.SetActive(isActive); 

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

    public void OpenOptionsMenu()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        SetCamera(optionsCamera);
    }

    public void CloseOptionsMenu()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }
}
