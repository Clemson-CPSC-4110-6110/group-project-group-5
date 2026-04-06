using UnityEngine;

public class IngotToSword : MonoBehaviour
{
    [SerializeField] float requiredHitValue = 20f;
    [SerializeField] GameObject visualOfSwordInProgress;
    [SerializeField] GameObject visualOfIngot;
    [SerializeField] Vector3 swordBladeFinalScale;
    [SerializeField] Vector3 ingotFinalScale = new(0.2f, 0.2f, 0.2f);
    [SerializeField] GameObject swordPrefab;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float minHitVelocity = 0.01f;
    [SerializeField] float maxHitVelocity = 1f;
    [SerializeField] float hitCooldown = 0.5f; // cooldown in seconds
    [SerializeField] Vector3 defaultSwordBladeBodySize;
    [SerializeField] Vector3 defaultSwordHandleSize;
    [SerializeField] Vector3 defaultSwordTipSize;
    [SerializeField] TemperatureScript temperatureScript;
    private float currentHitValue = 0;
    private float lastHitTime = 0f;

    void Awake()
    {
        visualOfSwordInProgress.transform.localScale = new(0.01f,0.01f,0.01f);
        // TEMPORARY
        // SetVolume(0.00555075796059f * Random.Range(0.8f,1.2f));
    }

    void SetVolume(float volume)
    {
        float defaultHandleVolume = defaultSwordHandleSize[0] * defaultSwordHandleSize[1] * defaultSwordHandleSize[2];
        float defaultBodyVolume = defaultSwordBladeBodySize[0] * defaultSwordBladeBodySize[1] * defaultSwordBladeBodySize[2];
        float defaultTipVolume = defaultSwordTipSize[0] * defaultSwordTipSize[1] * defaultSwordTipSize[2];
        float defaultTotalVolume = defaultHandleVolume + defaultBodyVolume + defaultTipVolume;
        float volumeMultiplier = volume / defaultTotalVolume;
        
        float xAxisScaleMultiplier = Mathf.Pow(volumeMultiplier, 1f / 3f) * Random.Range(0.8f, 1.2f);
        float yAxisScaleMultiplier = Mathf.Pow(volumeMultiplier / xAxisScaleMultiplier, 1f / 2f) * Random.Range(0.8f, 1.2f);
        float zAxisScaleMultiplier = volumeMultiplier / xAxisScaleMultiplier / yAxisScaleMultiplier;
        transform.localScale = new(
            transform.localScale[0] * xAxisScaleMultiplier, 
            transform.localScale[1] * yAxisScaleMultiplier, 
            transform.localScale[2] * zAxisScaleMultiplier
        );
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision collision)
    {
        if (!anvilAttachable.isOnAnvil) return;
        if (!collision.gameObject.CompareTag("hammer")) return;
        Rigidbody hammerRb = collision.rigidbody;
        if (hammerRb == null) return;
        float velocityMagnitude = hammerRb.linearVelocity.magnitude;
        // Debug.Log("Velocity Magnitude: " + velocityMagnitude);
        if (velocityMagnitude < minHitVelocity || velocityMagnitude > maxHitVelocity) return;
        if (Time.time - lastHitTime < hitCooldown) return;
        float hitValue = Mathf.Clamp01( (velocityMagnitude - minHitVelocity) / (maxHitVelocity - minHitVelocity) ) * 3 * temperatureScript.GetPercentMaxTemperature();
        Debug.Log("hitValue: " + hitValue);
        lastHitTime = Time.time;
        HandleHit(hitValue);
    }

    void HandleHit(float hitValue)
    {
        currentHitValue += hitValue;
        float percentHits = Mathf.Max((float)currentHitValue / (float)requiredHitValue, 0.001f);
        // visualOfIngot.transform.localScale = new Vector3(1,1,1) * (1f - percentHits);
        visualOfIngot.transform.localScale = new(
            1 - (1 - ingotFinalScale[0]) * percentHits,
            1 - (1 - ingotFinalScale[1]) * percentHits,
            1 - (1 - ingotFinalScale[2]) * percentHits
        );
        // Debug.Log("Local scale: " + visualOfIngot.transform.localScale);
        Vector3 visualScale = swordBladeFinalScale * percentHits;
        // visualScale = new(visualScale[0] / transform.localScale[0], visualScale[1] / transform.localScale[1], visualScale[2] / transform.localScale[2]);
        // Debug.Log("Visual scale: " + visualScale);
        visualOfSwordInProgress.transform.localScale = visualScale;

        // Debug.Log("Hammer hit object. Current hit count: " + currentHitValue);
        if (currentHitValue >= requiredHitValue)
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
            newSword.GetComponent<TemperatureScript>().SetTemperature(GetComponent<TemperatureScript>().GetTemperature());
            // 0.00555075796059
        }
        Debug.Log("DESTROYING GAME OBJECT");
        // Destroy the current object
        Destroy(gameObject);
    }
}
