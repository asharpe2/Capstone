using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Animator animator;
    private float minTime = 1f;  // Minimum time between hooks
    private float maxTime = 3f;  // Maximum time between hooks
    private bool canHook = true;

    private int maxHealth = 100;
    private int health;

    void Start()
    {
        animator = GetComponent<Animator>();
        Invoke("ThrowHook", Random.Range(minTime, maxTime));
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= maxHealth)
        {
            GameManager.Instance.HandleGameOver(true); // Player wins
        }
    }

    void ThrowHook()
    {
        if (canHook)
        {
            animator.SetTrigger("Right_Hook");
            canHook = false;
            Invoke("ResetHook", 1f);  // Adjust for animation length
        }
        Invoke("ThrowHook", Random.Range(minTime, maxTime));  // Schedule next hook
    }

    void ResetHook()
    {
        canHook = true;
    }
}
