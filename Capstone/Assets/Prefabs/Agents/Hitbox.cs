using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Colliders")]
    public Collider leftHitboxCollider;
    public Collider rightHitboxCollider;
    public Collider hurtbox;

    [Header("Particle Effects")]
    [SerializeField] public GameObject hitEffect;
    [SerializeField] public GameObject blockEffect;
    [SerializeField] public GameObject counterEffect;
    [SerializeField] public Transform leftHandTransform;
    [SerializeField] public Transform rightHandTransform;

    [Header("Sound Effects")]
    [SerializeField] private EventReference punchSound1;

    private Animator animator; // Reference to the Animator

    private Dictionary<int, int> punchDamageMap;

    void Start()
    {
        animator = GetComponentInParent<Animator>(); // Get the Animator from the parent object

        punchDamageMap = new Dictionary<int, int>
        {
            { Animator.StringToHash("Jab"), 5 }, { Animator.StringToHash("Jab_Counter"), 10 },
            { Animator.StringToHash("Straight"), 15 }, { Animator.StringToHash("Straight_Counter"), 25 },
            { Animator.StringToHash("Left_Hook"), 20 }, { Animator.StringToHash("Left_Hook_Counter"), 40 },
            { Animator.StringToHash("Right_Hook"), 25 }, { Animator.StringToHash("Right_Hook_Counter"), 50 }
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

    #region Particles

    private void PlayParticleEffect(GameObject particlePrefab, Vector3 position)
    {
        if (particlePrefab == null) return;

        GameObject particleInstance = Instantiate(particlePrefab, position, Quaternion.identity);
        Destroy(particleInstance, 2f); // Cleanup after 2 seconds
    }


    #endregion

    private void OnTriggerEnter(Collider other)
    {
        Agent agent = other.GetComponentInParent<Agent>();

        AnimatorStateInfo punch = animator.GetCurrentAnimatorStateInfo(0);
        int damage = punchDamageMap.TryGetValue(punch.shortNameHash, out int dmg) ? dmg : 0;

        if (agent != null)
        {
            if (other.CompareTag("Hurtbox"))
            {
                //Set hand for particles/sounds later
                Vector3 particleTransform = new Vector3(0, 0, 0);
                if (leftHitboxCollider.enabled) particleTransform = leftHandTransform.position;
                else particleTransform = leftHandTransform.position;


                agent.TakeHealthDamage(damage);
                PlayParticleEffect(hitEffect, particleTransform);
                AudioManager.instance.PlayOneShot(punchSound1, particleTransform);
            }
            else if (other.CompareTag("Block"))
            {
                agent.ModifyStamina(-damage);
                Debug.Log("Got Hit");
                PlayParticleEffect(blockEffect, transform.position + transform.forward * 0.5f);
            }
            else if (other.CompareTag("Counter"))
            {
                HandleCounter(other, damage);
                agent.ModifyStamina(-20f);
            }
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
            opponent.ThrowPunch("Counter", 20f);
            PlayParticleEffect(counterEffect, transform.position + transform.forward * 0.5f);
        }
        else
        {
            Debug.Log("Counter conditions not met.");
            opponent.TakeHealthDamage(damage);
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
