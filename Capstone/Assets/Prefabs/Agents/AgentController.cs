using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public abstract class Agent : MonoBehaviour
{
    protected Animator animator;
    protected Transform targetTransform; // This could be the enemy or player, depending on the agent
    protected bool isAttacking;
    protected bool isDead;
    protected bool isRegeneratingStamina = false; // Tracks if regen is active
    private Coroutine staminaRegenCoroutine; // Store coroutine reference

    [SerializeField] protected float maxHealth = 100f;
    protected float health;
    [SerializeField] protected float maxStamina = 100f;
    protected float stamina;

    [Header("UI Elements")]
    public Image healthBar;
    public Image staminaBar;
    public Image staminaDelayBar;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
        stamina = maxStamina;
    }

    public virtual void TakeHealthDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Max(0, health);
        OnHealthChanged();

        if (health <= 0)
        {
            OnDeath();
        }
    }

    public virtual void ModifyStamina(float amount)
    {
        stamina += amount; // Use + for regeneration or - for depletion
        stamina = Mathf.Clamp(stamina, 0, maxStamina); // Prevents overfilling or going negative
        OnStaminaChanged();

        // If stamina is fully depleted, stop blocking
        if (stamina <= 0)
        {
            animator.SetBool("isBlocking", false);
        }

        // If stamina was reduced (negative amount), restart regen delay
        if (amount < 0 && stamina < maxStamina)
        {
            if (staminaRegenCoroutine != null)
            {
                StopCoroutine(staminaRegenCoroutine); // Fully stop any running regen coroutine
            }
            staminaRegenCoroutine = StartCoroutine(RegenerateStamina()); // Restart delay countdown
        }
    }


    public void HandleBlocking(bool isBlocking)
    {
        animator.SetBool("isBlocking", isBlocking);

        if (isBlocking)
        {
            // If blocking, stop stamina regeneration
            if (staminaRegenCoroutine != null)
            {
                StopCoroutine(staminaRegenCoroutine);
                staminaRegenCoroutine = null;
                staminaDelayBar.fillAmount = 0; // Reset delay bar UI
            }
        }
        else
        {
            // If stamina is not full, start regen
            if (stamina < maxStamina && staminaRegenCoroutine == null)
            {
                staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
            }
        }
    }

    public void ThrowPunch(string punch, float stamina)
    {
        if (stamina <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName(punch) || animator.IsInTransition(0))
        {
            Debug.Log(stamina);
            Debug.Log("Can't throw!");
            return; // Prevent punch if out of stamina/currently punching/transitioning.
        }
        Debug.Log(stamina);
        animator.SetTrigger(punch); // Play punch animation
        ModifyStamina(-stamina);
    }

    private IEnumerator RegenerateStamina(float waitTime = 2f)
    {
        isRegeneratingStamina = true;

        Debug.Log("Starting Stamina Regen Countdown");

        float timer = 0f;

        // **Wait before starting stamina regeneration**
        while (timer < waitTime)
        {
            if (animator.GetBool("isBlocking")) // Stop countdown if blocking happens again
            {
                staminaDelayBar.fillAmount = 0; // Reset UI
                isRegeneratingStamina = false;
                staminaRegenCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;
            staminaDelayBar.fillAmount = timer / waitTime; // Smoothly fills the bar
            yield return null;
        }

        staminaDelayBar.fillAmount = 0; // Reset delay bar when regen starts

        float regenRate = 50f; // Stamina per second

        // **Regenerate stamina over time**
        while (stamina < maxStamina)
        {
            if (animator.GetBool("isBlocking")) // Stop regen if blocking happens again
            {
                isRegeneratingStamina = false;
                staminaRegenCoroutine = null;
                yield break;
            }

            ModifyStamina(50f * Time.deltaTime);
            stamina = Mathf.Min(stamina, maxStamina);
            UpdateUI();
            yield return null;
        }

        isRegeneratingStamina = false;
        staminaRegenCoroutine = null; // **Ensure coroutine reference resets when finished**
    }

    public void RotateToMidpoint()
    {
        if (targetTransform == null) return;

        Vector3 midpoint = (transform.position + targetTransform.position) / 2;
        Vector3 direction = (midpoint - transform.position).normalized;
        direction.y = 0;  // Keep rotation in the horizontal plane

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    protected virtual void OnHealthChanged()
    {
        UpdateUI();
    }

    protected virtual void OnStaminaChanged()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = health / maxHealth;

        if (staminaBar != null)
            staminaBar.fillAmount = stamina / maxStamina;
    }

    protected abstract void OnDeath();

    protected void PlayAnimation(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    protected virtual void Move(Vector3 direction, float speed)
    {
        float distance = speed * Time.deltaTime;
        float detectionRadius = 0.5f;  // Adjust the detection radius
        Vector3 targetPosition = transform.position + direction * distance;

        // Check for nearby colliders in the detection radius
        Collider[] hitColliders = Physics.OverlapSphere(targetPosition, detectionRadius);

        bool obstacleDetected = false;

        foreach (Collider hit in hitColliders)
        {
            if ((hit.CompareTag("Hurtbox") || (hit.CompareTag("Block"))) && hit.transform.root != transform.root)
            {
                obstacleDetected = true;
                break;  // Exit loop once an obstacle is found
            }
        }

        if (!obstacleDetected)
        {
            transform.position = targetPosition;  // Move if no obstacles detected
        }
    }

    //MODIFY TO STRING FOR COUNTERING LATER
    protected bool IsAnimationPlaying(string animationName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }
}
