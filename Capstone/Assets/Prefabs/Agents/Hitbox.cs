using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Colliders")]
    public Collider leftHitboxCollider;
    public Collider rightHitboxCollider;
    public Collider hurtbox;

    private Animator animator; // Reference to the Animator

    private Dictionary<int, int> punchDamageMap;

    void Start()
    {
        animator = GetComponentInParent<Animator>(); // Get the Animator from the parent object

        punchDamageMap = new Dictionary<int, int>
        {
            { Animator.StringToHash("Jab"), 10 },
            { Animator.StringToHash("Straight"), 15 },
            { Animator.StringToHash("Left_Hook"), 20 },
            { Animator.StringToHash("Right_Hook"), 25 }
        };
    }

    void Update()
    {
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
        Debug.Log("Counter Started!");
    }

    public void StopCounter()
    {
        hurtbox.tag = "Hurtbox";
        Debug.Log("Counter Ended!");
    }

    private void OnTriggerEnter(Collider other)
    {
        Agent agent = other.GetComponentInParent<Agent>();

        AnimatorStateInfo punch = animator.GetCurrentAnimatorStateInfo(0);
        int damage = punchDamageMap.TryGetValue(punch.shortNameHash, out int dmg) ? dmg : 0;

        if (agent != null)
        {
            if (other.CompareTag("Hurtbox"))
            {
                Debug.Log(damage);
                agent.TakeHealthDamage(damage);
            }
            else if (other.CompareTag("Block"))
            {
                agent.ModifyStamina(-damage);
                Debug.Log("Got Hit");
            }
            else if (other.CompareTag("Counter"))
            {
                HandleCounter(other);
            }
        }
    }

    private void HandleCounter(Collider other)
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
            opponentAnimator.SetTrigger("Counter");
        }
        else
        {
            Debug.Log("Counter conditions not met.");
        }
    }

    // ✅ Helper Method to Get Animation State Name from AnimatorStateInfo
    private string GetCurrentStateName(AnimatorStateInfo stateInfo, Animator animator)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (stateInfo.IsName(clip.name))
            {
                return clip.name; // Return the actual animation name
            }
        }
        return "Unknown"; // No match found
    }

}
