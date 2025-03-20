using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InterimManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject interimUI;
    public TMP_Text player1StatsText;
    public TMP_Text player2StatsText;
    public TMP_Text skipPromptText; // NEW: Skip prompt text
    public Image InterimTimer;

    [Header("Players")]
    public PlayerStats player1Stats;
    public PlayerStats player2Stats;

    public PlayerController player1Controller;
    public PlayerController player2Controller;

    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    [Header("Input")]
    public PlayerInput player1Input;
    public PlayerInput player2Input;

    [Header("Settings")]
    public float interimDuration = 10f;
    private float timer;
    private bool player1Ready = false;
    private bool player2Ready = false;

    private RoundTimerManager roundTimerManager;

    void Awake()
    {
        roundTimerManager = GetComponent<RoundTimerManager>();

        if (player1Controller == null || player2Controller == null)
            Debug.LogError("Player controller references not assigned in InterimManager!");

        if (player1Input == null || player2Input == null)
            Debug.LogError("PlayerInput references not assigned in InterimManager!");
    }

    void Start()
    {
        interimUI.SetActive(false);
    }

    public void StartInterim()
    {
        // Disable player controls
        player1Controller.EnableUIControls();
        player2Controller.EnableUIControls();

        player1.transform.position = new Vector3(-2, player1.transform.position.y, player1.transform.position.z);
        player2.transform.position = new Vector3(2, player2.transform.position.y, player2.transform.position.z);

        // Show UI & populate stats
        interimUI.SetActive(true);
        skipPromptText.text = "Press Any Button to Skip"; // Initial prompt

        player1StatsText.text = $"Damage: {player1Stats.totalDamageDealt}";
        player2StatsText.text = $"Damage: {player2Stats.totalDamageDealt}";

        var topCombos = player1Stats.GetTopCombos(3);
        string comboText = "Top Combos:\n";
        foreach (var combo in topCombos)
            comboText += $"<b><color=yellow>{combo.Key}</color></b> ({combo.Value}x)\n";
        player1StatsText.text += $"\n{comboText}";

        topCombos = player2Stats.GetTopCombos(3);
        comboText = "Top Combos:\n";
        foreach (var combo in topCombos)
            comboText += $"<b><color=yellow>{combo.Key}</color></b> ({combo.Value}x)\n";
        player2StatsText.text += $"\n{comboText}";

        player1Ready = false;
        player2Ready = false;
        timer = interimDuration;

        // Subscribe to input
        player1Input.actions["Skip"].performed += OnPlayer1Skip;
        player2Input.actions["Skip"].performed += OnPlayer2Skip;
    }

    void Update()
    {
        if (!interimUI.activeSelf) return;

        timer -= Time.deltaTime;
        InterimTimer.fillAmount = timer / interimDuration;

        if ((player1Ready && player2Ready) || timer <= 0f)
        {
            EndInterim();
        }
    }

    private void OnPlayer1Skip(InputAction.CallbackContext context)
    {
        Debug.Log("Player 1 Skip Pressed"); // ADD THIS
        if (!player1Ready)
        {
            player1Ready = true;
            UpdateSkipPrompt();
        }
    }

    private void OnPlayer2Skip(InputAction.CallbackContext context)
    {
        if (!player2Ready)
        {
            player2Ready = true;
            UpdateSkipPrompt();
        }
    }

    void UpdateSkipPrompt()
    {
        string prompt = "";
        if (player1Ready) prompt += "Player 1 Ready\n";
        if (player2Ready) prompt += "Player 2 Ready\n";

        if (player1Ready && player2Ready)
            prompt += ""; // You can add "Starting next round..." if you want
        else
            prompt += "Press Any Button to Skip";

        skipPromptText.text = prompt;
    }

    void EndInterim()
    {
        interimUI.SetActive(false);

        // Reset stats
        player1Stats.ResetStats();
        player2Stats.ResetStats();

        // Enable controls
        player1Controller.EnableGameplayControls();
        player2Controller.EnableGameplayControls();

        // Unsubscribe inputs to prevent memory leaks
        player1Input.actions["Skip"].performed -= OnPlayer1Skip;
        player2Input.actions["Skip"].performed -= OnPlayer2Skip;

        // Start next round
        roundTimerManager.StartRoundTimer();
    }
}
