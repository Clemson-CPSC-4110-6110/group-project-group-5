using UnityEngine;

[RequireComponent(typeof(TemperatureScript))]
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
    [SerializeField] TemperatureScript temperatureScript;
    public SmithingMaterial smithingMaterial;
    private float currentHitValue = 0;
    private float lastHitTime = 0f;
    float defaultHandleVolume = 0.02f * 0.02f * 0.2f;
    float defaultBodyVolume = 0.1f * 0.1f * 0.6f;
    float defaultTipVolume = 0.1f * 0.01f * 0.078f;
    float defaultTotalVolume;
    Vector3 volumeScale;

    void Awake()
    {
        defaultTotalVolume = defaultHandleVolume + defaultBodyVolume + defaultTipVolume;
        visualOfSwordInProgress.transform.localScale = new(0.01f,0.01f,0.01f);
        SetVolume(defaultTotalVolume * Random.Range(0.3f,1.2f));
        GetComponent<TemperatureScript>().smithingMaterial = smithingMaterial;
        visualOfIngot.GetComponent<Renderer>().material = smithingMaterial.material;
    }

    void SetVolume(float volume)
    {
        float volumeMultiplier = volume / defaultTotalVolume;
        
        float xAxisScaleMultiplier = Mathf.Pow(volumeMultiplier, 1f / 3f) * Random.Range(0.8f, 1.2f);
        float yAxisScaleMultiplier = Mathf.Pow(volumeMultiplier / xAxisScaleMultiplier, 1f / 2f) * Random.Range(0.8f, 1.2f);
        float zAxisScaleMultiplier = volumeMultiplier / xAxisScaleMultiplier / yAxisScaleMultiplier;
        volumeScale = new(
            transform.localScale[0] * xAxisScaleMultiplier, 
            transform.localScale[1] * yAxisScaleMultiplier, 
            transform.localScale[2] * zAxisScaleMultiplier
        );
        transform.localScale = volumeScale;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision collision)
    {
        if (!anvilAttachable.isOnAnvil) return;
        if (!collision.gameObject.CompareTag("hammer")) return;
        Rigidbody hammerRb = collision.rigidbody;
        if (hammerRb == null) return;
        float velocityMagnitude = hammerRb.linearVelocity.magnitude;
        if (velocityMagnitude < minHitVelocity || velocityMagnitude > maxHitVelocity) return;
        if (Time.time - lastHitTime < hitCooldown) return;
        float hitValue = Mathf.Clamp01( (velocityMagnitude - minHitVelocity) / (maxHitVelocity - minHitVelocity) ) * 3 * temperatureScript.GetPercentMaxTemp();
        Debug.Log("hitValue: " + hitValue);
        lastHitTime = Time.time;
        HandleHit(hitValue);
    }

    void HandleHit(float hitValue)
    {
        currentHitValue += hitValue;
        float percentHits = Mathf.Max(currentHitValue / requiredHitValue, 0.001f);
        visualOfIngot.transform.localScale = new(
            1 - (1 - ingotFinalScale[0]) * percentHits,
            1 - (1 - ingotFinalScale[1]) * percentHits,
            1 - (1 - ingotFinalScale[2]) * percentHits
        );
        Vector3 visualScale = swordBladeFinalScale * percentHits;
        visualOfSwordInProgress.transform.localScale = visualScale;
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
            newSword.GetComponent<Sword>().SetSmithingMaterial(smithingMaterial);
            newSword.GetComponent<TemperatureScript>().SetTemp(GetComponent<TemperatureScript>().GetTemp());
        }
        Debug.Log("DESTROYING GAME OBJECT");
        // Destroy the current object
        Destroy(gameObject);
    }
}
