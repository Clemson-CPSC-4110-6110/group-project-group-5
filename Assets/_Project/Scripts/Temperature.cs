using UnityEngine;

public class TemperatureScript : MonoBehaviour
{
    float temperature = 60;
    float minTemperature = 60;
    float maxTemperature = 500;
    float temperatureLostPerSecond = 10;

    [SerializeField] Renderer[] targetRenderers;

    [SerializeField] Color coldColor = new Color(0.4f, 0.5f, 0.6f); // gray-blue
    [SerializeField] Color hotColor = new Color(1f, 0.3f, 0.3f);    // red
    [SerializeField] Color maxHotColor = Color.white;  
    [SerializeField] Color coldEmissionColor = Color.black; 
    [SerializeField] Color hotEmissionColor = Color.red;

    void Awake()
    {
        temperature = minTemperature;
    }

    void Update()
    {
        // Decrease temperature
        temperature -= temperatureLostPerSecond * Time.deltaTime;
        temperature = Mathf.Clamp(temperature, minTemperature, maxTemperature);
        // Debug.Log("Temperature: " + temperature);
        UpdateColor();
    }

    void UpdateColor()
    {
        // Debug.Log("Updating Color");
        // Normalize temperature (0 → 1)
        float t = Mathf.InverseLerp(minTemperature, maxTemperature, temperature);

        // First blend: cold → red
        Color midColor = Color.Lerp(coldColor, hotColor, t);
        Color midEmissionColor = Color.Lerp(coldEmissionColor, hotEmissionColor, t);

        // Optional: push toward white at very high temps
        if (t > 0.8f)
        {
            float whiteBlend = (t - 0.8f) / 0.2f;
            midColor = Color.Lerp(midColor, maxHotColor, whiteBlend);
        }
        
        foreach (Renderer targetRenderer in targetRenderers)
        {
            targetRenderer.material.color = midColor;
            targetRenderer.material.SetColor("_EmissionColor", midEmissionColor);
        }
    }

    public void AddTemperature(float amountAdded)
    {
        temperature += amountAdded;
    }
    public void SetTemperature(float newTemperature)
    {
        temperature = newTemperature;
    }
    public float GetTemperature()
    {
        return temperature;
    }
    public float GetPercentMaxTemperature()
    {
        return temperature / (maxTemperature - minTemperature);
    }
}
