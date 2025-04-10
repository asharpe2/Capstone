using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RoundTimerManager : MonoBehaviour
{
    public float roundDuration = 60f; // Seconds per round
    private float currentTime;

    public TMP_Text timerText;

    private bool timerActive = false;

    private InterimManager interimManager;
    private GameManager gameManager;

    void Awake()
    {
        interimManager = GetComponent<InterimManager>();
        gameManager = GetComponent<GameManager>();
    }


    void Start()
    {
        StartRoundTimer();
    }

    void Update()
    {
        if (timerActive)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTime <= 0)
            {
                EndRoundDueToTimer(interimManager.player1Stats.totalDamageDealt, interimManager.player2Stats.totalDamageDealt);
            }
        }
    }

    public void StartRoundTimer()
    {
        GameManager.Instance.currentRound++;
        currentTime = roundDuration;
        timerActive = true;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        if (seconds <= 0)
        {
            timerText.text = "00:00";
        }
        else
        {
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void EndRound()
    {
        timerActive = false;
        interimManager.StartInterim(); // Transition to interim phase
    }

    //TODO: CREATE MORE ROBUST ROUND WINNER CALCULATION
    private int DetermineRoundWinner(int player1Damage, int player2Damage)
    {
        if (player1Damage > player2Damage) return 1;
        else return 2;
    }

    public void EndRoundDueToTimer(int player1Damage, int player2Damage)
    {
        EndRound();

        // Call your dynamic formula here
        int roundWinner = DetermineRoundWinner(player1Damage, player2Damage); // returns 1, 2, or 0 for draw

        if (roundWinner == 1)
        {
            GameManager.Instance.player1RoundsWon++;
            interimManager.ShowRoundWinner("Player 1 Wins Round!");
            Debug.Log("Player 1 Wins Round!");
        }
        else if (roundWinner == 2)
        {
            GameManager.Instance.player2RoundsWon++;
            interimManager.ShowRoundWinner("Player 2 Wins Round!");
            Debug.Log("Player 2 Wins Round!");
        }
        else
        {
            interimManager.ShowRoundWinner("Draw!");
            Debug.Log("Draw");
        }

        // Check if it's the final round
        if (GameManager.Instance.currentRound > GameManager.Instance.totalRounds)
        {
            DetermineMatchWinnerByRounds();
        }
    }

    public void DetermineMatchWinnerByRounds()
    {
        if (GameManager.Instance.player1RoundsWon > GameManager.Instance.player2RoundsWon)
        {
            gameManager.HandleGameOver(true);
        }
        else if (GameManager.Instance.player2RoundsWon > GameManager.Instance.player1RoundsWon)
        {
            gameManager.HandleGameOver(false);
        }
        else
        {
            return;
        }
    }
}
