using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Collider leftHitboxCollider;
    public Collider rightHitboxCollider;

    public void EnableHitbox(string hitboxName)
    {
        if (hitboxName == "left" && leftHitboxCollider != null)
        {
            leftHitboxCollider.enabled = true;
            Debug.Log("Left hitbox enabled.");
        }
        else if (hitboxName == "right" && rightHitboxCollider != null)
        {
            rightHitboxCollider.enabled = true;
            Debug.Log("Right hitbox enabled.");
        }
        else
        {
            Debug.LogWarning("Invalid hitbox name or collider not assigned.");
        }
    }

    public void DisableHitbox(string hitboxName)
    {
        if (hitboxName == "left" && leftHitboxCollider != null)
        {
            leftHitboxCollider.enabled = false;
            Debug.Log("Left hitbox disabled.");
        }
        else if (hitboxName == "right" && rightHitboxCollider != null)
        {
            rightHitboxCollider.enabled = false;
            Debug.Log("Right hitbox disabled.");
        }
    }
}
