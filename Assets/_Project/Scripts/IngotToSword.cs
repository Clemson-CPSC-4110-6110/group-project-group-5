using UnityEngine;

public class IngotToSword : MonoBehaviour
{
    [SerializeField] int requiredHitCount = 5;
    [SerializeField] GameObject visualOfSwordInProgress;
    [SerializeField] Vector3 swordBladeFinalScale;
    [SerializeField] GameObject swordPrefab;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float minHitVelocity = 0.01f;
    [SerializeField] float maxHitVelocity = 1f;

    private int currentHitCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision collision)
    {
        if (!anvilAttachable.isOnAnvil) return;
        if (!collision.gameObject.CompareTag("hammer")) return;
        Rigidbody hammerRb = collision.rigidbody;
        if (hammerRb == null) return;
        float velocityMagnitude = hammerRb.linearVelocity.magnitude;
        if (velocityMagnitude < minHitVelocity || velocityMagnitude > maxHitVelocity) return;

        currentHitCount++;
        Debug.Log("Hammer hit object. Current hit count: " + currentHitCount);
        if (currentHitCount == requiredHitCount)
        {
            SpawnReplacementObject();
        }
    }

    private void SpawnReplacementObject()
    {
        // Spawn the replacement object at the same position and rotation
        if (swordPrefab != null)
        {
            GameObject newSword = Instantiate(swordPrefab, transform.position, transform.rotation);
            newSword.GetComponent<Sword>().SetBladeScale(swordBladeFinalScale);
        }

        // Destroy the current object
        Destroy(gameObject);
    }
}
