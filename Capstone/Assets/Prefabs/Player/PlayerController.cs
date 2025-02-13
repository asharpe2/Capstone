using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private PlayerControls controls;
    private Vector2 moveInput;
    private bool isBlocking;
    private bool isCountering;
    private bool inRecovery;
    private bool blockHeld; // Tracks if block is being held

    public float moveSpeed = 5f;

    public Transform blockIndicator;
    public GameObject blockLarge;
    public GameObject blockSmall;
    private Animator animator;

    public float counterDuration = 0.5f; // Parry window duration
    public float recoveryDuration = 0.5f; // Cooldown before blocking can be used again

    public float blockAngleThreshold = 0.1f;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;
    public float health;
    public float stamina;

    [Header("UI Elements")]
    public Image healthBar;
    public Image staminaBar;
    public Image staminaDelayBar;

    private bool isRegeneratingStamina = false;
    private Coroutine staminaRegenCoroutine;

    private BulletSpawner bulletSpawner;
    private BulletSpawnerManager bulletSpawnerManager;

    private void Start()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
        stamina = maxStamina;
        bulletSpawner = FindObjectOfType<BulletSpawner>();
        bulletSpawnerManager = FindObjectOfType<BulletSpawnerManager>();
        UpdateUI();
    }

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Block.performed += ctx => HoldBlock();
        controls.Player.Block.canceled += ctx => ReleaseBlock();

        controls.Player.CounterNorth.performed += _ => AttemptCounter(0);
        controls.Player.CounterSouth.performed += _ => AttemptCounter(180);
        controls.Player.CounterWest.performed += _ => AttemptCounter(-90);

        controls.Player.CounterEast.performed += _ => TriggerHook();  // Replace with hook
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        Move();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))  // Ensure you're in the hook animation state
        {
            other.GetComponent<EnemyController>().TakeDamage(10);
        }
    }


    private void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    private void TriggerHook()
    {
        if (isBlocking && !inRecovery)
        {
            StartCoroutine(PerformHookCounter());  // Hook behaves as a counter while blocking
        }
        else
        {
            animator.SetTrigger("Right_Hook");  // Regular right hook attack
        }
    }

    private IEnumerator PerformHookCounter()
    {
        isCountering = true;
        inRecovery = true;

        blockLarge.SetActive(false);  // Disable block visual
        animator.SetTrigger("Right_Hook");  // Play hook animation as counter

        // Hook counter window (0.5s)
        yield return new WaitForSeconds(counterDuration);

        // Recovery phase (0.5s)
        yield return new WaitForSeconds(recoveryDuration);

        isCountering = false;
        inRecovery = false;

        // Resume blocking if the block button is still held
        if (blockHeld)
        {
            StartBlocking();
        }
    }

    private void HoldBlock()
    {
        blockHeld = true;
        if (!inRecovery) // Only start blocking if we're not in recovery
        {
            StartBlocking();
        }
    }

    private void ReleaseBlock()
    {
        blockHeld = false;
        StopBlocking();
    }

    private void StartBlocking()
    {
        if (stamina <= 0 || inRecovery || isBlocking) return;

        isBlocking = true;
        blockLarge.SetActive(true);

        // Stop stamina regen when blocking starts
        if (staminaRegenCoroutine != null)
        {
            StopCoroutine(staminaRegenCoroutine);
            staminaRegenCoroutine = null;
            staminaDelayBar.fillAmount = 0; // Reset delay UI
        }
    }

    private void StopBlocking()
    {
        if (isCountering) return; // Don't allow stopping block mid-counter
        isBlocking = false;
        blockLarge.SetActive(false);

        // Start stamina regen countdown only if stamina is NOT full
        if (stamina < maxStamina && staminaRegenCoroutine == null)
        {
            staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
        }
    }

    private void AttemptCounter(float zRotation)
    {
        if (!isBlocking || isCountering || inRecovery) return; // Must be blocking, not countering, and not in recovery

        StartCoroutine(PerformCounter(zRotation));
    }

    private IEnumerator PerformCounter(float zRotation)
    {
        isCountering = true;
        isBlocking = false;
        inRecovery = true;

        blockLarge.SetActive(false); // Disable large block
        blockSmall.SetActive(true);

        // Rotate blockSmall on the Z-axis
        blockSmall.transform.rotation = Quaternion.Euler(0, 0, -zRotation - 90);

        // Keep parry active for counterDuration (0.5s)
        yield return new WaitForSeconds(counterDuration);

        blockSmall.SetActive(false); // Disable parry window

        // Recovery phase (0.5s) - Cannot block or counter during this time
        yield return new WaitForSeconds(recoveryDuration);

        isCountering = false;
        inRecovery = false; // Recovery is complete

        // If block button is still held, resume blocking immediately
        if (blockHeld)
        {
            StartBlocking();
        }
        else
        {
            // If block is not held, ensure stamina regen starts like normal
            if (stamina < maxStamina && staminaRegenCoroutine == null)
            {
                staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
            }
        }
    }


    public void OnSuccessfulCounter()
    {
        // Immediately reset counter state and allow blocking again
        isCountering = false;
        inRecovery = false;

        // Make sure both block hitboxes are off
        blockSmall.SetActive(false);
        blockLarge.SetActive(false);

        // Immediately start stamina regeneration if it’s not full
        if (stamina < maxStamina && staminaRegenCoroutine == null)
        {
            staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
        }
    }


    public void TakeHealthDamage(int damage)
    {
        health -= damage;
        health = Mathf.Max(0, health);
        if (health <= 0)
        {
            GameManager.Instance.HandleGameOver(false); // Player loses
        }
        UpdateUI();
    }

    public void RestartGame()
    {
        Debug.Log("Restart Button Clicked!");
        Time.timeScale = 1; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload scene
    }

    public void TakeStaminaDamage(int damage)
    {
        stamina -= damage;
        stamina = Mathf.Max(0, stamina);
        if (stamina <= 0)
        {
            StopBlocking();
        }
        UpdateUI();
    }

    private IEnumerator RegenerateStamina(float waitTime = 2f)
    {
        Debug.Log("Starting Stamina Regen Countdown");

        float timer = 0f;

        // Fill the staminaDelayBar over 2 seconds
        while (timer < waitTime)
        {
            if (isBlocking) // Stop countdown if blocking happens again
            {
                staminaDelayBar.fillAmount = 0; // Reset UI
                yield break;
            }

            timer += Time.deltaTime; // Ensure consistent timing
            staminaDelayBar.fillAmount = timer / waitTime; // Smoothly fills the bar
            yield return null; // Wait for the next frame
        }

        staminaDelayBar.fillAmount = 0; // Reset delay bar when regen starts
        isRegeneratingStamina = true;

        // **Ensure stamina regenerates at a constant rate per second**
        float regenRate = 75f; // Stamina per second (adjust as needed)

        while (stamina < maxStamina)
        {
            if (isBlocking) // Stop regen if blocking happens again
            {
                staminaRegenCoroutine = null;
                yield break;
            }

            stamina += regenRate * Time.deltaTime; // Scales regeneration with frame rate
            stamina = Mathf.Min(stamina, maxStamina); // Clamp to max
            UpdateUI();
            yield return null; // Wait for next frame
        }

        isRegeneratingStamina = false;
        staminaRegenCoroutine = null;
    }


    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = health / maxHealth;

        if (staminaBar != null)
            staminaBar.fillAmount = stamina / maxStamina;

        Debug.Log($"Updated UI → Health: {health}/{maxHealth}, Stamina: {stamina}/{maxStamina}");
    }
}
