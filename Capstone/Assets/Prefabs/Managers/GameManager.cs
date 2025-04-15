using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using FMOD.Studio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject gameOverUI; // Assign a UI panel with score display & reset button

    [SerializeField] private EventReference fightMusic;

    public static GameManager instance { get; private set; }

    // Stat tracking
    public int playerDamage;
    public int enemyDamage;
    public int playerCombos;
    public int enemyCombos;

    // Round tracking
    public int currentRound;
    public int totalRounds;
    public int player1RoundsWon;
    public int player2RoundsWon;

    private InterimManager interimManager;

    private void Awake()
    {
        interimManager = GetComponent<InterimManager>();
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

    public void ResetRoundStats()
    {
        playerDamage = 0;
        enemyDamage = 0;
        playerCombos = 0;
        enemyCombos = 0;
    }

    public void ResetMatchStats()
    {
        player1RoundsWon = 0;
        player2RoundsWon = 0;
        currentRound = 1;
    }

    public void FullReset()
    {
        interimManager.EndInterim();
        interimManager.player1Controller.ResetPlayer();
        interimManager.player1Controller.UpdateUI();
        interimManager.player2Controller.ResetPlayer();
        interimManager.player2Controller.UpdateUI();
        ResetRoundStats();
        ResetMatchStats();
    }
}