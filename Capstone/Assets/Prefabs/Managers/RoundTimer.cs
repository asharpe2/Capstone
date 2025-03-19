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

    void Awake()
    {
        interimManager = GetComponent<InterimManager>();
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
                EndRound();
            }
        }
    }

    public void StartRoundTimer()
    {
        currentTime = roundDuration;
        timerActive = true;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void EndRound()
    {
        timerActive = false;
        interimManager.StartInterim(); // Transition to interim phase
    }
}
