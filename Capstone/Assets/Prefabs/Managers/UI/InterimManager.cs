using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

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

    [SerializeField] public GameObject player1;
    [SerializeField] public GameObject player2;

    [Header("Input")]
    public PlayerInput player1Input;
    public PlayerInput player2Input;

    [Header("Settings")]
    public float interimDuration = 10f;
    private float timer;
    private bool player1Ready = false;
    private bool player2Ready = false;

    [Header("Combos")]
    [SerializeField] private Transform player1ComboContainer;
    [SerializeField] private Transform player2ComboContainer;
    [SerializeField] private GameObject comboRowPrefab;

    private RoundTimerManager roundTimerManager;

    public TMP_Text roundWinnerText; // Assign in Inspector

    [Header("Combo Icons")]
    public Sprite aButtonSprite;
    public Sprite bButtonSprite;
    public Sprite xButtonSprite;
    public Sprite yButtonSprite;

    private Dictionary<string, Sprite> punchSprites;

    void Awake()
    {
        roundTimerManager = GetComponent<RoundTimerManager>();

        if (player1Controller == null || player2Controller == null)
            Debug.LogError("Player controller references not assigned in InterimManager!");

        if (player1Input == null || player2Input == null)
            Debug.LogError("PlayerInput references not assigned in InterimManager!");

        // Map your punch or input string to the correct sprite
        punchSprites = new Dictionary<string, Sprite>()
        {
            { "Left_Hook", aButtonSprite },
            { "Right_Hook", bButtonSprite },
            { "Jab", xButtonSprite },
            { "Straight", yButtonSprite },

        };
    }

    public void StartInterim()
    {
        // Disable player controls
        player1Controller.EnableUIControls();
        player2Controller.EnableUIControls();

        Rigidbody rb1 = player1.GetComponent<Rigidbody>();
        rb1.position = new Vector3(-2, rb1.position.y, rb1.position.z);
        Rigidbody rb2 = player2.GetComponent<Rigidbody>();
        rb2.position = new Vector3(2, rb2.position.y, rb2.position.z);

        // Show UI & populate stats
        interimUI.SetActive(true);
        Debug.Log("Ending Round");
        skipPromptText.text = "Press Any Button to Skip"; // Initial prompt

        player1StatsText.text = $"Damage: {player1Stats.totalDamageDealt}";
        player2StatsText.text = $"Damage: {player2Stats.totalDamageDealt}";

        //var topCombos = player1Stats.GetTopCombos(3);
        string comboText = "Top Combos:\n";
        //foreach (var combo in topCombos)
        //    comboText += $"<b><color=yellow>{combo.Key}</color></b> ({combo.Value}x)\n";
        //player1StatsText.text += $"\n{comboText}";

        //topCombos = player2Stats.GetTopCombos(3);
        comboText = "Top Combos:\n";
        //foreach (var combo in topCombos)
        //    comboText += $"<b><color=yellow>{combo.Key}</color></b> ({combo.Value}x)\n";
        //player2StatsText.text += $"\n{comboText}";

        var topCombosP1 = player1Stats.GetTopCombos(3);
        DisplayCombosWithIcons(topCombosP1, player1ComboContainer, "Player1");

        var topCombosP2 = player2Stats.GetTopCombos(3);
        DisplayCombosWithIcons(topCombosP2, player2ComboContainer, "Player2");

        player1Ready = false;
        player2Ready = false;
        timer = interimDuration;

        // Subscribe to input
        player1Input.actions["Skip"].performed += OnPlayer1Skip;
        player2Input.actions["Skip"].performed += OnPlayer2Skip;
    }

    // Then in StartInterim, after you get topCombos, call something like:
    private void DisplayCombosWithIcons(
        List<KeyValuePair<string, int>> topCombos,
        Transform container,
        string playerName)
    {
        // 1. Clear old children
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // 2. For each combo in topCombos
        foreach (var comboEntry in topCombos)
        {
            string comboKey = comboEntry.Key; // e.g. "Left_Hook -> Right_Hook"
            int timesUsed = comboEntry.Value;

            // Create a "row" for this combo
            GameObject row = Instantiate(comboRowPrefab, container);

            // Split the comboKey back into individual punches
            string[] punches = comboKey.Split(new string[] { " -> " }, System.StringSplitOptions.None);

            foreach (string punch in punches)
            {
                // Create an Image object for the punch icon
                if (punchSprites.TryGetValue(punch, out Sprite punchSprite))
                {
                    GameObject punchImageObj = new GameObject(punch, typeof(UnityEngine.UI.Image));
                    punchImageObj.transform.SetParent(row.transform, false);
                    var punchImage = punchImageObj.GetComponent<UnityEngine.UI.Image>();
                    punchImage.sprite = punchSprite;
                }
                else
                {
                    // If no sprite found for this punch, maybe fallback or skip
                    Debug.LogWarning($"No sprite found for punch key '{punch}'");
                }
            }

            // Add a small text for how many times used
            // Or you can omit if you only want icons
            GameObject countTextObj = new GameObject("ComboCount", typeof(TextMeshProUGUI));
            countTextObj.transform.SetParent(row.transform, false);
            var textComp = countTextObj.GetComponent<TextMeshProUGUI>();
            textComp.text = $" (x{timesUsed})";
        }
    }


    public void ShowRoundWinner(string winnerMessage)
    {
        roundWinnerText.text = winnerMessage;
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

    public void EndInterim()
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
