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

    // Stat tracking
    public int playerDamage;
    public int enemyDamage;
    public int playerCombos;
    public int enemyCombos;

    // Round tracking
    public int currentRound = 1;
    public int totalRounds = 3;

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
        AudioManager.instance.PlayMusic(fightMusic);
    }

    public void HandleGameOver(bool win)
    {
        gameOverUI.SetActive(true); // Show game over UI
        Time.timeScale = 0; // Pause the game
        if (win)
        {
            scoreText.text = $"Player 1 Wins!";
        }
        else
        {
            scoreText.text = $"Player 2 Wins!";
        }
    }

    public void ResetRoundStats()
    {
        playerDamage = 0;
        enemyDamage = 0;
        playerCombos = 0;
        enemyCombos = 0;
    }

    public void ResetScene()
    {
        gameOverUI.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
        AudioManager.instance.StopMusic();
    }
}