using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public abstract class Agent : MonoBehaviour
{
    protected Animator animator;
    protected Transform targetTransform; // Enemy reference
    protected bool isAttacking;
    protected bool isDead;
    protected bool isRegeneratingStamina = false;
    private Coroutine staminaRegenCoroutine;

    [SerializeField] protected float maxHealth = 100f;
    protected float health;
    [SerializeField] protected float maxStamina = 100f;
    protected float stamina;

    [Header("UI Elements")]
    public Image healthBar;
    public Image staminaBar;
    public Image staminaDelayBar;

    // IK Variables
    [Header("IK Stuff")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    private Transform punchTarget; // Current punch target position

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
        stamina = maxStamina;
    }

    protected void Update()
    {
        RotateToMidpoint();
    }

    #region IK Handling

    public void SetPunchTarget(Transform enemy, string punchType)
    {
        if (enemy == null)
        {
            Debug.LogError("SetPunchTarget: No enemy assigned!");
            return;
        }

        PunchTarget enemyTarget = enemy.GetComponent<PunchTarget>();
        if (enemyTarget != null)
        {
            Transform rawTarget = enemyTarget.GetTarget(punchType);
            if (rawTarget != null)
            {
                punchTarget = rawTarget;
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        float ikStrength = 0.3f;

        if (animator && punchTarget != null)
        {
            // Get the current animation state info
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Ensure we only apply IK for punch animations
            if (stateInfo.IsTag("Punch")) // Ensure punch animations have the tag "Punch"
            {
                float punchTime = stateInfo.normalizedTime % 1; // Keep value between 0-1

                // Determine which hand is punching
                bool isLeftHandPunch = stateInfo.IsName("Left_Hook") || stateInfo.IsName("Left_Jab"); // Adjust based on animation names
                bool isRightHandPunch = !isLeftHandPunch; // If it's not left, it's right

                // Select the correct hand IK goal and transform
                AvatarIKGoal handGoal = isLeftHandPunch ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
                Transform handTransform = isLeftHandPunch ? leftHand : rightHand;

                // IK Activation Phase (between 30% - 50%)
                if (punchTime >= 0.3f && punchTime <= 0.5f)
                {
                    float impactPhase = Mathf.InverseLerp(0.3f, 0.5f, punchTime); // Smooth blend in
                    float dynamicBlendFactor = Mathf.Lerp(0, ikStrength, impactPhase);
                    ApplyIK(handGoal, handTransform, dynamicBlendFactor);
                }
                // IK Fade Out Phase (between 50% - 70%)
                else if (punchTime > 0.5f && punchTime <= 0.7f)
                {
                    float fadeOutPhase = Mathf.InverseLerp(0.5f, 0.7f, punchTime); // Smooth blend out
                    float dynamicBlendFactor = Mathf.Lerp(ikStrength, 0, fadeOutPhase);
                    ApplyIK(handGoal, handTransform, dynamicBlendFactor);
                }
                // Reset IK outside the punch phase
                else
                {
                    ResetIK();
                }
            }
            else
            {
                ResetIK(); // Ensure IK is reset if not in punch animation
            }
        }
        else
        {
            ResetIK(); // Ensure IK resets if no valid punch target is found
        }
    }

    private void ApplyIK(AvatarIKGoal handGoal, Transform handTransform, float blendFactor)
    {
        animator.SetIKPositionWeight(handGoal, blendFactor);
        animator.SetIKRotationWeight(handGoal, blendFactor);

        animator.SetIKPosition(handGoal, punchTarget.position);
        animator.SetIKRotation(handGoal, punchTarget.rotation);
    }

    // Helper function to reset IK
    private void ResetIK()
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
    }

    #endregion

    #region Health/Stamina

    public virtual void TakeHealthDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Max(0, health);
        OnHealthChanged();

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) animator.SetTrigger("Hit");


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

    private IEnumerator RegenerateStamina(float waitTime = 2f)
    {
        isRegeneratingStamina = true;

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

        // **Regenerate stamina over time**
        while (stamina < maxStamina)
        {
            if (animator.GetBool("isBlocking")) // Stop regen if blocking happens again
            {
                isRegeneratingStamina = false;
                staminaRegenCoroutine = null;
                yield break;
            }

            ModifyStamina(50f * Time.deltaTime); //Stamina Regened per second
            stamina = Mathf.Min(stamina, maxStamina);
            UpdateUI();
            yield return null;
        }

        isRegeneratingStamina = false;
        staminaRegenCoroutine = null; // **Ensure coroutine reference resets when finished**
    }

    protected virtual void OnHealthChanged()
    {
        UpdateUI();
    }

    protected virtual void OnStaminaChanged()
    {
        UpdateUI();
    }

    #endregion

    #region Movement

    //TODO: Fix movement into walls, not needed for vertical slice.
    protected virtual void Move(Vector3 direction, float speed)
    {
        // Use input direction directly for animation (not affected by Time.deltaTime)
        Vector3 localInput = transform.InverseTransformDirection(direction);
        animator.SetFloat("MoveX", localInput.x);
        animator.SetFloat("MoveY", localInput.z);
        if (direction == Vector3.zero) return;

        float distance = speed * Time.deltaTime; // Normal movement distance
        Vector3 proposedPosition = transform.position + direction.normalized * distance;
        float detectionRadius = 1f; // How close an obstacle needs to be to stop movement

        // Check for obstacles in the movement direction
        Collider[] hitColliders = Physics.OverlapSphere(proposedPosition, detectionRadius);

        Vector3 adjustedDirection = direction.normalized; // Start with full movement

        foreach (Collider hit in hitColliders)
        {
            // Ignore self and non-relevant colliders
            if (hit.transform == transform || (!hit.CompareTag("Hurtbox") && !hit.CompareTag("Block") && !hit.CompareTag("Counter") && !hit.CompareTag("Wall")))
                continue;

            // Get direction from player to obstacle
            Vector3 obstacleDirection = (hit.transform.position - transform.position).normalized;

            // Project movement direction onto the obstacle direction
            float dotProduct = Vector3.Dot(direction.normalized, obstacleDirection);

            // If the obstacle is in front, reduce movement in that direction
            if (dotProduct > 0)
            {
                adjustedDirection -= obstacleDirection * dotProduct; // Remove blocked direction
            }
        }

        // Normalize to keep speed consistent (prevent slower diagonal movement)
        Vector3 finalMovement = new Vector3 ((direction.x * distance), 0f, (direction.z * distance));
        transform.position += finalMovement;
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

    #endregion

    #region Fighting

    public void ThrowPunch(string punch, float cost)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(punch) || animator.IsInTransition(0))
            return; // Prevents spamming punches

        else if (animator.GetBool("isBlocking"))
        {
            animator.SetTrigger(punch);
        }
        else if (stamina > cost)
        {
            animator.SetTrigger(punch); // Play punch animation
            ModifyStamina(-cost);

            if (targetTransform != null)
            {
                SetPunchTarget(targetTransform, punch);
            }
        }
    }


    public void HandleBlocking(bool isBlocking)
    {
        animator.SetBool("isBlocking", isBlocking);

        if (isBlocking)
        {
            Debug.Log("Blocking Started");
            // Stop stamina regeneration
            if (staminaRegenCoroutine != null)
            {
                StopCoroutine(staminaRegenCoroutine);
                staminaRegenCoroutine = null;
                staminaDelayBar.fillAmount = 0;
            }
        }
        else
        {
            Debug.Log("Blocking Stopped");
            if (stamina < maxStamina && staminaRegenCoroutine == null)
            {
                staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
            }
        }
    }


    #endregion

    #region UI

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

    #endregion
}
