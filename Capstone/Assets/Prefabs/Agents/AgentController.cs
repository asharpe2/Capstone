using UnityEngine;
using System.Collections;

public abstract class Agent : MonoBehaviour
{
    protected Animator animator;
    protected Transform targetTransform; // This could be the enemy or player, depending on the agent
    protected bool isBlocking;
    protected bool isAttacking;
    protected bool isDead;

    [SerializeField] protected float maxHealth = 100f;
    protected float health;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
    }

    public virtual void TakeDamage(int damage)
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
        Debug.Log($"{gameObject.name} took damage! Current health: {health}/{maxHealth}");
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
            if (hit.CompareTag("Parry"))  // Check for specific tag (e.g., "Enemy")
            {
                if (hit.CompareTag("Parry") && hit.transform.root != transform.root)
                {
                    Debug.Log("Collision detected with " + hit.transform.parent.gameObject);
                    obstacleDetected = true;
                    break;  // Exit loop once an obstacle is found
                }
            }
        }

        if (!obstacleDetected)
        {
            transform.position = targetPosition;  // Move if no obstacles detected
        }
    }

    protected bool IsAnimationPlaying(string animationName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }
}
