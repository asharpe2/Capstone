using UnityEngine;

public class MidpointUpdater : MonoBehaviour
{
    public Transform player;
    public Transform enemy;

    void Update()
    {
        if (player != null && enemy != null)
        {
            transform.position = new Vector3(((player.position.x + enemy.position.x) / 2), 2f, ((player.position.z + enemy.position.z) / 2));
        }
    }
}
