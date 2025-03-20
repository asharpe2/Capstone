using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using FMODUnity;

public abstract class Agent : MonoBehaviour
{
    #region Inspector

    [SerializeField] protected Transform targetTransform; // Enemy reference
    protected Animator animator;
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

    [Header("Hitbox Colliders")]
    public Collider leftHitboxCollider;
    public Collider rightHitboxCollider;
    public Collider hurtbox;

    [Header("Particle Effects")]
    [SerializeField] public GameObject hitEffect;
    [SerializeField] public GameObject blockEffect;
    [SerializeField] public GameObject counterEffect;

    [Header("Punch Sound Effects")]
    [SerializeField] private EventReference punchSound1;

    [Header("Block Sound Effects")]
    [SerializeField] private EventReference blockSound1;

    [Header("Counter Sound Effects")]
    [SerializeField] private EventReference counterSound1;

    private Dictionary<int, int> punchDamageMap;

    #endregion

    #region Unity Events

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
        stamina = maxStamina;
    }

    void Start()
    {
        punchDamageMap = new Dictionary<int, int>
        {
        { Animator.StringToHash("Jab"), 1 }, { Animator.StringToHash("Jab_Counter"), 10 },
        { Animator.StringToHash("Straight"), 3 }, { Animator.StringToHash("Straight_Counter"), 15 },
        { Animator.StringToHash("Left_Hook"), 5 }, { Animator.StringToHash("Left_Hook_Counter"), 25 },
        { Animator.StringToHash("Right_Hook"), 5 }, { Animator.StringToHash("Right_Hook_Counter"), 25 }
        };
        animator.Rebind(); // ✅ Ensures Animator properly resets
        animator.Update(0f); // ✅ Forces an immediate update
        animator.Play("Idle", 0, 0f);
    }

    protected void Update()
    {
        RotateToMidpoint();
        // Check if blocking and update hurtbox tag
        if (animator.GetBool("isBlocking"))
        {
            hurtbox.tag = "Block";
        }
        else if (hurtbox.tag != "Counter")
        {
            hurtbox.tag = "Hurtbox";
        }
    }

    #endregion

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
        float ikStrength = 0f;

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

        if (damage >= 10)
        {
            animator.SetTrigger("Big_Hit");
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) animator.SetTrigger("Hit");
        


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

    //TODO: Movement can break when both players move directly at each other
    protected virtual void Move(Vector3 direction, float speed)
    {
        if (GetCurrentStateName(animator.GetCurrentAnimatorStateInfo(0), animator) == "Block")
        {
            return;
        }

        // Convert world-space direction to local space for animation
        direction = new Vector3(direction.x, 0f, 0f);
        Vector3 localInput = transform.InverseTransformDirection(direction);

        // Use input magnitude to allow partial movement (e.g., controller sensitivity)
        float inputMagnitude = Mathf.Clamp01(new Vector2(localInput.x, localInput.z).magnitude);

        //animator.SetFloat("MoveX", localInput.x);
        animator.SetFloat("MoveY", localInput.z);

        // Scale movement speed by input magnitude (for variable movement)
        float adjustedSpeed = speed * inputMagnitude;
        Vector3 right = new Vector3(0, 0, 1);
        Vector3 moveDirection = right * direction.z;
        float distance = adjustedSpeed * Time.deltaTime;

        Vector3 proposedPosition = transform.position + direction.normalized * distance;
        float detectionRadius = 0.5f; // Radius for obstacle detection

        RaycastHit hit;
        bool hitWall = false;
        Vector3 adjustedDirection = direction;

        // Primary raycast to check for obstacles in the movement direction
        if (Physics.CapsuleCast(transform.position, transform.position + Vector3.up * 2f, detectionRadius, direction.normalized, out hit, distance))
        {
            if (hit.collider.CompareTag("Hurtbox") || hit.collider.CompareTag("Block") || hit.collider.CompareTag("Counter") || hit.collider.CompareTag("Wall"))
            {
                hitWall = true;
                Vector3 obstacleNormal = hit.normal;
                adjustedDirection = Vector3.ProjectOnPlane(direction, obstacleNormal); // Slide along the surface
            }
        }

        // SECOND CHECK: Prevents sliding into corners
        if (hitWall && Physics.CapsuleCast(transform.position, transform.position + Vector3.up * 2f, detectionRadius, adjustedDirection.normalized, out hit, distance))
        {
            // If another wall is detected right after adjusting, stop movement
            adjustedDirection = Vector3.zero;
        }

        // Scale final movement by input magnitude
        adjustedDirection = adjustedDirection.normalized * adjustedSpeed * Time.deltaTime;
        adjustedDirection.y = 0; // Prevent unintended vertical movement

        transform.position += adjustedDirection;
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
        float comboStart = 0.3f;
        float comboEnd = 0.7f;
        PlayerStats playerStats = GetComponent<PlayerStats>();

        playerStats.AddPunchToCombo(punch);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (animator.IsInTransition(0))
            return; // Prevents spamming punches

        else if (animator.GetBool("isBlocking"))
        {
            animator.SetTrigger(punch);
        }
        else if (stamina > cost)
        {
            if (stateInfo.IsTag("Idle"))
            {
                animator.SetTrigger(punch); // Play punch animation
                ModifyStamina(-cost);
                playerStats.FinalizeCombo();
            }
            else if (stateInfo.IsTag("Punch")) // Ensure we're in a punch animation
            {
                float punchProgress = stateInfo.normalizedTime % 1; // Keep it within 0-1 range

                if (punchProgress >= comboStart && punchProgress <= comboEnd)
                {
                    animator.SetTrigger(punch); // Play punch animation
                    StartCoroutine(ClearTriggerIfNotUsed(punch, cost));
                }

            }
            if (targetTransform != null)
            {
                SetPunchTarget(targetTransform, punch);
            }
        }
    }

    private IEnumerator ClearTriggerIfNotUsed(string triggerName, float cost)
    {
        yield return null; // Wait for the next frame

        animator.ResetTrigger(triggerName);

        // If still in the same animation (didn't transition), reset the trigger
        if (animator.GetAnimatorTransitionInfo(0).duration > 0)
        {
            ModifyStamina(-cost);
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

    #region Hitboxes

    public void EnableHitbox(string hitboxName)
    {
        if (hitboxName == "left" && leftHitboxCollider != null)
        {
            leftHitboxCollider.enabled = true;
        }
        else if (hitboxName == "right" && rightHitboxCollider != null)
        {
            rightHitboxCollider.enabled = true;
        }
    }

    public void DisableHitbox(string hitboxName)
    {
        if (hitboxName == "left" && leftHitboxCollider != null)
        {
            leftHitboxCollider.enabled = false;
        }
        else if (hitboxName == "right" && rightHitboxCollider != null)
        {
            rightHitboxCollider.enabled = false;
        }
    }

    public void StartCounter()
    {
        animator.SetBool("isBlocking", false);
        hurtbox.tag = "Counter";
    }

    public void StopCounter()
    {
        hurtbox.tag = "Hurtbox";
    }

    #region Particles

    private void PlayParticleEffect(GameObject particlePrefab, Vector3 position)
    {
        if (particlePrefab == null) return;

        GameObject particleInstance = Instantiate(particlePrefab, position, Quaternion.identity);
        Destroy(particleInstance, 2f); // Cleanup after 2 seconds
    }

    #endregion

    private void OnTriggerStay(Collider other)
    {
        Agent agent = other.GetComponentInParent<Agent>();

        AnimatorStateInfo punch = animator.GetCurrentAnimatorStateInfo(0);
        int damage = punchDamageMap.TryGetValue(punch.shortNameHash, out int dmg) ? dmg : 0;

        if (agent != null)
        {
            //Set hand for particles/sounds later
            Vector3 particleTransform = new Vector3(0, 0, 0);
            if (leftHitboxCollider.enabled) particleTransform = leftHand.position;
            else particleTransform = rightHand.position;

            if (other.CompareTag("Hurtbox"))
            {
                PlayParticleEffect(hitEffect, particleTransform);
                agent.TakeHealthDamage(damage);
                AudioManager.instance.PlayOneShot(punchSound1, particleTransform);
                GetComponent<PlayerStats>().AddDamage(damage);

                //Just in case the hit missed
                AudioManager.instance.musicInstance.setParameterByName("Music_Fade", 1);
            }
            else if (other.CompareTag("Block"))
            {
                PlayParticleEffect(blockEffect, particleTransform);
                agent.ModifyStamina(-damage);
                AudioManager.instance.PlayOneShot(blockSound1, particleTransform);
            }
            else if (other.CompareTag("Counter"))
            {
                agent.ModifyStamina(-20f);
                HandleCounter(other, damage);
            }
            leftHitboxCollider.enabled = false;
            rightHitboxCollider.enabled = false;
        }
    }

    private void HandleCounter(Collider other, int damage)
    {
        // Find the root object of the hitbox
        Transform rootTransform = other.transform.root;

        // Find the Agent script in the root object
        Agent opponent = rootTransform.GetComponent<Agent>();

        // Ensure opponent and animators exist
        if (opponent == null) return;
        Animator opponentAnimator = opponent.GetComponent<Animator>();

        if (opponentAnimator == null || animator == null) return;

        // THIS IS MAGIC, I KNOW IT LOOKS BACKWARDS BUT IT ISN'T
        AnimatorStateInfo opponentState = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo selfState = opponentAnimator.GetCurrentAnimatorStateInfo(0);
        
        // Retrieve actual animation names for debugging (FIX: Ensure correct mapping)
        string selfAnimationName = GetCurrentStateName(selfState, animator);
        string opponentAnimationName = GetCurrentStateName(opponentState, opponentAnimator);

        // Build the expected counter state name
        string expectedCounterState = opponentAnimationName + "_Counter_Windup";

        // Check if self animation is the expected counter state
        bool counter = selfState.IsName(expectedCounterState);

        if (counter)
        {
            Debug.Log($"{opponent.gameObject.name} was countered!");

            // Play Counter Execution Animation
            opponent.ThrowPunch("Counter", -40f);
            PlayParticleEffect(counterEffect, transform.position + transform.forward * 0.5f);
            AudioManager.instance.PlayOneShot(counterSound1, opponent.transform.position);
            StartCoroutine(CounterSlowdownEffect());
        }
        else
        {
            Debug.Log("Counter conditions not met.");
            Debug.Log("Enemy State = " + opponentAnimationName);
            Debug.Log("Self State = " + selfAnimationName);
            opponent.TakeHealthDamage(damage);
            PlayParticleEffect(hitEffect, rootTransform.position);
            AudioManager.instance.PlayOneShot(punchSound1, rootTransform.position);
        }
    }

    private string GetCurrentStateName(AnimatorStateInfo stateInfo, Animator animator)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (stateInfo.shortNameHash == Animator.StringToHash(clip.name))
            {
                return clip.name;
            }
        }
        return "Unknown";
    }

    private IEnumerator CounterSlowdownEffect()
    {
        float slowdownFactor = 0.1f; // Slow down to 20% speed
        float pauseDuration = 0.3f; // How long the game "freezes" before impact
        float fadeDuration = 0.3f; // How long music takes to fade out

        // ✅ Step 1: Slow down time smoothly
        float originalTimeScale = Time.timeScale;
        AudioManager.instance.musicInstance.setParameterByName("Music_Fade", 0);
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            Time.timeScale = Mathf.Lerp(originalTimeScale, slowdownFactor, t / fadeDuration);
            yield return null;
        }

        Time.timeScale = slowdownFactor;

        // ✅ Step 2: Pause momentarily before impact
        yield return new WaitForSecondsRealtime(pauseDuration);

        Time.timeScale = originalTimeScale;
        AudioManager.instance.musicInstance.setParameterByName("Music_Fade", 1);
    }


    #endregion
}