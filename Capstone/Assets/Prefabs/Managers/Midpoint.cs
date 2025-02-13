using UnityEngine;

public class MidpointUpdater : MonoBehaviour
{
    public Transform player;
    public Transform enemy;

    void Update()
    {
        if (player != null && enemy != null)
        {
            transform.position = (player.position + enemy.position) / 2;
        }
    }
}
