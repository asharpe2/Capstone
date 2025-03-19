using UnityEngine;
using TMPro;

public class InterimManager : MonoBehaviour
{
    // UI Elements
    [Header("UI")]
    public GameObject interimUI;
    public TMP_Text player1StatsText;
    public TMP_Text player2StatsText;

    // Player Stat References
    [Header("Players")]
    public PlayerStats player1Stats;
    public PlayerStats player2Stats;

    public PlayerController player1Controller;
    public PlayerController player2Controller;

    // Timer & Skip
    [Header("Settings")]
    public float interimDuration = 10f;
    private float timer;
    private bool player1Ready = false;
    private bool player2Ready = false;

    private RoundTimerManager roundTimerManager;

    void Awake()
    {
        roundTimerManager = GetComponent<RoundTimerManager>(); // Same GameManager

        // Optional null checks:
        if (player1Controller == null || player2Controller == null)
        {
            Debug.LogError("Player controller references not assigned in InterimManager!");
        }
    }

    void Start()
    {
        interimUI.SetActive(false); // Start hidden
    }

    public void StartInterim()
    {
        // Disable player controls
        player1Controller.enabled = false;
        player2Controller.enabled = false;

        // Show UI & populate stats
        interimUI.SetActive(true);
        player1StatsText.text = $"Damage: {player1Stats.totalDamageDealt}\nCombos: {player1Stats.totalCombos}";
        player2StatsText.text = $"Damage: {player2Stats.totalDamageDealt}\nCombos: {player2Stats.totalCombos}";

        player1Ready = false;
        player2Ready = false;
        timer = interimDuration;
    }

    void Update()
    {
        if (!interimUI.activeSelf) return;

        timer -= Time.deltaTime;

        // Replace with actual player input
        if (Input.GetKeyDown(KeyCode.Joystick1Button7)) player1Ready = true;
        if (Input.GetKeyDown(KeyCode.Joystick2Button7)) player2Ready = true;

        if ((player1Ready && player2Ready) || timer <= 0)
        {
            EndInterim();
        }
    }

    void EndInterim()
    {
        interimUI.SetActive(false);

        // Reset stats
        player1Stats.ResetStats();
        player2Stats.ResetStats();

        // Enable controls
        player1Controller.enabled = true;
        player2Controller.enabled = true;

        // Start next round
        roundTimerManager.StartRoundTimer();
    }
}
