using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Colliders")]
    public Collider leftHitboxCollider;
    public Collider rightHitboxCollider;

    [Header("Damage Settings")]
    public int hookDamage = 25; // Damage dealt on hook

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
        // Attempt to get the AgentController component from the collided object
        if (!other.CompareTag("Parry")) return;

        Agent agent = other.GetComponentInParent<Agent>();
        Debug.Log(agent);
        
        if (agent != null)
        {
            // Apply damage to the agent
            agent.TakeDamage(hookDamage);
        }
            
        
    }
}
