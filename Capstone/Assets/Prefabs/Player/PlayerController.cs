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
    private bool blockHeld;
    private Transform enemyTransform;  // Reference to the enemy

    public float moveSpeed = 2f;
    public Transform blockIndicator;
    public GameObject blockLarge;
    public GameObject blockSmall;
    private Animator animator;

    public float counterDuration = 0.5f;
    public float recoveryDuration = 0.5f;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;
    public float health;
    public float stamina;
    private Coroutine staminaRegenCoroutine;

    [Header("UI Elements")]
    public Image healthBar;
    public Image staminaBar;
    public Image staminaDelayBar;

    [Header("Hitboxes")]
    public GameObject leftArmHitbox;
    public GameObject rightArmHitbox;
    public GameObject playerHurtbox;
    public GameObject blockHitbox;

    public float minDistance = 1f;  // Minimum allowed distance between player and enemy
    public float pushbackStrength = 0.5f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemyTransform = GameObject.FindWithTag("Enemy").transform;  // Ensure the enemy is tagged "Enemy"
        health = maxHealth;
        stamina = maxStamina;
        UpdateUI();
    }

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Block.performed += ctx => HoldBlock();
        controls.Player.Block.canceled += ctx => ReleaseBlock();

        controls.Player.CounterEast.performed += _ => TriggerHook(); // Right hook counter
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void Update()
    {
        Move();
        RotateTowardsMidpoint();
        CheckProximity();
    }

    private void RotateTowardsMidpoint()
    {
        if (enemyTransform == null) return;

        Vector3 midpoint = (transform.position + enemyTransform.position) / 2;
        Vector3 direction = (midpoint - transform.position).normalized;
        direction.y = 0;  // Keep the rotation only on the horizontal plane

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);  // Smooth rotation
        }
    }

    private void Move()
    {
        // Convert input into local space direction
        Vector3 localForward = transform.forward;
        Vector3 localRight = transform.right;
        Vector3 movement = (localForward * moveInput.y + localRight * moveInput.x).normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    private void CheckProximity()
    {
        if (enemyTransform == null) return;

        float distance = Vector3.Distance(transform.position, enemyTransform.position);

        if (distance < minDistance)
        {
            Vector3 pushDirection = (transform.position - enemyTransform.position).normalized;
            transform.position = Vector3.Lerp(transform.position, transform.position + pushDirection * pushbackStrength, Time.deltaTime * 10f);
        }
    }

    private void TriggerHook()
    {
        if (isBlocking && !inRecovery)
        {
            StartCoroutine(PerformHookCounter());
        }
        else
        {
            EnableHitbox(rightArmHitbox, counterDuration);
            animator.SetTrigger("Right_Hook");
        }
    }

    private IEnumerator PerformHookCounter()
    {
        isCountering = true;
        inRecovery = true;

        blockLarge.SetActive(false);
        EnableHitbox(rightArmHitbox, counterDuration);
        animator.SetTrigger("Right_Hook");

        yield return new WaitForSeconds(counterDuration);
        yield return new WaitForSeconds(recoveryDuration);

        isCountering = false;
        inRecovery = false;

        if (blockHeld)
        {
            StartBlocking();
        }
    }

    private void HoldBlock()
    {
        blockHeld = true;
        if (!inRecovery)
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
        blockHitbox.SetActive(true);

        // Stop stamina regeneration if it's active
        StopStaminaRegen();
    }

    private void StopBlocking()
    {
        isBlocking = false;
        blockLarge.SetActive(false);
        blockHitbox.SetActive(false);

        // Start stamina regeneration only if stamina is not full
        if (stamina < maxStamina)
        {
            if (staminaRegenCoroutine == null)
            {
                staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
            }
        }
    }

    private void EnableHitbox(GameObject hitbox, float duration)
    {
        hitbox.SetActive(true);
        StartCoroutine(DisableHitbox(hitbox, duration));
    }

    private IEnumerator DisableHitbox(GameObject hitbox, float duration)
    {
        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }

    public void OnHitboxTrigger(string hitboxName, Collider enemyCollider)
    {
        if (hitboxName == "RightArm" && animator.GetCurrentAnimatorStateInfo(0).IsName("Right_Hook"))
        {
            Debug.Log($"Hit Enemy with {hitboxName} during Right Hook!");
            EnemyController enemyController = enemyCollider.GetComponentInParent<EnemyController>();
            enemyController.TakeDamage(25);
        }
    }


    public void TakeHealthDamage(int damage)
    {
        health -= damage;
        health = Mathf.Max(0, health);
        if (health <= 0)
        {
            GameManager.Instance.HandleGameOver(false);
        }
        UpdateUI();
    }

    private IEnumerator RegenerateStamina(float waitTime = 2f)
    {
        yield return new WaitForSeconds(waitTime);
        float regenRate = 75f;

        while (stamina < maxStamina)
        {
            if (isBlocking)
            {
                staminaRegenCoroutine = null;  // Allow regeneration to restart later
                yield break;
            }

            stamina += regenRate * Time.deltaTime;
            stamina = Mathf.Min(stamina, maxStamina);
            UpdateUI();
            yield return null;
        }

        staminaRegenCoroutine = null;  // Reset coroutine reference when full
    }

    private void StopStaminaRegen()
    {
        if (staminaRegenCoroutine != null)
        {
            StopCoroutine(staminaRegenCoroutine);
            staminaRegenCoroutine = null;
        }
    }

    private void UpdateUI()
    {
        healthBar.fillAmount = health / maxHealth;
        staminaBar.fillAmount = stamina / maxStamina;
    }
}
