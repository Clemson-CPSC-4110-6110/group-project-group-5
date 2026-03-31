using UnityEngine;

public class DestroyOnHammerCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("hammer"))
        {
            Debug.Log("Hit by hammer.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Being hit by non-hammer.");
        }
    }
}
