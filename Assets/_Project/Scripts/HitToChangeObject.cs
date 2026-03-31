using UnityEngine;

public class HitToChangeObject : MonoBehaviour
{
    [SerializeField] int requiredHitCount = 5;
    [SerializeField] GameObject finalObject;

    private int currentHitCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("hammer"))
        {
            currentHitCount++;
            Debug.Log("Hammer hit object. Current hit count: " + currentHitCount);
            if (currentHitCount == requiredHitCount)
            {
                SpawnReplacementObject();
            }
        }
        else
        {
            Debug.Log("Colliding with non-hammer.");
        }
    }

    private void SpawnReplacementObject()
    {
        // Spawn the replacement object at the same position and rotation
        if (finalObject != null)
        {
            Instantiate(finalObject, transform.position, transform.rotation);
        }

        // Destroy the current object
        Destroy(gameObject);
    }
}
