using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Colliders")]
    public Collider leftHitboxCollider;
    public Collider rightHitboxCollider;
    public Collider hurtbox;

    [Header("Damage Settings")]
    public int hookDamage = 25; // Damage dealt on hook

    private Animator animator; // Reference to the Animator

    void Start()
    {
        animator = GetComponentInParent<Animator>(); // Get the Animator from the parent object
    }

    void Update()
    {
        // Check if the player is blocking and update the hurtbox tag
        if (animator.GetBool("isBlocking"))
        {
            hurtbox.tag = "Block";
        }
        else
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

    private void OnTriggerEnter(Collider other)
    {
        Agent agent = other.GetComponentInParent<Agent>();

        if (agent != null)
        {
            if (other.CompareTag("Hurtbox"))
            {   // Apply damage to the agent
                agent.TakeHealthDamage(hookDamage);
            }
            else if (other.CompareTag("Block"))
            {
                // Apply stamina damage to the agent
                agent.ModifyStamina(-hookDamage);
            }
            else return;
        }
        
    }
}
