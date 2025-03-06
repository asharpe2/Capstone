using UnityEngine;

public class EnemyController : Agent
{
    private bool canAttack = true;
    private float minTime = 1f;
    private float maxTime = 3f;

    protected override void Awake()
    {
        base.Awake();
        targetTransform = GameObject.FindWithTag("Player").transform;
        ScheduleNextAttack();
    }

    private void ScheduleNextAttack()
    {
        if (canAttack)
        {
            Invoke("PerformComboAttack", Random.Range(minTime, maxTime));
        }
    }

    private void PerformComboAttack()
    {
        if (!canAttack) return;

        // Ensure we have a target
        if (targetTransform == null) return;

        // Perform Jab first
        ThrowPunch("Jab", 0f); // Adjust stamina cost as needed

        // Delay the Hook slightly after the Jab
        Invoke("PerformHook", 0.35f); // Adjust delay for animation timing

        // Prevent immediate re-attacking
        canAttack = false;
        Invoke("ResetAttack", 2f); // Adjust time for next attack
    }

    private void PerformHook()
    {
        if (targetTransform == null) return;
        ThrowPunch("Right_Hook", 0f); // Adjust stamina cost as needed
    }

    private void ResetAttack()
    {
        canAttack = true;
        ScheduleNextAttack();
    }

    protected override void OnDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleGameOver(true);
        }
        else
        {
            Debug.LogError("GameManager instance is missing!");
        }
    }
}
