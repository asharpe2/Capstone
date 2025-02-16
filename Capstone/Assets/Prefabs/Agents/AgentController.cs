using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 100f;
    protected float health;
    protected Animator animator;

    protected virtual void Start()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
    }

    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        health = Mathf.Max(0, health);
        if (health <= 0)
        {
            Die();
        }
    }

    protected abstract void Die();  // Each child class implements its own death behavior

    public void TriggerHook()
    {
        animator.SetTrigger("Right_Hook");
    }
}
