using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using FMOD.Studio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI scoreText; // Assign in Inspector
    public GameObject gameOverUI; // Assign a UI panel with score display & reset button

    [SerializeField] private EventReference fightMusic;

    public static GameManager instance { get; private set; }

   private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Keeps it alive across scene reloads
        }
        else
        {
            Destroy(gameObject); // ✅ Prevents duplicate instances
        }
    }

    public void HandleGameOver(bool win)
    {
        gameOverUI.SetActive(true); // Show game over UI
        Time.timeScale = 0; // Pause the game
        if (win)
        {
            scoreText.text = $"You Win!";
        }
        else
        {
            scoreText.text = $"You Lose!";
        }
    }

    public void ResetScene()
    {
        gameOverUI.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
        AudioManager.instance.StopMusic();
    }
}