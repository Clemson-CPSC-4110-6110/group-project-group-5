using UnityEngine;

public class FollowingText : MonoBehaviour
{
    [SerializeField] Transform player;
    void Update()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // ignore vertical difference

        transform.rotation = Quaternion.LookRotation(direction);
    }
}
