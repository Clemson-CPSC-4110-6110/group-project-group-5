using UnityEngine;

public class IngotToSword : MonoBehaviour
{
    [SerializeField] int requiredHitCount = 20;
    [SerializeField] GameObject visualOfSwordInProgress;
    [SerializeField] GameObject visualOfIngot;
    [SerializeField] Vector3 swordBladeFinalScale;
    [SerializeField] GameObject swordPrefab;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float minHitVelocity = 0.01f;
    [SerializeField] float maxHitVelocity = 1f;
    [SerializeField] float hitCooldown = 0.5f; // cooldown in seconds
    private int currentHitCount = 0;
    private float lastHitTime = 0f;

    void Awake()
    {
        visualOfSwordInProgress.transform.localScale = new(0.01f,0.01f,0.01f);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision collision)
    {
        if (!anvilAttachable.isOnAnvil) return;
        if (!collision.gameObject.CompareTag("hammer")) return;
        Rigidbody hammerRb = collision.rigidbody;
        if (hammerRb == null) return;
        float velocityMagnitude = hammerRb.linearVelocity.magnitude;
        Debug.Log("Velocity Magnitude: " + velocityMagnitude);
        if (velocityMagnitude < minHitVelocity || velocityMagnitude > maxHitVelocity) return;
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;
        HandleHit();
    }

    void HandleHit()
    {
        currentHitCount++;
        float percentHits = Mathf.Max((float)currentHitCount / (float)requiredHitCount, 0.001f);
        visualOfIngot.transform.localScale = new Vector3(1,1,1) * (1f - percentHits);
        Debug.Log("Local scale: " + visualOfIngot.transform.localScale);
        Vector3 visualScale = swordBladeFinalScale * percentHits;
        // visualScale = new(visualScale[0] / transform.localScale[0], visualScale[1] / transform.localScale[1], visualScale[2] / transform.localScale[2]);
        Debug.Log("Visual scale: " + visualScale);
        visualOfSwordInProgress.transform.localScale = visualScale;

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
        Debug.Log("DESTROYING GAME OBJECT");
        // Destroy the current object
        Destroy(gameObject);
    }
}
