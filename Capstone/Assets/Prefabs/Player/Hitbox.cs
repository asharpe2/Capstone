using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public string hitboxName;  // "RightArm", "LeftArm", etc.
    public PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            playerController.OnHitboxTrigger(hitboxName, other);
        }
    }
}
